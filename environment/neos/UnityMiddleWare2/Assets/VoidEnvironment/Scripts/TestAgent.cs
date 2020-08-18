using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System;
using System.Drawing;
//using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Screen = System.Windows.Forms.Screen;
using Google.Protobuf;
using UnityEngine.UI;
using Grpc.Core;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels;
using MLAgents.Demonstrations;
using MLAgents.Policies;
using System.IO;
using Newtonsoft.Json;
using Random = System.Random;

public class TestAgent : Agent
{
    private DataComm.DataCommClient client;
    //public TestAcademy academy;
    //public List<float> inputs; //can't call this variable observations, coz I guess that's being used for something else? dunno
    public float stop_training = 0;
    public bool should_reset = false;
    public int texture_width = 84, texture_height = 84;
    public int agent_index = 0;
    int action_dim;
    int obs_dim;
    int vis_obs_dim;
    bool neos_do_recording;
    bool is_recording = false;
    string demo_file;
    StreamWriter demo_file_writer;
    float[][] demo_obss;
    List<List<float>> demo_obss_list;
    bool has_loaded_demo = false;
    Texture2D[] texs;
    RawImage image;
    public RawImage raw_image;
    TextureSensorComponent[] texture_sensors;
    Random rnd = new Random();


    public override void Initialize()
    {
        Debug.Log("HII");
        try
        {
            var channel = new Channel("127.0.0.1:5005"+(2+agent_index).ToString(), ChannelCredentials.Insecure);
            this.client = new DataComm.DataCommClient(channel);
            is_recording =this.GetComponent<DemonstrationRecorder>().record;

            var response = client.EstablishConnection(new ConnectionParams { IsRecording = is_recording });
            action_dim = response.ActionDim;
            obs_dim = response.ObsDim;
            vis_obs_dim = response.VisObsDim;
            neos_do_recording = response.DoRecording;
            agent_index = response.AgentIndex;
            demo_file = response.DemoFile;
            Response res = response.Res;
            if (res.Res != "Ok")
                Debug.Log(res.Res);
            BehaviorParameters behavior_params = GetComponent<BehaviorParameters>();
            behavior_params.brainParameters.vectorActionSize = new int[] { action_dim };
            behavior_params.brainParameters.vectorObservationSize = obs_dim;
            if (neos_do_recording) behavior_params.behaviorType = BehaviorType.HeuristicOnly;

            DemonstrationRecorder demo_recorder = GetComponent<DemonstrationRecorder>();
            demo_recorder.record = neos_do_recording;

            //TODO: set camera params from Neos too.
            texture_sensors = new TextureSensorComponent[vis_obs_dim];
            texs = new Texture2D[vis_obs_dim];
            for (int i = 0; i < vis_obs_dim; i++)
            {
                TextureSensorComponent texture_sensor = gameObject.AddComponent<TextureSensorComponent>() as TextureSensorComponent;
                texture_sensor.num_channels = 4;
                texture_sensor.width = texture_width;
                texture_sensor.height = texture_height;
                //texture_sensor.name = "Texture Sensor " + i.ToString();
                texture_sensors[i] = texture_sensor;
            }
            image = raw_image.GetComponent<RawImage>();

            if (neos_do_recording)
            {
                demo_file_writer = new StreamWriter("Assets\\Demonstrations\\" + demo_file + "_side_info.csv", true);
            }
            else
            {
                if (File.Exists("Assets\\Demonstrations\\" + demo_file + "_side_info.csv"))
                {
                    demo_obss_list = new List<List<float>>();
                    using (StreamReader r = new StreamReader("Assets\\Demonstrations\\" + demo_file + "_side_info.csv"))
                    {
                        while (!r.EndOfStream)
                        {
                            string line = r.ReadLine();
                            List<float> values = line.Split(',').Select((x)=>float.Parse(x)).ToList();
                            demo_obss_list.Add(values);
                        }
                    }
                    demo_obss = demo_obss_list.Select(a => a.ToArray()).ToArray();
                    has_loaded_demo = true;
                }
            }
            //if (File.Exists("Assets\\Demonstrations\\"+demo_file+"_side_info.csv")) {
            //    Debug.Log("loading demo file");
            //    using (StreamReader r = new StreamReader("Assets\\Demonstrations\\current_demo_floats.json"))
            //    {
            //        string json = r.ReadToEnd();
            //        demo_obss_list = JsonConvert.DeserializeObject<List<List<float>>>(json);
            //        //Debug.Log(demo_obss_list.ToString());
            //        //demo_obss_list.ForEach((x) => x.ForEach((y) => Debug.Log(y.ToString())));
            //        demo_obss = demo_obss_list.Select(a => a.ToArray()).ToArray();
            //        Debug.Log(demo_obss[0][0]);
            //    }
            //    has_loaded_demo = true;
            //}
        } catch (Exception e) {
            Debug.Log("Exception caught "+e.ToString());
            this.gameObject.SetActive(false);
            var res = client.StopConnection(new Empty());
            if (res.Res != "Ok")
                Debug.Log(res.Res);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        NeosObservation obs = client.GetObs(new Empty());
        for (int i = 0; i < obs.Obs.Count(); i++) { sensor.AddObservation(obs.Obs[i]); }
        //Debug.Log("OBS");
        //obs.Obs.ToList().ForEach(i => Debug.Log(i.ToString()));
        if (neos_do_recording)
        {
            float[] side_info = obs.SideInfo.ToArray();
            demo_file_writer.WriteLine(String.Join(",", side_info.Select((x) => x.ToString())));
        }
        float reward = obs.Reward;
        //obs.Obs;
        //Debug.Log("Reward " + reward.ToString());
        AddReward(reward);
        should_reset = obs.ShouldReset;

        TextureObservation res = client.GetTextureObs(new Empty());
        //byte[][] texture_bytes = new byte[vis_obs_dim][];
        //Debug.Log(vis_obs_dim.ToString());
        for (int i = 0; i < vis_obs_dim; i++)
        {
            //texture_bytes[i] = res.Textures[i].ToByteArray();
            //Debug.Log(texture_bytes[i][1].ToString());
            Destroy(texs[i]);
            texs[i] = new Texture2D(texture_width, texture_height, TextureFormat.ARGB32, false);
            texs[i].LoadRawTextureData(res.Textures[i].ToByteArray());
            texs[i].Apply();
            //if (i == 0) image.texture = texs[0];
            //Debug.Log("Length of texture bytes: " + texture_bytes[i].Length.ToString());
        }
        //Debug.Log(texs[0].GetRawTextureData()[0].ToString());
        //Debug.Log(texs[0].GetRawTextureData()[10].ToString());
        //image.texture = texs[0];
        ////visual obs
        for (int i = 0; i < vis_obs_dim; i++) { texture_sensors[i].UpdateTexture(texs[i]); }
    }

    /// <summary>
    /// we run this on FixedUpdate, because for some reason applying the texture to the image doesn't work inside CollectObservations, for some reason
    /// </summary>
    void FixedUpdate()
    {
        //TextureObservation res = client.GetTextureObs(new Empty());
        //byte[][] texture_bytes = new byte[vis_obs_dim][];
        ////Debug.Log(vis_obs_dim.ToString());
        //for (int i = 0; i < vis_obs_dim; i++)
        //{
        //    texture_bytes[i] = res.Textures[i].ToByteArray();
        //    //Debug.Log(texture_bytes[i][1].ToString());
        //    Destroy(texs[i]);
        //    texs[i] = new Texture2D(texture_width, texture_height, TextureFormat.ARGB32, false);
        //    texs[i].LoadRawTextureData(texture_bytes[i]);
        //    texs[i].Apply();
        //    if (i == 0) image.texture = texs[0];
        //    //Debug.Log("Length of texture bytes: " + texture_bytes[i].Length.ToString());
        //}
        //image.texture = texs[0];

    }

    void Update()
    {
        //image.texture = texs[0];
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //Debug.Log("ACTIONS");
        //vectorAction.ToList().ForEach(i => Debug.Log(i.ToString()));
        //Debug.Log("Total steps: " + StepCount.ToString());
        //vectorAction[0] = 0;
        //vectorAction[1] = 0;
        //vectorAction[2] = 0;
        //vectorAction[9] = 0;
        //vectorAction[10] = 0;
        //vectorAction[11] = 0;
        Response res;
        if (should_reset || StepCount >= 1500)
        {
            res = client.SendAct(new NeosAction { Action = { vectorAction } });
            if (res.Res != "Ok")
                Debug.Log(res.Res);
            EndEpisode();
        }
        else
        {
            res = client.SendAct(new NeosAction { Action = { vectorAction } });
            if (res.Res != "Ok")
                Debug.Log(res.Res);
        }

    }

    public override void OnEpisodeBegin()
    {
        Response res;
        float[] observations = new float[obs_dim];
        if (has_loaded_demo)
        {
            int idx = rnd.Next(0,demo_obss.Length-1);
            float[] demo_obs = demo_obss[idx];
            observations = demo_obs;
        }
        else
        {
            Array.Clear(observations, 0, observations.Length);
        }
        //Debug.Log("hello");
        //Debug.Log(observations[0]);
        res = client.ResetAgent(new BareObs { Obs = { observations } });
        if (res.Res != "Ok")
            Debug.Log(res.Res);
    }

    public override float[] Heuristic()
    {
        //Debug.Log("HIIII HEURISTICAAA");
        NeosAction action_message = client.GatherAct(new Empty());
        var action = new float[action_dim];
        action_message.Action.CopyTo(action, 0);
        //action.ToList().ForEach(i => Debug.Log(i.ToString()));
        return action;
    }

    private void OnApplicationQuit()
    {
        if (neos_do_recording)
            demo_file_writer.Close();
        var res = client.StopConnection(new Empty());
        if (res.Res != "Ok")
            Debug.Log(res.Res);
    }

}

