using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UDPReceiver : MonoBehaviour
{
    public RawImage display;       // drag UI RawImage ke sini
    public int port = 5052;        // harus sama dengan sender.py
    private Thread receiveThread;
    private UdpClient client;
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
                    // update texture di main thread
                    UpdateTexture(data);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Receive error: " + e.Message);
            }
        }
    }

    void UpdateTexture(byte[] data)
    {
        // karena thread beda, gunakan Unity main thread
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            texture.LoadImage(data);
            display.texture = texture;
        });
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        client?.Close();
    }
}
