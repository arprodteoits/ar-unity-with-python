using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UDPReceiver : MonoBehaviour
{
    public RawImage display;       // Drag RawImage dari Inspector
    public int port = 5052;

    private Thread receiveThread;
    private UdpClient client;
    private byte[] latestData = null;
    private Texture2D texture;

    void Start()
    {
        texture = new Texture2D(2, 2);
        display.texture = texture;

        receiveThread = new Thread(new ThreadStart(ReceiveData));
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
                if (data != null && data.Length > 0)
                {
                    latestData = data; // simpan sementara, tidak langsung update UI
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
        // Update texture di main thread (aman)
        // if (latestData != null)
        // {
        //     texture.LoadImage(latestData);
        //     display.texture = texture;
        //     latestData = null;
        // }

        if (latestData != null)
                {
                    Debug.Log($"📩 Menerima data ukuran: {latestData.Length} bytes");
                    bool success = texture.LoadImage(latestData);

                    if (!success)
                        Debug.LogWarning("⚠️ Gagal decode image data!");
                    else
                    {
                        display.texture = texture;
                        display.SetNativeSize();
                    }

                    latestData = null;
                }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        client?.Close();
    }
}
