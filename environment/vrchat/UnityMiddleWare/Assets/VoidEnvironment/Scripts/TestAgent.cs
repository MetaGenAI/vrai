using UnityEngine;
using MLAgents;
using VrcaiMlaCommunicator;
using System.Collections.Generic;
using ServiceWire.TcpIp;
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
using Microsoft.PerceptionSimulation;
using Vector3 = UnityEngine.Vector3;

public class TestAgent : Agent
{
    //[Header("Specific to Ball3D")]
    //public GameObject ball;
    //Rigidbody m_BallRb;
    //IFloatProperties m_ResetParams;
    public TcpClient<IVrcaiMlaTest> client;
    public IPEndPoint ipEndPoint;
    public TestAcademy academy;
    public List<float> inputs; //can't call this variable observations, coz I guess that's being used for something else? dunno
    //public IntPtr texturePtr;
    public float stop_training = 0;
    Texture2D tex;
    //RenderTexture rt;
    RawImage image;
    public override void InitializeAgent()
    {
        inputs = new List<float>() {0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f};
        academy = FindObjectOfType<TestAcademy>();
        //client = academy.client;
        ipEndPoint = academy.ipEndPoint;
        //using (var client = new TcpClient<IVrcaiMlaTest>(ipEndPoint))
        //{
        //    TextureMessage response = client.Proxy.getObs(new List<float>{0f,0f,0f});
        //    image = GameObject.Find("RawImage").GetComponent<RawImage>();
        //    //image.uvRect = new Rect(0,0, response.width, response.height);
        //    GameObject.Find("RawImage").GetComponent<RectTransform>().sizeDelta = new Vector2(response.width,response.height);
        //    //GameObject.Find("RawImage").GetComponent<RectTransform>().
        //    //Debug.Log("hi");
        //    //rt = new RenderTexture(tex.width / 2, tex.height / 2, 0);
        //}
        //m_BallRb = ball.GetComponent<Rigidbody>();
        //var academy = FindObjectOfType<Academy>();
        //m_ResetParams = academy.FloatProperties;
        //SetResetParameters();
    }

    public override void CollectObservations()
    {
        //AddVectorObs(gameObject.transform.rotation.z);
        //AddVectorObs(gameObject.transform.rotation.x);
        //AddVectorObs(ball.transform.position - gameObject.transform.position);
        //AddVectorObs(m_BallRb.velocity);
        //AddVectorObs(1f);
        //Debug.Log(inputs.Count);
        //for (int i = 0; i < observations.Count; i++)
        //{
        //    Debug.Log(observations[i].ToString());
        //}
        Debug.Log(inputs.Count);
        for (int i = 0; i < inputs.Count-2; i++)
        {
            //Debug.Log(inputs[i]);
            AddVectorObs(inputs[i]);
            //AddVectorObs(0.0f);
        }
        float reward = Mathf.Clamp(0.1f * inputs[inputs.Count - 2], -1f, 1f)+1f;
        //Debug.Log("Reward " + reward.ToString());
        AddReward(reward);
        stop_training = inputs[inputs.Count - 1];
    }

    //void FixedUpdate()
    //{

    //    using (var client = new TcpClient<IVrcaiMlaTest>(ipEndPoint))
    //    {
    //        //inputs = client.Proxy.getObs(new List<float>(vectorAction));
    //        TextureMessage response = client.Proxy.getObs(new List<float>(new List<float> { 0f, 0f, 0f }));
    //        //Debug.Log(response.raw_texture[0]);
    //        Destroy(tex);
    //        tex = new Texture2D(response.width, response.height, TextureFormat.RGB24, false);
    //        tex.LoadRawTextureData(response.raw_texture);
    //        tex.Apply();
    //        //Debug.Log(image);
    //        image.texture = tex;
    //    }
    //}

    public override void AgentAction(float[] vectorAction)
    {
        //Debug.Log(vectorAction.Length);
        //vectorAction = new float[] { 0f, 0f, 0f };
        //if (GetStepCount() >= 2500 || stop_training == 1f)
        //{
        //    using (var client = new TcpClient<IVrcaiMlaTest>(ipEndPoint))
        //    {
        //        //inputs = client.Proxy.getObs(new List<float>(vectorAction));
        //        TextureMessage response = client.Proxy.getObs(new List<float>(vectorAction));
        //        //Debug.Log(response.raw_texture[0]);
        //        Destroy(tex);
        //        tex = new Texture2D(response.width, response.height, TextureFormat.RGB24, false);
        //        tex.LoadRawTextureData(response.raw_texture);
        //        tex.Apply();
        //        image.texture = tex;
        //        //Destroy(tex);
        //        //Debug.Log(response.raw_texture);
        //        //tex.LoadRawTextureData(response.ptr,response.size);
        //    }
        //    //AddReward(-10f);
        //    Done();
        //}
        //else
        //{
        //    Debug.Log("acting");
        //    using (var client = new TcpClient<IVrcaiMlaTest>(ipEndPoint))
        //    {
        //        //inputs = client.Proxy.getObs(new List<float>(vectorAction));
        //        TextureMessage response = client.Proxy.getObs(new List<float>(vectorAction));
        //        //Debug.Log(response.raw_texture[0]);
        //        Destroy(tex);
        //        tex = new Texture2D(response.width, response.height, TextureFormat.RGB24, false);
        //        tex.LoadRawTextureData(response.raw_texture);
        //        tex.Apply();
        //        //Debug.Log(image);
        //        image.texture = tex;
        //        // texRef is your Texture2D
        //        // You can also reduice your texture 2D that way
        //        // Copy your texture ref to the render texture
        //        //UnityEngine.Graphics.Blit(tex, rt);
        //        //CaptureApplication("notepad");
        //        //Create a new bitmap.
        //        //var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
        //        //    Screen.PrimaryScreen.Bounds.Height,
        //        //    PixelFormat.Format32bppArgb);

        //        //// Create a graphics object from the bitmap.
        //        //var gfxScreenshot = System.Drawing.Graphics.FromImage(bmpScreenshot);

        //        //// Take the screenshot from the upper left corner to the right bottom corner.
        //        //gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
        //        //    Screen.PrimaryScreen.Bounds.Y,
        //        //    0,
        //        //    0,
        //        //    Screen.PrimaryScreen.Bounds.Size,
        //        //    CopyPixelOperation.SourceCopy);

        //        //// Save the screenshot to the specified path that the user has chosen.
        //        //bmpScreenshot.Save("Screenshot.png", ImageFormat.Png);
        //    }
        //}
        //Debug.Log("doing action");
        Dictionary<string, Vector3> actions = new Dictionary<String,Vector3>();
        actions.Add("move", new Vector3(0f, 0f, 0.05f));
        academy.DoAction(actions);
    }

    public override void AgentReset()
    {
        //Debug.Log("Episode done");
        //if (stop_training == 0)
        //{
        //    Debug.Log("Reseting agent");
        //    using (var client = new TcpClient<IVrcaiMlaTest>(ipEndPoint))
        //        client.Proxy.resetAgent();
        //}
    }

    public void Update()
    {
    }

    public override float[] Heuristic()
    {
        var action = new float[2];

        //action[0] = -Input.GetAxis("Horizontal");
        //action[1] = Input.GetAxis("Vertical");
        return action;
    }

    public void SetBall()
    {
        //Set the attributes of the ball by fetching the information from the academy
        //m_BallRb.mass = m_ResetParams.GetPropertyWithDefault("mass", 1.0f);
        //var scale = m_ResetParams.GetPropertyWithDefault("scale", 1.0f);
        //ball.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetResetParameters()
    {
        //SetBall();
    }
    public void CaptureApplication(string procName)
    {
        var proc = Process.GetProcessesByName(procName)[0];
        Debug.Log(proc);
        //var bmp = PrintWindow(proc.MainWindowHandle);
        //var rect = new User32.Rect();
        //User32.GetWindowRect(proc.MainWindowHandle, ref rect);

        //int width = rect.right - rect.left;
        //int height = rect.bottom - rect.top;

        ////Debug.Log(width.ToString());
        //var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        //System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp);
        //graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

        //bmp.Save("c:\\tmp\\test.png", ImageFormat.Png);
    }

    private class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
    }
}
