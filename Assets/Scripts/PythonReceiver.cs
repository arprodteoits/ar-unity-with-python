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

        if (pos.x < 0)
        {
            cube.SetActive(false);
            hasPrev = false;
            return;
        }

        cube.SetActive(true);

        // --- Konversi pixel ke world space ---
        float unityX = (pos.x - 640f) / 300f;
        float unityY = -(pos.y - 360f) / 300f;

        float depth = Mathf.Clamp(2.5f - (pos.w / 250f), 0.5f, 3f);

        Vector3 newPos = new Vector3(unityX, unityY, depth);

        // --- POSISI ---
        cube.transform.localPosition = newPos;

        // --- ROTASI mengikuti arah gerakan tangan ---
        if (hasPrev)
        {
            Vector3 velocity = newPos - prevPos;

            // rotasi Y berdasarkan gerakan kiri-kanan
            float rotY = velocity.x * 500f;

            // rotasi X berdasarkan gerakan atas-bawah
            float rotX = -velocity.y * 500f;

            // apply rotasi smooth
            Quaternion targetRot = Quaternion.Euler(rotX, rotY, 0);
            cube.transform.localRotation = Quaternion.Slerp(
                cube.transform.localRotation,
                targetRot,
                Time.deltaTime * 6f
            );
        }

        prevPos = newPos;
        hasPrev = true;
    }
}


    void OnApplicationQuit()
    {
        client?.Close();
    }
}
