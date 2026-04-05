using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectionUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown connectionModeDropdown;
    [SerializeField] private TMP_Dropdown transportTypeDropdown;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Scene Names")]
    [SerializeField] private string chatSceneName = "ChatScene";

    private void Start()
    {
        InitializeUI();
        connectButton.onClick.AddListener(OnConnectButtonPressed);
    }

    private void OnDestroy()
    {
        connectButton.onClick.RemoveListener(OnConnectButtonPressed);
    }

    private void InitializeUI()
    {
        statusText.text = string.Empty;

        if (ipInputField != null && string.IsNullOrWhiteSpace(ipInputField.text))
        {
            ipInputField.text = "127.0.0.1";
        }

        if (portInputField != null && string.IsNullOrWhiteSpace(portInputField.text))
        {
            portInputField.text = "7777";
        }

        if (userNameInputField != null && string.IsNullOrWhiteSpace(userNameInputField.text))
        {
            userNameInputField.text = "Player";
        }
    }

    private void OnConnectButtonPressed()
    {
        if (!TryBuildConnectionConfig(out ConnectionConfig config, out string errorMessage))
        {
            SetStatus(errorMessage);
            return;
        }

        if (SessionData.Instance == null)
        {
            SetStatus("SessionData instance was not found.");
            return;
        }

        SessionData.Instance.SetConfig(config);
        SceneManager.LoadScene(chatSceneName);
    }

    private bool TryBuildConnectionConfig(out ConnectionConfig config, out string errorMessage)
    {
        config = null;
        errorMessage = string.Empty;

        string ipAddress = ipInputField.text.Trim();
        string portText = portInputField.text.Trim();
        string userName = userNameInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            errorMessage = "IP address cannot be empty.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(portText))
        {
            errorMessage = "Port cannot be empty.";
            return false;
        }

        if (!int.TryParse(portText, out int port))
        {
            errorMessage = "Port must be a valid number.";
            return false;
        }

        if (port < 1 || port > 65535)
        {
            errorMessage = "Port must be between 1 and 65535.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            errorMessage = "User name cannot be empty.";
            return false;
        }

        if (userName.Length > 16)
        {
            errorMessage = "User name must be 16 characters or less.";
            return false;
        }

        ConnectionMode mode = (ConnectionMode)connectionModeDropdown.value;
        TransportType transportType = (TransportType)transportTypeDropdown.value;

        config = new ConnectionConfig
        {
            Mode = mode,
            TransportType = transportType,
            IPAddress = ipAddress,
            Port = port,
            UserName = userName
        };

        return true;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.LogWarning(message);
    }
}