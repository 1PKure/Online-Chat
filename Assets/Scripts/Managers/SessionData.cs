using UnityEngine;

public class SessionData : MonoBehaviour
{
    public static SessionData Instance { get; private set; }

    public ConnectionConfig CurrentConfig { get; private set; }

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

    public void SetConfig(ConnectionConfig config)
    {
        CurrentConfig = config;
    }

    public bool HasConfig()
    {
        return CurrentConfig != null;
    }

    public void ClearConfig()
    {
        CurrentConfig = null;
    }
}