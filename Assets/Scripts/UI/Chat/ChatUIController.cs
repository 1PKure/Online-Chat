using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text sessionInfoText;
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform messagesContainer;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ChatMessageView messageViewPrefab;

    [Header("Reply UI")]
    [SerializeField] private GameObject replyPanel;
    [SerializeField] private TMP_Text replyPreviewText;
    [SerializeField] private Button cancelReplyButton;

    [Header("Managers")]
    [SerializeField] private ChatNetworkManager chatNetworkManager;

    private ChatMessageData selectedReplyTarget;

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

        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonPressed);
        }

        if (messageInputField != null)
        {
            messageInputField.onSubmit.AddListener(HandleInputSubmit);
        }


        if (cancelReplyButton != null)
        {
            cancelReplyButton.onClick.AddListener(ClearReplySelection);
        }

        SetReplyPanelVisible(false);

        chatNetworkManager.OnMessageReceived += HandleMessageReceived;
        chatNetworkManager.OnStatusChanged += HandleStatusChanged;
        chatNetworkManager.OnConnected += HandleConnected;
        chatNetworkManager.OnDisconnected += HandleDisconnected;
    }
    private void Update()
    {
        if (messageInputField == null)
        {
            return;
        }

        if (!messageInputField.isFocused)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSendButtonPressed();
        }
    }

    private void HandleInputSubmit(string submittedText)
    {
        if (string.IsNullOrWhiteSpace(submittedText))
        {
            return;
        }

        OnSendButtonPressed();
    }


    private void OnDestroy()
    {
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnSendButtonPressed);
        }

        if (cancelReplyButton != null)
        {
            cancelReplyButton.onClick.RemoveListener(ClearReplySelection);
        }

        if (messageInputField != null)
        {
            messageInputField.onSubmit.RemoveListener(HandleInputSubmit);
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

        string replyToMessageId = selectedReplyTarget != null
            ? selectedReplyTarget.MessageId
            : string.Empty;

        chatNetworkManager.SendChatMessage(text, replyToMessageId);

        messageInputField.text = string.Empty;
        messageInputField.ActivateInputField();

        ClearReplySelection();
    }

    private void HandleMessageReceived(ChatMessageData messageData)
    {
        if (messageViewPrefab == null || messagesContainer == null)
        {
            SetStatus("Message UI references are missing.");
            return;
        }

        MessageRepository.Instance?.AddMessage(messageData);

        ChatMessageView messageView = Instantiate(messageViewPrefab, messagesContainer);
        messageView.Setup(messageData);
        messageView.OnMessageSelected += HandleMessageSelected;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)messagesContainer);

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void HandleMessageSelected(ChatMessageData messageData)
    {
        selectedReplyTarget = messageData;
        RefreshReplyPanel();
    }

    private void RefreshReplyPanel()
    {
        bool hasReplyTarget = selectedReplyTarget != null;
        SetReplyPanelVisible(hasReplyTarget);

        if (!hasReplyTarget || replyPreviewText == null)
        {
            return;
        }

        string previewText = selectedReplyTarget.Text;

        if (previewText.Length > 40)
        {
            previewText = previewText.Substring(0, 40) + "...";
        }

        replyPreviewText.text = $"Replying to {selectedReplyTarget.SenderName}: {previewText}";
    }

    private void ClearReplySelection()
    {
        selectedReplyTarget = null;
        SetReplyPanelVisible(false);
    }

    private void SetReplyPanelVisible(bool isVisible)
    {
        if (replyPanel != null)
        {
            replyPanel.SetActive(isVisible);
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
        Debug.Log($"[CHAT UI] {message}");
    }
}