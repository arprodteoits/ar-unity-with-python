using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PythonReceiverTexture : MonoBehaviour
{
    UdpClient client;
    public RawImage rawImage;  // untuk UI Canvas
    public GameObject cube;    // objek yang bergerak
    private Texture2D texture;

    void Start()
    {
        client = new UdpClient(5052);
        texture = new Texture2D(2, 2);
    }

    void Update()
    {
        if (client.Available > 0)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.Receive(ref endpoint);
            string json = Encoding.UTF8.GetString(data);
            FrameData frameData = JsonUtility.FromJson<FrameData>(json);

            // Decode base64 ke gambar
            byte[] imageBytes = Convert.FromBase64String(frameData.frame);
            texture.LoadImage(imageBytes);
            rawImage.texture = texture;

            // Ubah posisi cube berdasarkan data dari Python
            cube.transform.localPosition = new Vector3(frameData.x / 100f, frameData.y / 100f, 0);
        }
    }

    [Serializable]
    public class FrameData
    {
        public int x;
        public int y;
        public string frame;
    }
}
