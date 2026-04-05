using System;
using System.Threading.Tasks;
using UnityEngine;

public class ChatNetworkManager : MonoBehaviour
{
    public static ChatNetworkManager Instance { get; private set; }

    private TcpChatServer server;
    private TcpChatClient client;

    private string localUserId;

    public event Action<ChatMessageData> OnMessageReceived;
    public event Action<string> OnStatusChanged;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public bool IsConnected => client != null && client.IsConnected;

    public string LocalUserName =>
        SessionData.Instance != null && SessionData.Instance.HasConfig()
            ? SessionData.Instance.CurrentConfig.UserName
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
        await InitializeNetworkAsync();
    }

    private async Task InitializeNetworkAsync()
    {
        if (SessionData.Instance == null || !SessionData.Instance.HasConfig())
        {
            SetStatus("Missing session config.");
            return;
        }

        ConnectionConfig config = SessionData.Instance.CurrentConfig;

        if (config.TransportType != TransportType.TCP)
        {
            SetStatus("Only TCP is implemented in this stage.");
            return;
        }

        localUserId = Guid.NewGuid().ToString();

        client = new TcpChatClient();
        client.OnRawMessageReceived += HandleRawMessageReceived;
        client.OnClientLog += HandleClientLog;
        client.OnDisconnected += HandleClientDisconnected;

        if (config.Mode == ConnectionMode.Host)
        {
            server = new TcpChatServer();
            server.OnServerLog += HandleServerLog;
            server.StartServer(config.Port);

            await Task.Delay(200);
        }

        string connectionAddress = config.Mode == ConnectionMode.Host ? "127.0.0.1" : config.IPAddress;
        bool connected = await client.ConnectAsync(connectionAddress, config.Port);

        if (!connected)
        {
            SetStatus("Failed to connect.");
            return;
        }

        SetStatus("Connected successfully.");
        OnConnected?.Invoke();
    }

    public void SendChatMessage(string text)
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
            SenderId = localUserId,
            SenderName = LocalUserName,
            Text = text,
            ReplyToMessageId = string.Empty,
            Timestamp = DateTime.Now.ToString("HH:mm")
        };

        string json = JsonPacketSerializer.Serialize(messageData);
        client.Send(json);
    }

    public void Shutdown()
    {
        client?.Disconnect();
        server?.StopServer();
    }

    private void HandleRawMessageReceived(string rawMessage)
    {
        ChatMessageData messageData = JsonPacketSerializer.Deserialize<ChatMessageData>(rawMessage);

        if (messageData == null)
        {
            SetStatus("Failed to parse incoming message.");
            return;
        }

        if (MainThreadDispatcher.Instance != null)
        {
            MainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnMessageReceived?.Invoke(messageData);
            });
        }
    }

    private void HandleClientLog(string message)
    {
        SetStatus(message);
    }

    private void HandleServerLog(string message)
    {
        Debug.Log($"[NETWORK MANAGER] {message}");
    }

    private void HandleClientDisconnected()
    {
        SetStatus("Disconnected from server.");
        OnDisconnected?.Invoke();
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