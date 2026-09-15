using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenController : MonoBehaviour
{
    public Slider progressBar;
    public Text loadingText;

    void Start()
    {
        int sceneIndex = GlobalData.SharedInstance.nextSceneIndex;
        StartCoroutine(LoadScene(sceneIndex));
    }

    IEnumerator LoadScene(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float displayProgress = 0f;

        while (displayProgress < 1f)
        {
            float realProgress = operation.progress / 0.9f;

            displayProgress += Time.deltaTime * 0.5f;

            float finalProgress = Mathf.Min(displayProgress, realProgress);

            if (progressBar != null)
                progressBar.value = finalProgress;

            if (loadingText != null)
                loadingText.text = "Загрузка... " + (int)(finalProgress * 100) + "%";

            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        if (loadingText != null)
            loadingText.text = "Готово! Нажмите любую клавишу";

        while (!Input.anyKeyDown)
            yield return null;

        operation.allowSceneActivation = true;
    }
}