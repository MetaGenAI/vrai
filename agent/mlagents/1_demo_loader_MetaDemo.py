import logging
import os
print("hi")
import sys
from typing import List, Tuple
import numpy as np
from mlagents.trainers.buffer import AgentBuffer
from mlagents.trainers.brain import BrainParameters
from mlagents.trainers.brain_conversion_utils import group_spec_to_brain_parameters
from mlagents_envs.communicator_objects.agent_info_action_pair_pb2 import (
    AgentInfoActionPairProto,
)
from mlagents.trainers.trajectory import SplitObservations
from mlagents_envs.rpc_utils import (
    agent_group_spec_from_proto,
    batched_step_result_from_proto,
)
from mlagents_envs.base_env import AgentGroupSpec
from mlagents_envs.communicator_objects.brain_parameters_pb2 import BrainParametersProto
from mlagents_envs.communicator_objects.demonstration_meta_pb2 import (
    DemonstrationMetaProto,
)
from mlagents_envs.timers import timed, hierarchical_timer
from google.protobuf.internal.decoder import _DecodeVarint32  # type: ignore

from mlagents_envs.environment import UnityEnvironment
#env = UnityEnvironment(file_name="../Builds/Basic.x86_64", base_port=5005, seed=1, side_channels=[])
#group_spec, info_action_pairs, total_expected = load_demonstration("Demonstrations/TestStill5.demo")
#print([group_spec, info_action_pairs, total_expected])


logger = logging.getLogger("mlagents.trainers")

@timed

def make_demo_buffer(
    pair_infos: List[AgentInfoActionPairProto],
    group_spec: AgentGroupSpec,
    sequence_length: int,
) -> AgentBuffer:
    # Create and populate buffer using experiences
    demo_raw_buffer = AgentBuffer()
    demo_processed_buffer = AgentBuffer()
    for idx, current_pair_info in enumerate(pair_infos):
        if idx > len(pair_infos) - 2:
            break
        next_pair_info = pair_infos[idx + 1]
        current_step_info = batched_step_result_from_proto(
            [current_pair_info.agent_info], group_spec
        )
        next_step_info = batched_step_result_from_proto(
            [next_pair_info.agent_info], group_spec
        )
        previous_action = (
            np.array(pair_infos[idx].action_info.vector_actions, dtype=np.float32) * 0
        )
        if idx > 0:
            previous_action = np.array(
                pair_infos[idx - 1].action_info.vector_actions, dtype=np.float32
            )
        agent_id = current_step_info.agent_id[0]
        current_agent_step_info = current_step_info.get_agent_step_result(agent_id)
        next_agent_step_info = next_step_info.get_agent_step_result(agent_id)

        demo_raw_buffer["done"].append(next_agent_step_info.done)
        demo_raw_buffer["rewards"].append(next_agent_step_info.reward)
        split_obs = SplitObservations.from_observations(current_agent_step_info.obs)
        for i, obs in enumerate(split_obs.visual_observations):
            demo_raw_buffer["visual_obs%d" % i].append(obs)
        demo_raw_buffer["vector_obs"].append(split_obs.vector_observations)
        demo_raw_buffer["actions"].append(current_pair_info.action_info.vector_actions)
        demo_raw_buffer["prev_action"].append(previous_action)
        if next_step_info.done:
            demo_raw_buffer.resequence_and_append(
                demo_processed_buffer, batch_size=None, training_length=sequence_length
            )
            demo_raw_buffer.reset_agent()
    demo_raw_buffer.resequence_and_append(
        demo_processed_buffer, batch_size=None, training_length=sequence_length
    )
    return demo_processed_buffer


@timed
def demo_to_buffer(
    file_path: str, sequence_length: int
) -> Tuple[BrainParameters, AgentBuffer]:
    """
    Loads demonstration file and uses it to fill training buffer.
    :param file_path: Location of demonstration file (.demo).
    :param sequence_length: Length of trajectories to fill buffer.
    :return:
    """
    group_spec, info_action_pair, _ = load_demonstration(file_path)
    demo_buffer = make_demo_buffer(info_action_pair, group_spec, sequence_length)
    brain_params = group_spec_to_brain_parameters("DemoBrain", group_spec)
    return brain_params, demo_buffer


def get_demo_files(path: str) -> List[str]:
    """
    Retrieves the demonstration file(s) from a path.
    :param path: Path of demonstration file or directory.
    :return: List of demonstration files
    Raises errors if |path| is invalid.
    """
    if os.path.isfile(path):
        if not path.endswith(".demo"):
            raise ValueError("The path provided is not a '.demo' file.")
        return [path]
    elif os.path.isdir(path):
        paths = [
            os.path.join(path, name)
            for name in os.listdir(path)
            if name.endswith(".demo")
        ]
        if not paths:
            raise ValueError("There are no '.demo' files in the provided directory.")
        return paths
    else:
        raise FileNotFoundError(
            f"The demonstration file or directory {path} does not exist."
        )


@timed
def load_demonstration(
    file_path: str
) -> Tuple[BrainParameters, List[AgentInfoActionPairProto], int]:
    """
    Loads and parses a demonstration file.
    :param file_path: Location of demonstration file (.demo).
    :return: BrainParameter and list of AgentInfoActionPairProto containing demonstration data.
    """

    # First 32 bytes of file dedicated to meta-data.
    INITIAL_POS = 33
    file_paths = get_demo_files(file_path)
    group_spec = None
    brain_param_proto = None
    info_action_pairs = []
    total_expected = 0
    for _file_path in file_paths:
        with open(_file_path, "rb") as fp:
            with hierarchical_timer("read_file"):
                data = fp.read()
            #print(data)
            next_pos, pos, obs_decoded = 0, 0, 0
            #print(len(data))
            while pos < len(data):
                next_pos, pos = _DecodeVarint32(data, pos)
                # print([pos, next_pos, obs_decoded])
                if obs_decoded == 0:
                    meta_data_proto = DemonstrationMetaProto()
                    meta_data_proto.ParseFromString(data[pos : pos + next_pos])
                    #print(data[pos : pos + next_pos])
                    total_expected += meta_data_proto.number_steps
                    pos = INITIAL_POS
                if obs_decoded == 1:
                    brain_param_proto = BrainParametersProto()
                    #print("##################brain")
                    #print(brain_param_proto.ParseFromString(data[pos : pos + next_pos]))
                    #print("##################")
                    brain_param_proto.ParseFromString(data[pos : pos + next_pos])
                    #print(data[pos : pos + next_pos])
                    pos += next_pos
                if obs_decoded > 1:
                    agent_info_action = AgentInfoActionPairProto()
                    #print("##################action")
                    #print(agent_info_action)
                    #print("##################")
                    agent_info_action.ParseFromString(data[pos : pos + next_pos])
                    #print("I print here..")
                    #print(data[pos : pos + next_pos])
                    if group_spec is None:
                        group_spec = agent_group_spec_from_proto(
                            brain_param_proto, agent_info_action.agent_info
                        )
                    info_action_pairs.append(agent_info_action)
                    if len(info_action_pairs) == total_expected:
                        break
                    pos += next_pos
                obs_decoded += 1
    if not group_spec:
        raise RuntimeError(
            f"No BrainParameters found in demonstration file at {file_path}."
        )
    return group_spec, info_action_pairs, total_expected

#%%

print("#########################################")
# demo_file=sys.argv[1]
# demo_file="..\\..\\environment\\neos\\UnityMiddleWare2\\Assets\\Demonstrations\\betatest2_19.demo"
demo_file="..\\..\\environment\\neos\\built_env\\Unity Environment_Data\\Demonstrations\\betatest2_0.demo"
group_spec, info_action_pairs, total_expected = load_demonstration(demo_file)
# group_spec, info_action_pairs, total_expected = load_demonstration("..\\..\\environment\\neos\\built_env\\Unity Environment_Data\\Demonstrations\\betatest2.demo")
# group_spec, info_action_pairs, total_expected = load_demonstration("D:\code\\temp\\built_env\\Unity Environment_Data\\Demonstrations\\betatest2_4.demo")
# group_spec, info_action_pairs, total_expected = load_demonstration("D:\code\\temp\\built_env\\Unity Environment_Data\\Demonstrations\\older\\betatest2_3.demo")
# group_spec, info_action_pairs, total_expected = load_demonstration("D:\code\\temp\\built_env\\Unity Environment_Data\\Demonstrations\\older\\betatest2_0.demo")
print(group_spec)
len(info_action_pairs)
type(info_action_pairs)
get_obs = lambda pair: np.array(pair.agent_info.observations[0].float_data.data)
get_actions = lambda pair: np.array(pair.action_info.vector_actions)

obs = np.stack(list(map(get_obs,info_action_pairs)))
acts = np.stack(list(map(get_actions,info_action_pairs)))

obs.shape
acts.shape

np.save("circling_box_obs",obs)
np.save("circling_box_acts",acts)

group_spec
print("#######")
print("Example obs-action pair")
print(info_action_pairs[0])
print("#######")
# type(info_action_pairs)
# len(info_action_pairs)
# info_action_pairs[0]

# type(info_action_pairs[0])

# list(info_action_pairs[0].agent_info.observations[1].float_data.data)[0])

float_obs=[list(pair.agent_info.observations[1].float_data.data) for pair in info_action_pairs]
import json
open("..\\..\\environment\\neos\\built_env\\Unity Environment_Data\\Demonstrations\\current_demo_floats.json","w").write(json.dumps(float_obs))
open("..\\..\\environment\\neos\\UnityMiddleWare2\\Assets\\Demonstrations\\current_demo_floats.json","w").write(json.dumps(float_obs))
# json.load(open("test.json","r"))
# info_action_pairs[0].agent_info.observations[0].compressed_data

# info_action_pairs[100]

print("#######")
print( total_expected)
print("#######")


# print("#########################################")
#
# group_spec, info_action_pairs, total_expected = load_demonstration("..\..\environment\neos\built_env\Unity Environment_Data\Demonstrations\betatest2.demo")
# print(group_spec)
# print("#######")
# print( info_action_pairs)
# print("#######")
# print( total_expected)
# print("#######")
