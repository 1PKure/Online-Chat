using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatSceneUIAdapter : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool forceMobileInEditor;

    [Header("Root References")]
    [SerializeField] private RectTransform chatRoot;
    [SerializeField] private VerticalLayoutGroup chatRootLayout;

    [Header("Backgrounds")]
    [SerializeField] private GameObject desktopBackground;
    [SerializeField] private GameObject mobileBackground;

    [Header("Panels")]
    [SerializeField] private RectTransform replyPanelRect;
    [SerializeField] private RectTransform inputPanelRect;

    [Header("Reply Panel")]
    [SerializeField] private Button cancelReplyButton;
    [SerializeField] private TMP_Text replyLabel;

    [Header("Input Panel")]
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendButton;

    [Header("Desktop Settings")]
    [SerializeField] private int desktopMargin = 40;
    [SerializeField] private float desktopSpacing = 10f;
    [SerializeField] private float desktopReplyHeight = 50f;
    [SerializeField] private float desktopInputPanelHeight = 60f;
    [SerializeField] private float desktopButtonHeight = 45f;
    [SerializeField] private int desktopReplyFontSize = 20;
    [SerializeField] private int desktopInputFontSize = 20;

    [Header("Mobile Settings")]
    [SerializeField] private int mobileMargin = 20;
    [SerializeField] private float mobileSpacing = 16f;
    [SerializeField] private float mobileReplyHeight = 80f;
    [SerializeField] private float mobileInputPanelHeight = 90f;
    [SerializeField] private float mobileButtonHeight = 70f;
    [SerializeField] private int mobileReplyFontSize = 28;
    [SerializeField] private int mobileInputFontSize = 28;

    private void Start()
    {
        ApplyLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        ApplyLayout();
    }

    private void ApplyLayout()
    {
        bool isMobile = IsMobileLayout();

        ApplyBackground(isMobile);
        ApplyRootSettings(isMobile);
        ApplyPanelSettings(isMobile);
        ApplyTextSettings(isMobile);
    }

    private bool IsMobileLayout()
    {
#if UNITY_EDITOR
        if (forceMobileInEditor)
        {
            return true;
        }
#endif
        return Application.isMobilePlatform;
    }

    private void ApplyBackground(bool isMobile)
    {
        if (desktopBackground != null)
        {
            desktopBackground.SetActive(!isMobile);
        }

        if (mobileBackground != null)
        {
            mobileBackground.SetActive(isMobile);
        }
    }

    private void ApplyRootSettings(bool isMobile)
    {
        if (chatRoot != null)
        {
            int margin = isMobile ? mobileMargin : desktopMargin;

            chatRoot.offsetMin = new Vector2(margin, margin);
            chatRoot.offsetMax = new Vector2(-margin, -margin);
        }

        if (chatRootLayout != null)
        {
            chatRootLayout.spacing = isMobile ? mobileSpacing : desktopSpacing;
        }
    }

    private void ApplyPanelSettings(bool isMobile)
    {
        SetRectHeight(replyPanelRect, isMobile ? mobileReplyHeight : desktopReplyHeight);
        SetRectHeight(inputPanelRect, isMobile ? mobileInputPanelHeight : desktopInputPanelHeight);

        if (messageInputField != null)
        {
            SetRectHeight(messageInputField.GetComponent<RectTransform>(), isMobile ? mobileButtonHeight : desktopButtonHeight);
        }

        if (sendButton != null)
        {
            SetRectHeight(sendButton.GetComponent<RectTransform>(), isMobile ? mobileButtonHeight : desktopButtonHeight);
        }

        if (cancelReplyButton != null)
        {
            SetRectHeight(cancelReplyButton.GetComponent<RectTransform>(), isMobile ? mobileButtonHeight : desktopButtonHeight);
        }
    }

    private void ApplyTextSettings(bool isMobile)
    {
        if (replyLabel != null)
        {
            replyLabel.fontSize = isMobile ? mobileReplyFontSize : desktopReplyFontSize;
        }

        if (messageInputField != null && messageInputField.textComponent != null)
        {
            messageInputField.textComponent.fontSize = isMobile ? mobileInputFontSize : desktopInputFontSize;
        }

        if (messageInputField != null && messageInputField.placeholder is TMP_Text placeholderText)
        {
            placeholderText.fontSize = isMobile ? mobileInputFontSize : desktopInputFontSize;
        }
    }

    private void SetRectHeight(RectTransform rectTransform, float height)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}