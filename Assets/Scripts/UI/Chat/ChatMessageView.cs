using TMPro;
using UnityEngine;

public class ChatMessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text bodyText;

    public void Setup(ChatMessageData messageData)
    {
        if (headerText != null)
        {
            headerText.text = $"{messageData.SenderName} - {messageData.Timestamp}";
        }

        if (bodyText != null)
        {
            bodyText.text = messageData.Text;
        }
    }
}