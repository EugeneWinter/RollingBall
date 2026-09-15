using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject helpPanel;

    public void StartGame()
    {
        GlobalData.SharedInstance.nextSceneIndex = 2;
        SceneManager.LoadScene("LoadingScreen");
    }

    public void ShowHelp()
    {
        if (helpPanel != null)
            helpPanel.SetActive(true);
    }

    public void HideHelp()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}