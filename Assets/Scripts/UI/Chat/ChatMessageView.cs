using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatMessageView : MonoBehaviour, IPointerClickHandler
{
    [Header("Main UI")]
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Reply UI")]
    [SerializeField] private GameObject replyPreviewRoot;
    [SerializeField] private TMP_Text replyPreviewText;

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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentMessageData == null)
        {
            return;
        }

        OnMessageSelected?.Invoke(currentMessageData);
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
            replyPreviewText.text = " ↪ Reply to message";
            return;
        }

        string previewText = repliedMessage.Text;

        if (previewText.Length > 30)
        {
            previewText = previewText.Substring(0, 30) + "...";
        }

       replyPreviewText.text = $"↪ Reply to {repliedMessage.SenderName}\n{previewText}";
    }
}