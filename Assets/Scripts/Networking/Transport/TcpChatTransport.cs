using System;

public class TcpChatTransport : IChatTransport
{
    public event Action<string> OnRawMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public bool IsRunning { get; private set; }

    private TcpChatServer server;
    private TcpChatClient client;

    private bool isServerMode;
    private bool isClientMode;

    public void StartAsServer(string ip, int port)
    {
        if (IsRunning)
        {
            OnError?.Invoke("TCP transport is already running.");
            return;
        }

        try
        {
            server = new TcpChatServer();
            server.OnServerLog += HandleServerLog;
            server.StartServer(port);

            isServerMode = true;
            isClientMode = false;
            IsRunning = server.IsRunning;

            if (IsRunning)
            {
                OnConnected?.Invoke();
            }
            else
            {
                OnError?.Invoke("TCP server failed to start.");
            }
        }
        catch (Exception exception)
        {
            OnError?.Invoke($"TCP server start failed: {exception.Message}");
        }
    }

    public async void StartAsClient(string ip, int port)
    {
        if (IsRunning)
        {
            OnError?.Invoke("TCP transport is already running.");
            return;
        }

        try
        {
            client = new TcpChatClient();
            client.OnRawMessageReceived += HandleClientMessageReceived;
            client.OnClientLog += HandleClientLog;
            client.OnDisconnected += HandleClientDisconnected;

            bool connected = await client.ConnectAsync(ip, port);

            isServerMode = false;
            isClientMode = connected;
            IsRunning = connected;

            if (connected)
            {
                OnConnected?.Invoke();
            }
            else
            {
                OnError?.Invoke("TCP client failed to connect.");
            }
        }
        catch (Exception exception)
        {
            OnError?.Invoke($"TCP client start failed: {exception.Message}");
        }
    }

    public void Send(string message)
    {
        if (!IsRunning)
        {
            OnError?.Invoke("TCP transport is not running.");
            return;
        }

        if (isClientMode)
        {
            client?.Send(message);
            return;
        }

        if (isServerMode)
        {
            server?.SendFromServer(message);
            return;
        }

        OnError?.Invoke("TCP transport is in an unknown state and cannot send messages.");
    }

    public void Stop()
    {
        if (client != null)
        {
            client.OnRawMessageReceived -= HandleClientMessageReceived;
            client.OnClientLog -= HandleClientLog;
            client.OnDisconnected -= HandleClientDisconnected;
            client.Disconnect();
            client = null;
        }

        if (server != null)
        {
            server.OnServerLog -= HandleServerLog;
            server.StopServer();
            server = null;
        }

        isServerMode = false;
        isClientMode = false;
        IsRunning = false;

        OnDisconnected?.Invoke();
    }

    private void HandleClientMessageReceived(string rawMessage)
    {
        OnRawMessageReceived?.Invoke(rawMessage);
    }

    private void HandleClientDisconnected()
    {
        IsRunning = false;
        OnDisconnected?.Invoke();
    }

    private void HandleClientLog(string logMessage)
    {
        if (logMessage.Contains("failed") || logMessage.Contains("error") || logMessage.Contains("Cannot"))
        {
            OnError?.Invoke(logMessage);
        }
    }

    private void HandleServerLog(string logMessage)
    {
        if (logMessage.Contains("failed") || logMessage.Contains("error"))
        {
            OnError?.Invoke(logMessage);
        }
    }
}