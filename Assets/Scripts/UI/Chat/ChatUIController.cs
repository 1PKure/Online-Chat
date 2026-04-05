using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text sessionInfoText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform messagesContainer;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ChatMessageView messageViewPrefab;

    [Header("Managers")]
    [SerializeField] private ChatNetworkManager chatNetworkManager;

    private void Start()
    {
        if (SessionData.Instance == null || !SessionData.Instance.HasConfig())
        {
            SetStatus("Session config was not found.");
            return;
        }

        if (chatNetworkManager == null)
        {
            SetStatus("ChatNetworkManager reference is missing.");
            return;
        }

        ConnectionConfig config = SessionData.Instance.CurrentConfig;

        if (sessionInfoText != null)
        {
            sessionInfoText.text =
                $"Mode: {config.Mode} | Protocol: {config.TransportType} | " +
                $"IP: {config.IPAddress} | Port: {config.Port} | User: {config.UserName}";
        }

        sendButton.onClick.AddListener(OnSendButtonPressed);

        chatNetworkManager.OnMessageReceived += HandleMessageReceived;
        chatNetworkManager.OnStatusChanged += HandleStatusChanged;
        chatNetworkManager.OnConnected += HandleConnected;
        chatNetworkManager.OnDisconnected += HandleDisconnected;
    }

    private void OnDestroy()
    {
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnSendButtonPressed);
        }

        if (chatNetworkManager != null)
        {
            chatNetworkManager.OnMessageReceived -= HandleMessageReceived;
            chatNetworkManager.OnStatusChanged -= HandleStatusChanged;
            chatNetworkManager.OnConnected -= HandleConnected;
            chatNetworkManager.OnDisconnected -= HandleDisconnected;
        }
    }

    private void OnSendButtonPressed()
    {
        if (messageInputField == null)
        {
            return;
        }

        string text = messageInputField.text;

        if (string.IsNullOrWhiteSpace(text))
        {
            SetStatus("Cannot send an empty message.");
            return;
        }

        chatNetworkManager.SendChatMessage(text);
        messageInputField.text = string.Empty;
        messageInputField.ActivateInputField();
    }

    private void HandleMessageReceived(ChatMessageData messageData)
    {
        if (messageViewPrefab == null || messagesContainer == null)
        {
            SetStatus("Message UI references are missing.");
            return;
        }

        ChatMessageView messageView = Instantiate(messageViewPrefab, messagesContainer);
        messageView.Setup(messageData);

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void HandleStatusChanged(string message)
    {
        SetStatus(message);
    }

    private void HandleConnected()
    {
        SetStatus("Connection established.");
    }

    private void HandleDisconnected()
    {
        SetStatus("Connection closed.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log($"[CHAT UI] {message}");
    }
}