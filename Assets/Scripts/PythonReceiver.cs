using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class PythonReceiver : MonoBehaviour
{
    UdpClient client;
    public GameObject cube;

    void Start()
    {
        client = new UdpClient(5053);
    }

    void Update()
    {
        if (client.Available > 0)
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.Receive(ref endpoint);
            string json = Encoding.UTF8.GetString(data);
            PositionData pos = JsonUtility.FromJson<PositionData>(json);

            // Ubah posisi cube berdasarkan data dari Python
            cube.transform.position = new Vector3(pos.x / 100f, pos.y / 100f, 0);
        }
    }

    [System.Serializable]
    public class PositionData
    {
        public float x, y, w, h;
    }
}
