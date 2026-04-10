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
    [SerializeField] private LayoutElement leftSpacer;
    [SerializeField] private LayoutElement rightSpacer;

    [Header("Colors")]
    [SerializeField] private Color myMessageColor = new Color(0.20f, 0.45f, 0.20f, 1f);
    [SerializeField] private Color otherMessageColor = new Color(0.18f, 0.18f, 0.18f, 1f);
    [SerializeField] private Color myHeaderColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    [SerializeField] private Color otherHeaderColor = new Color(0.92f, 0.92f, 0.92f, 1f);

    [SerializeField] private Color myBodyColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color otherBodyColor = new Color(1f, 1f, 1f, 1f);

    [SerializeField] private Color myReplyTextColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private Color otherReplyTextColor = new Color(1f, 1f, 1f, 1f);

    [SerializeField] private Image replyPreviewBackground;
    [SerializeField] private Color myReplyBackgroundColor = new Color(0.82f, 0.82f, 0.82f, 1f);
    [SerializeField] private Color otherReplyBackgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);

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

        if (leftSpacer != null)
        {
            leftSpacer.ignoreLayout = isMine;
            leftSpacer.flexibleWidth = isMine ? 0f : 1f;
        }

        if (rightSpacer != null)
        {
            rightSpacer.ignoreLayout = !isMine;
            rightSpacer.flexibleWidth = isMine ? 1f : 0f;
        }

        if (bubbleBackground != null)
        {
            bubbleBackground.color = isMine ? myMessageColor : otherMessageColor;
        }

        if (headerText != null)
        {
            headerText.color = isMine ? myHeaderColor : otherHeaderColor;
        }

        if (bodyText != null)
        {
            bodyText.color = isMine ? myBodyColor : otherBodyColor;
        }

        if (replyPreviewText != null)
        {
            replyPreviewText.color = isMine ? myReplyTextColor : otherReplyTextColor;
        }

        if (replyPreviewBackground != null)
        {
            replyPreviewBackground.color = isMine ? myReplyBackgroundColor : otherReplyBackgroundColor;
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