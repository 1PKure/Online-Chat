using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UdpChatServer
{
    private UdpClient udpServer;
    private CancellationTokenSource cancellationTokenSource;
    private readonly List<IPEndPoint> clients = new List<IPEndPoint>();

    public bool IsRunning { get; private set; }

    public event Action<string> OnServerLog;
    public event Action<string> OnRawMessageReceived;

    public void StartServer(int port)
    {
        if (IsRunning)
        {
            Log("UDP server is already running.");
            return;
        }

        try
        {
            cancellationTokenSource = new CancellationTokenSource();
            udpServer = new UdpClient(port);

            IsRunning = true;
            Log($"UDP server started on port {port}.");

            _ = Task.Run(() => ReceiveLoop(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }
        catch (Exception exception)
        {
            Log($"UDP server start failed: {exception.Message}");
            StopServer();
        }
    }

    public void StopServer()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;

        try
        {
            cancellationTokenSource?.Cancel();
            udpServer?.Close();

            lock (clients)
            {
                clients.Clear();
            }

            Log("UDP server stopped.");
        }
        catch (Exception exception)
        {
            Log($"UDP server stop error: {exception.Message}");
        }
    }

    public void SendFromServer(string rawMessage)
    {
        if (!IsRunning || string.IsNullOrWhiteSpace(rawMessage))
        {
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(rawMessage);
        Broadcast(data);
        Log($"UDP server sent: {rawMessage}");
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsRunning)
            {
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);
                IPEndPoint sender = result.RemoteEndPoint;

                RegisterClient(sender);

                if (string.IsNullOrWhiteSpace(message))
                {
                    continue;
                }

                if (message == "__hello__")
                {
                    Log($"UDP client registered: {sender.Address}:{sender.Port}");
                    continue;
                }

                Log($"UDP server received: {message}");
                OnRawMessageReceived?.Invoke(message);

                Broadcast(result.Buffer);
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
                Log($"UDP receive loop error: {exception.Message}");
            }
        }
    }

    private void Broadcast(byte[] messageData)
    {
        List<IPEndPoint> disconnectedClients = new List<IPEndPoint>();

        lock (clients)
        {
            foreach (IPEndPoint client in clients)
            {
                try
                {
                    udpServer.Send(messageData, messageData.Length, client);
                }
                catch
                {
                    disconnectedClients.Add(client);
                }
            }

            foreach (IPEndPoint disconnectedClient in disconnectedClients)
            {
                clients.RemoveAll(endpoint =>
                    endpoint.Address.Equals(disconnectedClient.Address) &&
                    endpoint.Port == disconnectedClient.Port);
            }
        }
    }

    private void RegisterClient(IPEndPoint endPoint)
    {
        lock (clients)
        {
            bool alreadyExists = clients.Exists(client =>
                client.Address.Equals(endPoint.Address) &&
                client.Port == endPoint.Port);

            if (!alreadyExists)
            {
                clients.Add(new IPEndPoint(endPoint.Address, endPoint.Port));
            }
        }
    }

    private void Log(string message)
    {
        Debug.Log($"[UDP SERVER] {message}");
        OnServerLog?.Invoke(message);
    }
}