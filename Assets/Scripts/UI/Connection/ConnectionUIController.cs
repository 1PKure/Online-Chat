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

    [Header("Scene Names")]
    [SerializeField] private string chatSceneName = "ChatScene";
    [SerializeField] private string serverSceneName = "ServerScene";

    private void Start()
    {
        InitializeUI();

        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OnConnectButtonPressed);
        }
    }

    private void OnDestroy()
    {
        if (connectButton != null)
        {
            connectButton.onClick.RemoveListener(OnConnectButtonPressed);
        }
    }

    private void InitializeUI()
    {
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

        switch (config.Mode)
        {
            case ConnectionMode.Client:
            case ConnectionMode.Host:
                SceneManager.LoadScene(chatSceneName);
                break;

            case ConnectionMode.DedicatedServer:
                SceneManager.LoadScene(serverSceneName);
                break;

            default:
                SetStatus("Unsupported connection mode.");
                break;
        }
    }

    private bool TryBuildConnectionConfig(out ConnectionConfig config, out string errorMessage)
    {
        config = null;
        errorMessage = string.Empty;

        string ipAddress = ipInputField != null ? ipInputField.text.Trim() : string.Empty;
        string portText = portInputField != null ? portInputField.text.Trim() : string.Empty;
        string userName = userNameInputField != null ? userNameInputField.text.Trim() : string.Empty;

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

        ConnectionMode mode = (ConnectionMode)connectionModeDropdown.value;
        TransportType transportType = (TransportType)transportTypeDropdown.value;

        if (mode == ConnectionMode.DedicatedServer)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = "0.0.0.0";
            }

            userName = "DedicatedServer";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                errorMessage = "IP address cannot be empty.";
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
        }

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
        Debug.LogWarning(message);
    }
}