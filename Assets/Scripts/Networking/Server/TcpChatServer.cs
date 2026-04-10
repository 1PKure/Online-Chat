using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TcpChatServer
{
    private TcpListener listener;
    private readonly List<ClientConnection> clients = new List<ClientConnection>();
    private CancellationTokenSource cancellationTokenSource;

    public bool IsRunning { get; private set; }

    public event Action<string> OnServerLog;

    public void StartServer(int port)
    {
        if (IsRunning)
        {
            Log("Server is already running.");
            return;
        }

        try
        {
            cancellationTokenSource = new CancellationTokenSource();
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            IsRunning = true;
            Log($"Server started on port {port}.");

            Task.Run(() => AcceptClientsLoop(cancellationTokenSource.Token));
        }
        catch (Exception exception)
        {
            Log($"Server start failed: {exception.Message}");
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
            listener?.Stop();

            lock (clients)
            {
                foreach (ClientConnection client in clients)
                {
                    client.Dispose();
                }

                clients.Clear();
            }

            Log("Server stopped.");
        }
        catch (Exception exception)
        {
            Log($"Server stop error: {exception.Message}");
        }
    }

    private async Task AcceptClientsLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                ClientConnection clientConnection = new ClientConnection(tcpClient);

                lock (clients)
                {
                    clients.Add(clientConnection);
                }

                Log("Client connected to server.");
                _ = Task.Run(() => ReceiveLoop(clientConnection, cancellationToken), cancellationToken);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception exception)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Log($"Accept client error: {exception.Message}");
                }
            }
        }
    }

    private async Task ReceiveLoop(ClientConnection clientConnection, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && clientConnection.TcpClient.Connected)
            {
                string message = await clientConnection.Reader.ReadLineAsync();

                if (string.IsNullOrEmpty(message))
                {
                    break;
                }

                Log($"Server received: {message}");
                Broadcast(message);
            }
        }
        catch (IOException)
        {
            Log("A client disconnected unexpectedly.");
        }
        catch (Exception exception)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Log($"Receive loop error: {exception.Message}");
            }
        }
        finally
        {
            RemoveClient(clientConnection);
        }
    }

    private void Broadcast(string message)
    {
        List<ClientConnection> disconnectedClients = new List<ClientConnection>();

        lock (clients)
        {
            foreach (ClientConnection client in clients)
            {
                try
                {
                    client.Writer.WriteLine(message);
                    client.Writer.Flush();
                }
                catch
                {
                    disconnectedClients.Add(client);
                }
            }

            foreach (ClientConnection disconnectedClient in disconnectedClients)
            {
                RemoveClient(disconnectedClient);
            }
        }
    }

    private void RemoveClient(ClientConnection clientConnection)
    {
        lock (clients)
        {
            if (clients.Contains(clientConnection))
            {
                clients.Remove(clientConnection);
                clientConnection.Dispose();
                Log("Client removed from server.");
            }
        }
    }

    private void Log(string message)
    {
        Debug.Log($"[SERVER] {message}");
        OnServerLog?.Invoke(message);
    }

    private class ClientConnection : IDisposable
    {
        public TcpClient TcpClient { get; }
        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }

        public ClientConnection(TcpClient tcpClient)
        {
            TcpClient = tcpClient;

            NetworkStream stream = tcpClient.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);
        }

        public void Dispose()
        {
            try { Reader?.Close(); } catch { }
            try { Writer?.Close(); } catch { }
            try { TcpClient?.Close(); } catch { }
        }
    }

    public void SendFromServer(string rawMessage)
    {
        if (!IsRunning || string.IsNullOrWhiteSpace(rawMessage))
        {
            return;
        }

        Log($"Server sending: {rawMessage}");
        Broadcast(rawMessage);
    }
}