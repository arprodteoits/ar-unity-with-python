using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PythonUDPReceiver : MonoBehaviour
{
    public RawImage display;    // tempat menampilkan gambar
    public GameObject cube;     // objek untuk digerakkan
    public int port = 5052;

    private Thread receiveThread;
    private UdpClient client;
    private Texture2D texture;
    private byte[] latestImage = null;
    private PositionData latestPos = null;

    [System.Serializable]
    public class PositionData
    {
        public float x, y, w, h;
    }

    void Start()
    {
        texture = new Texture2D(2, 2);
        if (display != null)
            display.texture = texture;

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        client = new UdpClient(port);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                byte[] data = client.Receive(ref remoteEndPoint);

                if (data.Length > 4)
                {
                    string prefix = Encoding.ASCII.GetString(data, 0, 4);

                    // Jika data diawali "IMG|"
                    if (prefix.StartsWith("IMG"))
                    {
                        byte[] imgData = new byte[data.Length - 4];
                        Buffer.BlockCopy(data, 4, imgData, 0, imgData.Length);
                        latestImage = imgData;
                    }
                    // Jika data diawali "POS|"
                    else if (prefix.StartsWith("POS"))
                    {
                        string json = Encoding.UTF8.GetString(data, 4, data.Length - 4);
                        PositionData pos = JsonUtility.FromJson<PositionData>(json);
                        latestPos = pos;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Receive error: " + e.Message);
            }
        }
    }

    void Update()
    {
        // update gambar
        if (latestImage != null)
        {
            texture.LoadImage(latestImage);
            if (display != null)
                display.texture = texture;
            latestImage = null;
        }

        // update posisi
        if (latestPos != null && cube != null)
        {
            cube.transform.position = new Vector3(latestPos.x / 100f, latestPos.y / 100f, 0);
            latestPos = null;
        }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        client?.Close();
    }
}
