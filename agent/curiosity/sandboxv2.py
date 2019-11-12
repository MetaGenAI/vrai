import pickle
from absl import app
import matplotlib.pyplot as plt
import os
import sys
# %matplotlib inline
import numpy as np
import gym
# import mujoco_py

env=gym.make("HandManipulatePen-v0")
env.reset()
env.observation_space["observation"]

from absl import flags

FLAGS = flags.FLAGS
flags.DEFINE_integer("n_dmp_basis",10,"number of basis functions used in the dynamical movement primitive")
flags.DEFINE_integer("n_simulation_steps",50,"number of simulation steps of each individual action rollout")
flags.DEFINE_integer("outcome_sampling_frequency",5,"frequency of observations which are saved into the outcome vector")
flags.DEFINE_bool("rendering",False,"whether to render the environment or not")
flags.DEFINE_bool("evaluating",False,"whether we are evaluating on the target goals, or still learning/exploring")
flags.DEFINE_integer("save_freq",1000,"the frequency by which to save memories")


#%%
# context_dim = env.observation_space["observation"].shape[0]
# n_steps = n_simulation_steps//outcome_sampling_frequency
# n_actuators = env.action_space.shape[0]
# action_dim = n_actuators*(n_dmp_basis+1) # +1 for target position, in dmp parametrization
# outcome_dim = context_dim*n_steps
# memory_dim = context_dim+outcome_dim+action_dim

# env.action_space.shape[0]
# env.observation_space["observation"].shape[0]
#
# # context_dim =
#
# context_dim = env.observation_space["observation"].shape[0]
# # n_simulation_steps = 200
# # outcome_sampling_frequency=10
# n_steps = n_simulation_steps//outcome_sampling_frequency
# n_actuators = env.action_space.shape[0]
# # n_dmp_basis = 20
# # # action_dim = n_steps*env.action_space.shape[0]
# action_dim = n_actuators*(n_dmp_basis+1) # +1 for target position, in dmp parametrization
# outcome_dim = context_dim*n_steps
# memory_dim = context_dim+outcome_dim+action_dim
# # rendering = False
# # evaluating = False
# # save_freq = 1000

# outcome = []
# for i in range(10):
#     action = env.action_space.sample()
#     results = env.step(action)
#     obs = results[0]["observation"]
#     outcome.append(obs)
#     # env.render()

# outcome = np.stack(outcome)
####
# env.action_space.low
#
# env.env.sim.model.joint_name2id(env.env.sim.model.joint_names[2])
#
# sim = env.env.sim
# joints = env.env.sim.model.joint_names
# n_joints = len(joints)
sim = env.env.sim
# sim.data.ctrl
#
#
# # joints
#
#
# # sim.data.get_joint_qpos("object:joint").shape
# #
# for i in range(n_joints):
#     print(sim.data.get_joint_qpos(env.env.sim.model.joint_names[i]))
#     print(sim.data.get_joint_qvel(env.env.sim.model.joint_names[i]))


# goal_outcomes = []
# for indices in goal_spaces_indices:
#     goal_outcomes.append(outcome[:,indices])

action = env.action_space.sample()
def get_actuator_center():
    ctrlrange = sim.model.actuator_ctrlrange
    actuation_range = (ctrlrange[:, 1] - ctrlrange[:, 0]) / 2.
    actuation_center = np.zeros_like(action)
    for i in range(sim.data.ctrl.shape[0]):
        actuation_center[i] = sim.data.get_joint_qpos(
        sim.model.actuator_names[i].replace(':A_', ':'))
        for joint_name in ['FF', 'MF', 'RF', 'LF']:
            act_idx = sim.model.actuator_name2id(
            'robot0:A_{}J1'.format(joint_name))
            actuation_center[act_idx] += sim.data.get_joint_qpos(
            'robot0:{}J0'.format(joint_name))
    return actuation_center

def goal_reward(goal, outcome, context):
    #TODO: I was lazy; really I should make it accept goal_space as argument
    return np.linalg.norm(goal - outcome)/np.sqrt(goal.shape[0])

###

# Instatiate metapolicies with random exploration


def goal_policy(goal_space, context):
    #goal is of size len(goal_spaces_indices[goal_space])
    indices = goal_spaces_indices[goal_space]
    #TODO: check ranges are right
    goal_dim = (indices.stop-indices.start)
    return 2*np.random.rand(goal_dim)-1

def goal_space_probabilities(intrinsic_rewards):
    # probs = np.exp(intrinsic_rewards*(intrinsic_rewards>0)-np.Inf*(intrinsic_rewards<=0))
    # probs = np.exp(intrinsic_rewards/np.sum(intrinsic_rewards))*(intrinsic_rewards>0)
    # probs = np.exp(intrinsic_rewards/(np.sum(intrinsic_rewards)+0.01))*(intrinsic_rewards>0)
    probs = np.exp(intrinsic_rewards)*(intrinsic_rewards>0)
    if not np.any(probs>0):
        probs = np.ones(probs.shape)
    probs /= np.sum(probs)
    return probs

def goal_space_policy(context):
    if np.random.rand() < 0.2:
        return np.random.choice(range(n_goal_spaces))
    else:
        return np.random.choice(range(n_goal_spaces), p=goal_space_probs)
    return None


def meta_policy(goal_space, goal, context):
    if evaluating:
        ###different types of evaluation###
        outcome_slice = slice(goal_spaces_indices[2].start,goal_spaces_indices[3].stop)
        # outcome_slice = slice(goal_spaces_indices[2].start,goal_spaces_indices[2].stop)
        # outcome_slice_one_time = slice(goal_spaces_indices[2].start//n_steps,goal_spaces_indices[2].stop//n_steps)
        # outcome_slice = slice(goal_spaces_indices[3].start,goal_spaces_indices[3].stop)
        # outcome_slice_one_time = slice(goal_spaces_indices[3].start//n_steps,goal_spaces_indices[3].stop//n_steps)
        mask = np.zeros(memory_dim)
        memory_slice = slice(context_dim+outcome_slice.start,context_dim+outcome_slice.stop)
        mask[memory_slice] = 1
        mask[:context_dim] = 1
        index_of_memory, closeness = find_memory_by_slice(context, outcome_slice, goal, mask=mask)
        print("closeness", closeness)
        if closeness > 2.0:
            outcome_slice = slice(goal_spaces_indices[2].start,goal_spaces_indices[3].stop)
            # outcome_slice = slice(goal_spaces_indices[3].start,goal_spaces_indices[3].stop)
            memory_slice = slice(outcome_slice.start,outcome_slice.stop)
            outcomes = np.zeros(outcome_dim)
            outcomes[memory_slice] = goal
            outcomes = np.expand_dims(outcomes,0)
            outcomes = outcomes.reshape((1,context_dim,n_steps))
            outcomes = np.concatenate([outcomes,np.zeros(outcomes.shape[0:2]+(1,))],2)
            context = np.expand_dims(context,0)
            outcomes = np.concatenate([np.expand_dims(context,2),outcomes],2)
            outcomes = outcomes.transpose(0,2,1)
            action = meta_policy_nn_model(outcomes.astype("float32")).numpy()[0,1:,:].transpose(1,0).reshape((-1,))
            print(action.shape)
        else:
            action = database[index_of_memory,-action_dim:]
    else:
        index_of_memory, closeness = find_memory(context, goal_space, goal)
        print("closeness",closeness)
        if closeness > 2.0:
            outcome_slice = goal_spaces_indices[goal_space]
            memory_slice = slice(outcome_slice.start,outcome_slice.stop)
            outcomes = np.zeros(outcome_dim)
            outcomes[memory_slice] = goal
            outcomes = np.expand_dims(outcomes,0)
            outcomes = outcomes.reshape((1,context_dim,n_steps))
            outcomes = np.concatenate([outcomes,np.zeros(outcomes.shape[0:2]+(1,))],2)
            context = np.expand_dims(context,0)
            outcomes = np.concatenate([np.expand_dims(context,2),outcomes],2)
            outcomes = outcomes.transpose(0,2,1)
            action = meta_policy_nn_model(outcomes.astype("float32")).numpy()[0,1:,:].transpose(1,0).reshape((-1,))
            print(action.shape)
        else:
            action = database[index_of_memory,-action_dim:]
    return action

def exploration_meta_policy(goal_space, goal, context):
    action = meta_policy(goal_space, goal, context)
    action += 0.1*np.random.randn(*action.shape)
    #clipping so that the added noise doesn't exceed the limits of the actuators
    action[:-n_actuators] = np.clip(action[:-n_actuators], -1, 1)
    actuator_center = get_actuator_center()
    action[-n_actuators:] = np.clip(action[-n_actuators:], -1 - actuator_center, 1 - actuator_center)
    return action

# running_average_window_size = 5
running_average_weighting = 0.5
def update_intrinsic_reward(intrinsic_rewards, goal_space, goal, context, outcome):
    index_of_memory,_ = find_memory(context, goal_space, goal)
    old_outcomes = database[index_of_memory,context_dim:-action_dim:]
    old_outcome_for_goal_space = old_outcomes[goal_spaces_indices[goal_space]]
    current_outcome_for_goal_space = outcome[goal_spaces_indices[goal_space]]
    # old_outcomes.shape
    learning_progress = goal_reward(goal,current_outcome_for_goal_space, context) - goal_reward(goal,old_outcome_for_goal_space, context)
    print(learning_progress)
    w = running_average_weighting
    r = intrinsic_rewards[goal_space]
    intrinsic_rewards[goal_space] = r*w + learning_progress*(1-w)
    return intrinsic_rewards

def update_exploration_policy(context, outcome, action_parameter):
    global database
    database = np.concatenate([database, np.expand_dims(np.concatenate([context, outcome, action_parameter]),0)], axis=0)

def update_goal_space_policy():
    global goal_space_probs
    goal_space_probs = goal_space_probabilities(intrinsic_rewards)

from scipy.integrate import ode

def basis_function(t,t0):
    return np.exp(-0.5*(t-t0)**2/((n_simulation_steps/n_dmp_basis)**2))

def basis_functions(t,x,g,w,y0):
    phis = np.array(list(map(lambda t0: basis_function(t,t0), np.linspace(0,n_simulation_steps,n_dmp_basis)))).T
    return x*(g-y0)*np.matmul(w,phis)/np.sum(phis)

env.relative_control = True
def dmp(t, variables, w, g):
    #TODO: make it relative to current position, rather than reset. Need to set env.relative_control = True
    y0 = np.zeros(n_actuators)
    alphay = 2/n_simulation_steps
    betay = 3.0
    alphax = 0.1
    variables = variables.reshape((n_actuators,3))
    y,v,x = variables[:,0],variables[:,1],variables[:,2]
    vdot = alphay*(betay*(g-y)-v) + basis_functions(t,x,g,w.reshape((n_actuators,n_dmp_basis)),y0)
    ydot = v
    xdot = -alphax*x
    return np.stack([ydot,vdot,xdot],axis=1).reshape((n_actuators*3))

def action_rollout(context,action_parameter,i):
    dt=1
    if i==0:
        solver = ode(dmp)
        solver.set_initial_value(np.tile(np.array([0,0,1]),(n_actuators,1)).reshape(-1),0)\
            .set_f_params(action_parameter[:-n_actuators],action_parameter[-n_actuators:])
        action_rollout.solver = solver
        return np.clip(action_rollout.solver.integrate(action_rollout.solver.t+dt).reshape((n_actuators,3))[:,0], -1,1)
    else:
        return np.clip(action_rollout.solver.integrate(action_rollout.solver.t+dt).reshape((n_actuators,3))[:,0], -1,1)

def main(argv):

    '''PREPPING UP variables'''

    global FLAGS
    FLAGS = FLAGS.flag_values_dict()
    # print(FLAGS)
    globals().update(FLAGS)

    context_dim = env.observation_space["observation"].shape[0]
    FLAGS["context_dim"] = context_dim
    n_steps = n_simulation_steps//outcome_sampling_frequency
    FLAGS["n_steps"] = n_steps
    n_actuators = env.action_space.shape[0]
    FLAGS["n_actuators"] = n_actuators
    action_dim = n_actuators*(n_dmp_basis+1) # +1 for target position, in dmp parametrization
    FLAGS["action_dim"] = action_dim
    outcome_dim = context_dim*n_steps
    FLAGS["outcome_dim"] = outcome_dim
    FLAGS["memory_dim"] = context_dim+outcome_dim+action_dim
    globals().update(FLAGS)

    indices_hand_pos = slice(0*n_steps,24*n_steps)
    indices_hand_vel = slice(24*n_steps,48*n_steps)
    indices_pen_pos = slice(48*n_steps,51*n_steps)
    indices_pen_rot = slice(51*n_steps,55*n_steps)
    indices_pen_vel = slice(55*n_steps,58*n_steps)
    indices_pen_rotvel = slice(58*n_steps,61*n_steps)

    global n_goal_spaces, goal_spaces_indices, goal_spaces_names, find_memory, find_memory_by_slice
    goal_spaces_indices = [indices_hand_pos,indices_hand_vel,indices_pen_pos,indices_pen_rot,indices_pen_vel,indices_pen_rotvel]
    goal_spaces_names = ["hand_pos","hand_vel","pen_pos","pen_rot","pen_vel","pen_rotvel"]
    n_goal_spaces = len(goal_spaces_indices)

    #used for find_memory
    default_mask = np.zeros(memory_dim)
    default_mask[:context_dim] = 1
    def find_memory(context, goal_space, goal, action=None, mask=default_mask):
        outcome_slice = goal_spaces_indices[goal_space]
        return find_memory_by_slice(context, outcome_slice, goal, action=None, mask=default_mask)

    def find_memory_by_slice(context, outcome_slice, goal, action=None, mask=default_mask):
        #TODO: This is a very hacky function, that is very specific to our implementation, rather than being general
        #it works because the goal and outcomes spaces are the same, but it's fine.
        query_vector = np.zeros(memory_dim)
        query_vector[:context_dim] = context
        #indices corresponding to the part of the outcome which we are querying against
        memory_slice = slice(context_dim+outcome_slice.start,context_dim+outcome_slice.stop)
        query_vector[memory_slice] = goal/np.sqrt(goal.shape[0])
        mask[memory_slice] = 1
        if action is not None:
            query_vector[-action_dim:] = action
        distances = np.linalg.norm((database - query_vector)*mask, axis=1)
        index_of_memory = np.argmin(distances,axis=0)
        return index_of_memory, distances[index_of_memory]


    from mpi4py import MPI
    comm = MPI.COMM_WORLD
    rank = comm.Get_rank()
    size = comm.Get_size()

    if evaluating: assert size == 1

    #initialize NN
    from meta_policy_neural_net import make_meta_policy_nn_model

    global meta_policy_nn_model
    meta_policy_nn_model = make_meta_policy_nn_model(FLAGS)

    if os.path.exists("model_weights.p"):
        updated_model_weigths = pickle.load(open("model_weights.p","rb"))
        meta_policy_nn_model.set_weights(updated_model_weigths)

    if rank == 0:
        # yeah this is uggly, okkk
        global database
        global intrinsic_rewards
        global goal_space_probs
        files = os.listdir("memories")
        # print(list(files))
        files = [os.path.join("memories", f) for f in files] # add path to each file
        files.sort(key=lambda x: os.path.getmtime(x),reverse=True)
        MAX_MEMORY_SIZE = 10000
        if len(files)>1:
            for ii,filename in enumerate(files):
                if ii==0:
                    database = np.load(filename)
                else:
                    database = np.concatenate([database,np.load(filename)],0)
                if len(database) >MAX_MEMORY_SIZE:
                    break
        else:
            database = None
        if os.path.exists("intrinsic_rewards.p"):
            intrinsic_rewards = pickle.load(open("intrinsic_rewards.p","rb"))
        else:
            intrinsic_rewards = np.array([0.1 for index in goal_spaces_indices],dtype=np.float32)
        goal_space_probs = goal_space_probabilities(intrinsic_rewards)

        '''INITIALIZE ENVIRONMENT'''

        # action = env.action_space.sample()
        results = env.reset()
        context = results["observation"]
        if evaluating:
            pen_goal = results["desired_goal"]
            # pen_goal = pen_goal[:3]
            # pen_goal = pen_goal[3:]
            goal = np.tile(np.expand_dims(pen_goal,0),(n_steps,1))
            goal = np.reshape(goal.T,(-1))

        '''INITILIAZE MEMORY DATABASE VIA RANDOM EXPLORATION'''

        #some random exploration
        if database is None: #cold start
            print("random warming up")
            reset_env = False
            memories = 0
            while memories < 1000:
                observations = []
                action_parameter = 2*np.random.rand(action_dim)-1
                for i in range(n_simulation_steps):
                    # print(i)
                    if rendering:
                        env.render()
                    action = action_rollout(context, action_parameter, i)
                    results = env.step(action)
                    obs = results[0]["observation"]
                    done = results[2]
                    if done:
                        print("reseting environment")
                        results = env.reset()
                        # obs = results["observation"]
                        reset_env = True
                        break
                    if i % outcome_sampling_frequency == 0:
                        observations.append(obs)

                if reset_env:
                    reset_env = False
                    continue
                else:
                    print("Adding memory")
                    memories+=1

                outcome = np.reshape(np.stack(observations).T, (outcome_dim))
                if database is None and memories == 1:
                    database = np.expand_dims(np.concatenate([context, outcome, action_parameter]),0)
                    # print(database.shape)
                else:
                    update_exploration_policy(context, outcome, action_parameter)
        else:
            memories = database.shape[0]

        '''TRAINING LOOP'''
        print("active goal babbling")
        reset_env = False
        for iteration in range(200000):
            print("iteration",iteration)
            # this chooses one of the goal_spaces as an index from 0 to len(goal_spaces_indices)-1
            goal_space = goal_space_policy(context)

            if not evaluating:
                if comm.Iprobe(source=1, tag=12):
                    updated_model_weigths = comm.recv(source=1, tag=12)
                    meta_policy_nn_model.set_weights(updated_model_weigths)
                #goal is of size len(goal_spaces_indices[goal_space])
                goal = goal_policy(goal_space, context)

            if evaluating:
                action_parameter = meta_policy(goal_space, goal, context)
            else:
                #USE EXPLORATION POLICY
                action_parameter = exploration_meta_policy(goal_space, goal, context)

            observations = []
            for i in range(n_simulation_steps):
                action = action_rollout(context,action_parameter, i)
                results = env.step(action)
                if rendering:
                    env.render()
                obs = results[0]["observation"]
                done = results[2]
                if done:
                    print("reseting environment")
                    results = env.reset()
                    if evaluating:
                        pen_goal = results["desired_goal"]
                        # pen_goal = pen_goal[:3]
                        # pen_goal = pen_goal[3:]
                        goal = np.tile(np.expand_dims(pen_goal,0),(n_steps,1))
                        goal = np.reshape(goal.T,(-1))
                    reset_env = True
                    break
                if i % outcome_sampling_frequency == 0:
                    observations.append(obs)

            if reset_env:
                reset_env = False
                continue
            else:
                memories += 1
            outcome = np.reshape(np.stack(observations).T, (outcome_dim))

            context = observations[-1]

            if not evaluating:
                intrinsic_rewards = update_intrinsic_reward(intrinsic_rewards, goal_space, goal, context, outcome)
                print(goal_spaces_names)
                print(intrinsic_rewards)
                sys.stdout.flush()


            if not evaluating:
                update_exploration_policy(context, outcome, action_parameter)
                update_goal_space_policy()
                if iteration % save_freq == save_freq - 1:
                    print("Saving new batch of memories")
                    sys.stdout.flush()
                    # pickle.dump(database, open("database.p","wb"))
                    database = database[-1000:]
                    np.save("memories/database_"+str(iteration)+".npy",database)
                    pickle.dump(intrinsic_rewards, open("intrinsic_rewards.p","wb"))
                    comm.send(True, dest=1, tag=11) #signal to send to consolidation code to continue going on

        comm.send(False, dest=1, tag=11)
    if rank == 1:
        from meta_policy_neural_net import learn_from_database
        while comm.recv(source=0, tag=11):
        # while True:
            # files = filter(os.path.isfile, os.listdir("memories"))
            files = os.listdir("memories")
            # print(list(files))
            files = [os.path.join("memories", f) for f in files] # add path to each file
            files.sort(key=lambda x: os.path.getmtime(x),reverse=True)
            # print(files)
            # sys.stdout.flush()
            for filename in files:
                # if True:
                if not comm.Iprobe(source=0, tag=11):
                    database = np.load(filename)
                    print("Training on",filename)
                    sys.stdout.flush()
                    meta_policy_nn_model = learn_from_database(meta_policy_nn_model,database,FLAGS)
                    # sys.stdout.flush()
                    updated_model_weigths = meta_policy_nn_model.get_weights()
                    comm.send(updated_model_weigths, dest=0, tag=12)
                    pickle.dump(updated_model_weigths, open("model_weights.p","wb"))
                else:
                    break


if __name__ == '__main__':
  app.run(main)
