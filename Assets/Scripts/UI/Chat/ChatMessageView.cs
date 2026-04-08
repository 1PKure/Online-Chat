using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatMessageView : MonoBehaviour, IPointerClickHandler
{
    [Header("Main UI")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Reply UI")]
    [SerializeField] private GameObject replyPreviewRoot;
    [SerializeField] private TMP_Text replyPreviewText;

    [Header("Layout")]
    [SerializeField] private RectTransform bubbleRoot;
    [SerializeField] private HorizontalLayoutGroup rowLayout;
    [SerializeField] private Image bubbleBackground;

    [Header("Colors")]
    [SerializeField] private Color myMessageColor = new Color(0.20f, 0.45f, 0.20f, 1f);
    [SerializeField] private Color otherMessageColor = new Color(0.18f, 0.18f, 0.18f, 1f);

    private ChatMessageData currentMessageData;

    public event Action<ChatMessageData> OnMessageSelected;

    public void Setup(ChatMessageData messageData)
    {
        currentMessageData = messageData;

        if (headerText != null)
        {
            headerText.text = $"{messageData.SenderName} - {messageData.Timestamp}";
        }

        if (bodyText != null)
        {
            bodyText.text = messageData.Text;
        }

        RefreshReplyPreview();
        RefreshLayout();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentMessageData == null)
        {
            return;
        }

        OnMessageSelected?.Invoke(currentMessageData);
    }

    private bool IsMine()
    {
        if (currentMessageData == null || SessionData.Instance == null)
        {
            return false;
        }

        return currentMessageData.SenderClientId == SessionData.Instance.ClientId;
    }

    private void RefreshLayout()
    {
        bool isMine = IsMine();

        if (rowLayout != null)
        {
            rowLayout.childAlignment = isMine ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
        }

        if (bubbleRoot != null)
        {
            bubbleRoot.pivot = isMine ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);
        }

        if (bubbleBackground != null)
        {
            bubbleBackground.color = isMine ? myMessageColor : otherMessageColor;
        }
    }

    private void RefreshReplyPreview()
    {
        if (replyPreviewRoot == null)
        {
            return;
        }

        bool hasReplyReference = currentMessageData != null &&
                                 !string.IsNullOrWhiteSpace(currentMessageData.ReplyToMessageId);

        replyPreviewRoot.SetActive(hasReplyReference);

        if (!hasReplyReference)
        {
            return;
        }

        ChatMessageData repliedMessage = MessageRepository.Instance != null
            ? MessageRepository.Instance.GetMessageById(currentMessageData.ReplyToMessageId)
            : null;

        if (replyPreviewText == null)
        {
            return;
        }

        if (repliedMessage == null)
        {
            replyPreviewText.text = "↪ Message not found";
            return;
        }

        string previewText = repliedMessage.Text;

        if (previewText.Length > 30)
        {
            previewText = previewText.Substring(0, 30) + "...";
        }

        replyPreviewText.text = $"<b>{repliedMessage.SenderName}</b>\n{previewText}";
    }
}