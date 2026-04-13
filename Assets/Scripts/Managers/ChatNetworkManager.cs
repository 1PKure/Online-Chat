using System;
using System.Threading.Tasks;
using UnityEngine;

public class ChatNetworkManager : MonoBehaviour
{
    public static ChatNetworkManager Instance { get; private set; }

    private IChatTransport serverTransport;
    private IChatTransport clientTransport;

    private string localUserId;
    private bool hasRaisedConnectedEvent;
    private bool hasManualInitialization;
    private ConnectionConfig currentConfig;

    public event Action<ChatMessageData> OnMessageReceived;
    public event Action<string> OnStatusChanged;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public bool IsConnected => clientTransport != null && clientTransport.IsRunning;

    public string LocalUserName =>
        currentConfig != null && !string.IsNullOrWhiteSpace(currentConfig.UserName)
            ? currentConfig.UserName
            : "Unknown";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        if (hasManualInitialization)
        {
            return;
        }

        await InitializeNetworkFromSessionAsync();
    }

    public void StartWithConfig(ConnectionConfig config)
    {
        if (config == null)
        {
            SetStatus("Missing connection config.");
            return;
        }

        hasManualInitialization = true;
        _ = StartWithConfigAsync(config);
    }

    private async Task StartWithConfigAsync(ConnectionConfig config)
    {
        Shutdown();
        await InitializeNetworkAsync(config);
    }

    private async Task InitializeNetworkFromSessionAsync()
    {
        if (SessionData.Instance == null || !SessionData.Instance.HasConfig())
        {
            SetStatus("Missing session config.");
            return;
        }

        await InitializeNetworkAsync(SessionData.Instance.CurrentConfig);
    }

    private async Task InitializeNetworkAsync(ConnectionConfig config)
    {
        if (config == null)
        {
            SetStatus("Missing connection config.");
            return;
        }

        currentConfig = config;
        localUserId = Guid.NewGuid().ToString();
        hasRaisedConnectedEvent = false;

        if (config.Mode != ConnectionMode.DedicatedServer && MessageRepository.Instance != null)
        {
            MessageRepository.Instance.ClearMessages();
        }

        switch (config.Mode)
        {
            case ConnectionMode.Client:
                await InitializeClientAsync(config);
                break;

            case ConnectionMode.Host:
                await InitializeHostAsync(config);
                break;

            case ConnectionMode.DedicatedServer:
                await InitializeDedicatedServerAsync(config);
                break;

            default:
                SetStatus("Unsupported connection mode.");
                break;
        }
    }

    private async Task InitializeHostAsync(ConnectionConfig config)
    {
        serverTransport = ChatTransportFactory.Create(config.TransportType);

        if (serverTransport == null)
        {
            SetStatus("Failed to create server transport.");
            return;
        }

        serverTransport.OnError += HandleServerTransportError;
        serverTransport.OnDisconnected += HandleServerTransportDisconnected;

        string listenAddress = string.IsNullOrWhiteSpace(config.IPAddress)
            ? "0.0.0.0"
            : config.IPAddress;

        serverTransport.StartAsServer(listenAddress, config.Port);

        await Task.Delay(200);

        clientTransport = ChatTransportFactory.Create(config.TransportType);

        if (clientTransport == null)
        {
            SetStatus("Failed to create local client transport.");
            return;
        }

        BindClientTransportEvents(clientTransport);

        string connectionAddress = "127.0.0.1";
        clientTransport.StartAsClient(connectionAddress, config.Port);
    }

    private async Task InitializeClientAsync(ConnectionConfig config)
    {
        await Task.Yield();

        clientTransport = ChatTransportFactory.Create(config.TransportType);

        if (clientTransport == null)
        {
            SetStatus("Failed to create client transport.");
            return;
        }

        BindClientTransportEvents(clientTransport);

        string connectionAddress = string.IsNullOrWhiteSpace(config.IPAddress)
            ? "127.0.0.1"
            : config.IPAddress;

        clientTransport.StartAsClient(connectionAddress, config.Port);
    }

    private async Task InitializeDedicatedServerAsync(ConnectionConfig config)
    {
        await Task.Yield();

        serverTransport = ChatTransportFactory.Create(config.TransportType);

        if (serverTransport == null)
        {
            SetStatus("Failed to create dedicated server transport.");
            return;
        }

        serverTransport.OnError += HandleServerTransportError;
        serverTransport.OnDisconnected += HandleServerTransportDisconnected;

        string listenAddress = string.IsNullOrWhiteSpace(config.IPAddress)
            ? "0.0.0.0"
            : config.IPAddress;

        serverTransport.StartAsServer(listenAddress, config.Port);

        SetStatus($"Dedicated server started on {listenAddress}:{config.Port} using {config.TransportType}.");
    }

    public void SendChatMessage(string text, string replyToMessageId = "")
    {
        if (!IsConnected)
        {
            SetStatus("You are not connected.");
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            SetStatus("Message cannot be empty.");
            return;
        }

        text = text.Trim();

        if (text.Length > NetworkConstants.MaxMessageLength)
        {
            SetStatus($"Message cannot exceed {NetworkConstants.MaxMessageLength} characters.");
            return;
        }

        ChatMessageData messageData = new ChatMessageData
        {
            MessageId = Guid.NewGuid().ToString(),
            SenderClientId = SessionData.Instance != null ? SessionData.Instance.ClientId : localUserId,
            SenderName = LocalUserName,
            Text = text,
            ReplyToMessageId = replyToMessageId,
            Timestamp = DateTime.Now.ToString("HH:mm")
        };

        string json = JsonPacketSerializer.Serialize(messageData);
        clientTransport.Send(json);
    }

    public void Shutdown()
    {
        UnbindAndStopClientTransport();
        UnbindAndStopServerTransport();

        hasRaisedConnectedEvent = false;
    }

    private void BindClientTransportEvents(IChatTransport transport)
    {
        transport.OnRawMessageReceived += HandleRawMessageReceived;
        transport.OnConnected += HandleClientTransportConnected;
        transport.OnDisconnected += HandleClientTransportDisconnected;
        transport.OnError += HandleClientTransportError;
    }

    private void UnbindAndStopClientTransport()
    {
        if (clientTransport == null)
        {
            return;
        }

        clientTransport.OnRawMessageReceived -= HandleRawMessageReceived;
        clientTransport.OnConnected -= HandleClientTransportConnected;
        clientTransport.OnDisconnected -= HandleClientTransportDisconnected;
        clientTransport.OnError -= HandleClientTransportError;

        clientTransport.Stop();
        clientTransport = null;
    }

    private void UnbindAndStopServerTransport()
    {
        if (serverTransport == null)
        {
            return;
        }

        serverTransport.OnError -= HandleServerTransportError;
        serverTransport.OnDisconnected -= HandleServerTransportDisconnected;

        serverTransport.Stop();
        serverTransport = null;
    }

    private void HandleRawMessageReceived(string rawMessage)
    {
        ChatMessageData messageData = JsonPacketSerializer.Deserialize<ChatMessageData>(rawMessage);

        if (messageData == null)
        {
            SetStatus("Failed to parse incoming message.");
            return;
        }

        if (MessageRepository.Instance != null)
        {
            MessageRepository.Instance.AddMessage(messageData);
        }

        if (MainThreadDispatcher.Instance != null)
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnMessageReceived?.Invoke(messageData);
            });
        }
        else
        {
            OnMessageReceived?.Invoke(messageData);
        }
    }

    private void HandleClientTransportConnected()
    {
        SetStatus("Connected successfully.");

        if (!hasRaisedConnectedEvent)
        {
            hasRaisedConnectedEvent = true;
            OnConnected?.Invoke();
        }
    }

    private void HandleClientTransportDisconnected()
    {
        SetStatus("Disconnected from server.");
        OnDisconnected?.Invoke();
    }

    private void HandleClientTransportError(string message)
    {
        SetStatus(message);
    }

    private void HandleServerTransportDisconnected()
    {
        Debug.Log("[CHAT NETWORK] Server transport disconnected.");
    }

    private void HandleServerTransportError(string message)
    {
        Debug.LogWarning($"[CHAT NETWORK][SERVER] {message}");
    }

    private void SetStatus(string message)
    {
        Debug.Log($"[CHAT NETWORK] {message}");

        if (MainThreadDispatcher.Instance != null)
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnStatusChanged?.Invoke(message);
            });
        }
        else
        {
            OnStatusChanged?.Invoke(message);
        }
    }

    private void OnApplicationQuit()
    {
        Shutdown();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Shutdown();
            Instance = null;
        }
    }
}