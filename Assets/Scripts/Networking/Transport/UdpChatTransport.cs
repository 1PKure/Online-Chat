using System;

public class UdpChatTransport : IChatTransport
{
    public event Action<string> OnRawMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public bool IsRunning { get; private set; }

    private UdpChatServer server;
    private UdpChatClient client;

    private bool isServerMode;
    private bool isClientMode;

    public void StartAsServer(string ip, int port)
    {
        if (IsRunning)
        {
            OnError?.Invoke("UDP transport is already running.");
            return;
        }

        try
        {
            server = new UdpChatServer();
            server.OnServerLog += HandleServerLog;
            server.OnRawMessageReceived += HandleServerMessageReceived;
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
                OnError?.Invoke("UDP server failed to start.");
            }
        }
        catch (Exception exception)
        {
            OnError?.Invoke($"UDP server start failed: {exception.Message}");
        }
    }

    public async void StartAsClient(string ip, int port)
    {
        if (IsRunning)
        {
            OnError?.Invoke("UDP transport is already running.");
            return;
        }

        try
        {
            client = new UdpChatClient();
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
                OnError?.Invoke("UDP client failed to connect.");
            }
        }
        catch (Exception exception)
        {
            OnError?.Invoke($"UDP client start failed: {exception.Message}");
        }
    }

    public void Send(string message)
    {
        if (!IsRunning)
        {
            OnError?.Invoke("UDP transport is not running.");
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
            server.OnRawMessageReceived -= HandleServerMessageReceived;
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

    private void HandleServerMessageReceived(string rawMessage)
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
        string lowerLog = logMessage.ToLower();

        if (lowerLog.Contains("failed") || lowerLog.Contains("error") || lowerLog.Contains("cannot"))
        {
            OnError?.Invoke(logMessage);
        }
    }

    private void HandleServerLog(string logMessage)
    {
        string lowerLog = logMessage.ToLower();

        if (lowerLog.Contains("failed") || lowerLog.Contains("error"))
        {
            OnError?.Invoke(logMessage);
        }
    }
}