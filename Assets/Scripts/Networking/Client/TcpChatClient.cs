using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TcpChatClient
{
    private TcpClient tcpClient;
    private StreamReader reader;
    private StreamWriter writer;
    private CancellationTokenSource cancellationTokenSource;

    public bool IsConnected => tcpClient != null && tcpClient.Connected;

    public event Action<string> OnRawMessageReceived;
    public event Action<string> OnClientLog;
    public event Action OnDisconnected;

    public async Task<bool> ConnectAsync(string ipAddress, int port)
    {
        if (IsConnected)
        {
            Log("Client is already connected.");
            return true;
        }

        try
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ipAddress, port);

            NetworkStream stream = tcpClient.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            cancellationTokenSource = new CancellationTokenSource();

            Log($"Connected to {ipAddress}:{port}");
            _ = Task.Run(() => ReceiveLoop(cancellationTokenSource.Token), cancellationTokenSource.Token);

            return true;
        }
        catch (Exception exception)
        {
            Log($"Connection failed: {exception.Message}");
            Disconnect();
            return false;
        }
    }

    public void Send(string rawMessage)
    {
        if (!IsConnected)
        {
            Log("Cannot send message because client is not connected.");
            return;
        }

        try
        {
            writer.WriteLine(rawMessage);
            writer.Flush();
        }
        catch (Exception exception)
        {
            Log($"Send failed: {exception.Message}");
            Disconnect();
        }
    }

    public void Disconnect()
    {
        try
        {
            cancellationTokenSource?.Cancel();
        }
        catch { }

        try { reader?.Close(); } catch { }
        try { writer?.Close(); } catch { }
        try { tcpClient?.Close(); } catch { }

        reader = null;
        writer = null;
        tcpClient = null;

        Log("Client disconnected.");
        OnDisconnected?.Invoke();
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                string message = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(message))
                {
                    break;
                }

                Log($"Received raw message: {message}");
                OnRawMessageReceived?.Invoke(message);
            }
        }
        catch (IOException)
        {
            Log("Server closed the connection.");
        }
        catch (Exception exception)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Log($"Receive error: {exception.Message}");
            }
        }
        finally
        {
            Disconnect();
        }
    }

    private void Log(string message)
    {
        Debug.Log($"[CLIENT] {message}");
        OnClientLog?.Invoke(message);
    }
}