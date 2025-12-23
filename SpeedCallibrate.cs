using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SpeedCallibrate : MonoBehaviour
{
    private int listenPort = 1804;

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool running = false;

    void Start()
    {
        udpClient = new UdpClient(listenPort);
        running = true;

        foreach (var addr in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                Debug.Log("Listening on IP: " + addr + " Port: " + listenPort);
            }
        }

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, listenPort);

        while (running)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data).Trim();
                ParseMessage(message);
            }
            catch (SocketException ex)
            {
                Debug.LogWarning("Socket exception: " + ex.Message);
            }
        }
    }

    void ParseMessage(string message)
    {
        string[] parts = message.Split(',');

        if (parts.Length != 6) return;

        if (int.TryParse(parts[0], out int forwardSpeed)) PLCSequence.forwardSpeed = forwardSpeed;
        if (int.TryParse(parts[1], out int reverseSpeed)) PLCSequence.reverseSpeed = reverseSpeed;
        if (int.TryParse(parts[2], out int upSpeed)) PLCSequence.upSpeed = upSpeed;
        if (int.TryParse(parts[3], out int downSpeed)) PLCSequence.downSpeed = downSpeed;
        if (int.TryParse(parts[4], out int clockwiseSpeed)) PLCSequence.clockwiseSpeed = clockwiseSpeed;
        if (int.TryParse(parts[5], out int anticlockwiseSpeed)) PLCSequence.anticlockwiseSpeed = anticlockwiseSpeed;

        Debug.Log($"Received speeds: {message}");
    }

    void OnApplicationQuit()
    {
        running = false;
        receiveThread?.Abort();
        udpClient?.Close();
    }
}
