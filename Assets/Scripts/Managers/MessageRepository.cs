using System.Collections.Generic;
using UnityEngine;

public class MessageRepository : MonoBehaviour
{
    public static MessageRepository Instance { get; private set; }

    private readonly List<ChatMessageData> messages = new List<ChatMessageData>();
    private readonly Dictionary<string, ChatMessageData> messagesById = new Dictionary<string, ChatMessageData>();

    public IReadOnlyList<ChatMessageData> Messages => messages;

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

    public void AddMessage(ChatMessageData messageData)
    {
        if (messageData == null || string.IsNullOrWhiteSpace(messageData.MessageId))
        {
            return;
        }

        if (messagesById.ContainsKey(messageData.MessageId))
        {
            return;
        }

        messages.Add(messageData);
        messagesById.Add(messageData.MessageId, messageData);
    }

    public ChatMessageData GetMessageById(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            return null;
        }

        messagesById.TryGetValue(messageId, out ChatMessageData messageData);
        return messageData;
    }

    public void ClearMessages()
    {
        messages.Clear();
        messagesById.Clear();
    }
}