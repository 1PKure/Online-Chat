using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionSceneUIAdapter : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool forceMobileInEditor;

    [Header("Root References")]
    [SerializeField] private RectTransform connectionPanel;
    [SerializeField] private VerticalLayoutGroup connectionPanelLayout;

    [Header("Backgrounds")]
    [SerializeField] private GameObject desktopBackground;
    [SerializeField] private GameObject mobileBackground;

    [Header("Text References")]
    [SerializeField] private TMP_Text welcomeText;

    [Header("Controls")]
    [SerializeField] private TMP_Dropdown connectionModeDropdown;
    [SerializeField] private TMP_Dropdown transportTypeDropdown;
    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconnectButton;

    [Header("Desktop Settings")]
    [SerializeField] private float desktopPanelWidth = 850f;
    [SerializeField] private int desktopFontSize = 28;
    [SerializeField] private float desktopControlHeight = 50f;
    [SerializeField] private float desktopButtonHeight = 55f;
    [SerializeField] private float desktopSpacing = 20f;
    [SerializeField] private int desktopPadding = 20;

    [Header("Mobile Settings")]
    [SerializeField] private float mobilePanelWidth = 680f;
    [SerializeField] private int mobileFontSize = 40;
    [SerializeField] private float mobileControlHeight = 80f;
    [SerializeField] private float mobileButtonHeight = 85f;
    [SerializeField] private float mobileSpacing = 28f;
    [SerializeField] private int mobilePadding = 28;

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
        ApplyPanelSettings(isMobile);
        ApplyTextSettings(isMobile);
        ApplyControlSettings(isMobile);
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

    private void ApplyPanelSettings(bool isMobile)
    {
        if (connectionPanel != null)
        {
            float targetWidth = isMobile ? mobilePanelWidth : desktopPanelWidth;
            connectionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        }

        if (connectionPanelLayout != null)
        {
            connectionPanelLayout.spacing = isMobile ? mobileSpacing : desktopSpacing;
            int padding = isMobile ? mobilePadding : desktopPadding;

            connectionPanelLayout.padding.left = padding;
            connectionPanelLayout.padding.right = padding;
            connectionPanelLayout.padding.top = padding;
            connectionPanelLayout.padding.bottom = padding;
        }
    }

    private void ApplyTextSettings(bool isMobile)
    {
        if (welcomeText != null)
        {
            welcomeText.fontSize = isMobile ? mobileFontSize : desktopFontSize;
        }
    }

    private void ApplyControlSettings(bool isMobile)
    {
        float controlHeight = isMobile ? mobileControlHeight : desktopControlHeight;
        float buttonHeight = isMobile ? mobileButtonHeight : desktopButtonHeight;

        SetDropdownHeight(connectionModeDropdown, controlHeight);
        SetDropdownHeight(transportTypeDropdown, controlHeight);

        SetInputHeight(userNameInputField, controlHeight);
        SetInputHeight(ipInputField, controlHeight);
        SetInputHeight(portInputField, controlHeight);

        if (connectButton != null)
        {
            SetRectHeight(connectButton.GetComponent<RectTransform>(), buttonHeight);
        }
        if (disconnectButton != null)
        {
            SetRectHeight(disconnectButton.GetComponent<RectTransform>(), buttonHeight);
        }
    }

    private void SetDropdownHeight(TMP_Dropdown dropdown, float height)
    {
        if (dropdown == null)
        {
            return;
        }

        SetRectHeight(dropdown.GetComponent<RectTransform>(), height);
    }

    private void SetInputHeight(TMP_InputField inputField, float height)
    {
        if (inputField == null)
        {
            return;
        }

        SetRectHeight(inputField.GetComponent<RectTransform>(), height);
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