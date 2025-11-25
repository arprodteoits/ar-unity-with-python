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

    Vector3 prevPos = Vector3.zero;  
    bool hasPrev = false;

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

        // Jika tidak ada tangan (-1), sembunyikan cube
        if (pos.x < 0)
        {
            cube.SetActive(false);
            return;
        }

        cube.SetActive(true);

        // Konversi posisi pixel → world space
        float unityX = (pos.x - 640f) / 250f; // frame 1280x720 → half 640
        float unityY = (pos.y - 360f) / 250f;

        cube.transform.position = new Vector3(unityX, -unityY, 3f);

        // --- ROTASI LOCKED (tidak berubah) ---
        cube.transform.rotation = Quaternion.identity;
        // Atau: cube.transform.LookAt(Camera.main.transform);
        // Atau: cube.transform.rotation = lockedRotation;
    }
}



    void OnApplicationQuit()
    {
        client?.Close();
    }
}
