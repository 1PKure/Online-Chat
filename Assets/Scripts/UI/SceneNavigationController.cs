using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationController : MonoBehaviour
{
    [SerializeField] private string connectionSceneName = "ConnectionScene";

    public void BackToConnectionScene()
    {
        if (ChatNetworkManager.Instance != null)
        {
            ChatNetworkManager.Instance.Shutdown();
            Destroy(ChatNetworkManager.Instance.gameObject);
        }

        if (SessionData.Instance != null)
        {
            SessionData.Instance.ClearConfig();
        }

        SceneManager.LoadScene(connectionSceneName);
    }

    public void ExitApplication()
    {
        if (ChatNetworkManager.Instance != null)
        {
            ChatNetworkManager.Instance.Shutdown();
        }

        if (SessionData.Instance != null)
        {
            SessionData.Instance.ClearConfig();
        }

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}