using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class PythonReceiver : MonoBehaviour
{
    private UdpClient client;
    public GameObject cube;
    public int port = 5053; // pastikan sama dengan di Python
    public float scale = 0.01f; // untuk ubah pixel jadi world space

    private IPEndPoint endpoint;

    [System.Serializable]
    public class PositionData
    {
        public float x, y, w, h;
    }

    void Start()
    {
        client = new UdpClient(port);
        endpoint = new IPEndPoint(IPAddress.Any, 0);
    }

    void Update()
    {
        if (client.Available > 0)
        {
            byte[] data = client.Receive(ref endpoint);
            string json = Encoding.UTF8.GetString(data);

            try
            {
                PositionData pos = JsonUtility.FromJson<PositionData>(json);
                
                // Konversi posisi pixel (kamera Python) ke Unity world
                float unityX = (pos.x - 160f) * scale; // 320px/2 = 160
                float unityY = (pos.y - 120f) * scale; // 240px/2 = 120

                cube.transform.localPosition = new Vector3(unityX, -unityY, 0);
                cube.SetActive(true);
            }
            catch
            {
                Debug.LogWarning($"Gagal parse JSON: {json}");
            }
        }
    }

    void OnApplicationQuit()
    {
        client?.Close();
    }
}
