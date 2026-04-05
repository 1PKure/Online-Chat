using System;

[Serializable]
public class ChatMessageData
{
    public string MessageId;
    public string SenderId;
    public string SenderName;
    public string Text;
    public string ReplyToMessageId;
    public string Timestamp;
}