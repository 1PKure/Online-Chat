using UnityEngine;

public class DedicatedServerBootstrap : MonoBehaviour
{
    [SerializeField] private ChatNetworkManager networkManager;
    [SerializeField] private int port = 7777;
    [SerializeField] private TransportType protocol = TransportType.TCP;

    private void Start()
    {
        if (networkManager == null)
        {
            Debug.LogError("DedicatedServerBootstrap: ChatNetworkManager reference is missing.");
            return;
        }

        ConnectionConfig config = new ConnectionConfig
        {
            UserName = "DedicatedServer",
            IPAddress = "0.0.0.0",
            Port = port,
            TransportType = protocol,
            Mode = ConnectionMode.DedicatedServer
        };

        networkManager.StartWithConfig(config);
    }
}