using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UdpChatClient
{
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;
    private CancellationTokenSource cancellationTokenSource;

    public bool IsConnected { get; private set; }

    public event Action<string> OnRawMessageReceived;
    public event Action<string> OnClientLog;
    public event Action OnDisconnected;

    public async Task<bool> ConnectAsync(string ipAddress, int port)
    {
        if (IsConnected)
        {
            Log("UDP client is already connected.");
            return true;
        }

        try
        {
            serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            udpClient = new UdpClient(0);
            cancellationTokenSource = new CancellationTokenSource();

            IsConnected = true;

            Log($"UDP client ready. Server endpoint: {ipAddress}:{port}");

            _ = Task.Run(() => ReceiveLoop(cancellationTokenSource.Token), cancellationTokenSource.Token);

            await SendHelloAsync();

            return true;
        }
        catch (Exception exception)
        {
            Log($"UDP connection failed: {exception.Message}");
            Disconnect();
            return false;
        }
    }

    public void Send(string rawMessage)
    {
        if (!IsConnected || udpClient == null)
        {
            Log("Cannot send UDP message because client is not connected.");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(rawMessage);
            udpClient.Send(data, data.Length, serverEndPoint);
        }
        catch (Exception exception)
        {
            Log($"UDP send failed: {exception.Message}");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        try
        {
            cancellationTokenSource?.Cancel();
        }
        catch
        {
        }

        try
        {
            udpClient?.Close();
        }
        catch
        {
        }

        udpClient = null;
        IsConnected = false;

        Log("UDP client disconnected.");
        OnDisconnected?.Invoke();
    }

    private async Task SendHelloAsync()
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes("__hello__");
            await udpClient.SendAsync(data, data.Length, serverEndPoint);
            Log("UDP hello sent to server.");
        }
        catch (Exception exception)
        {
            Log($"UDP hello failed: {exception.Message}");
        }
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
#if NET_STANDARD_2_1 || NET_4_6 || NET_4_8
                UdpReceiveResult result = await udpClient.ReceiveAsync();
#else
                UdpReceiveResult result = await udpClient.ReceiveAsync();
#endif
                string message = Encoding.UTF8.GetString(result.Buffer);

                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }

                if (message == "__hello__")
                {
                    continue;
                }

                Log($"UDP received raw message: {message}");
                OnRawMessageReceived?.Invoke(message);
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (SocketException)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Log("UDP socket closed.");
            }
        }
        catch (Exception exception)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Log($"UDP receive error: {exception.Message}");
            }
        }
        finally
        {
            Disconnect();
        }
    }

    private void Log(string message)
    {
        Debug.Log($"[UDP CLIENT] {message}");
        OnClientLog?.Invoke(message);
    }
}