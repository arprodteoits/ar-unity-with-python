using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class PythonReceiver : MonoBehaviour
{
    private UdpClient client;
    public GameObject cube;
    public int port = 5053;

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

        if (cube != null)
            cube.SetActive(false); // awalnya disembunyikan
    }

    void Update()
    {
        if (client.Available > 0)
        {
            byte[] data = client.Receive(ref endpoint);
            string json = Encoding.UTF8.GetString(data);

            PositionData pos = JsonUtility.FromJson<PositionData>(json);

            // ========== DEBUG ==============
            Debug.Log($"📩 From Python: x={pos.x}, y={pos.y}, w={pos.w}, h={pos.h}");

            // Jika Python kirim x=-1 → tidak ada tangan
            if (pos.x < 0 || pos.y < 0)
            {
                if (cube != null)
                    cube.SetActive(false);
                return;
            }

            // Aktifkan cube
            if (cube != null && !cube.activeSelf)
                cube.SetActive(true);

            // =============================
            // Mapping koordinat kamera (1280x720) → Unity
            // =============================

            float unityX = (pos.x - 640f) / 300f;  // dari tengah (1280/2)
            float unityY = (pos.y - 360f) / 300f;  // dari tengah (720/2)

            // Y dibalik (opencv 0,0 di kiri atas)
            unityY = -unityY;

            cube.transform.localPosition = new Vector3(unityX, unityY, 0f);
        }
    }

    void OnApplicationQuit()
    {
        client?.Close();
    }
}
