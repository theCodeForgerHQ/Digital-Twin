using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpHeatSender : MonoBehaviour
{
    private string serverIP = "192.168.56.60";
    private int serverPort = 4210;

    public HeatEmitter emitter1;
    public HeatEmitter emitter2;
    public HeatEmitter emitter3;

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;


    void Start()
    {
        Debug.Log("Initializing UDP Heat Sender");
        Debug.Log($"Server IP: {serverIP}, Server Port: {serverPort}");
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        InvokeRepeating(nameof(SendHeatData), 0f, 5f);
    }

    void SendHeatData()
    {
        Debug.Log("Preparing to send heat data");
        float heat1 = emitter1 != null ? emitter1.heat : 0f;
        float heat2 = emitter2 != null ? emitter2.heat : 0f;
        float heat3 = emitter3 != null ? emitter3.heat : 0f;

        string message = $"{heat1},{heat2},{heat3}";
        Debug.Log($"Sending heat data: {message}");
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, remoteEndPoint);
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
        udpClient = null;
    }
}
