using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    [Header("Кнопки уровней (включая заглушки)")]
    public Button[] levelButtons;

    [Header("Тексты со звездами (включая заглушки)")]
    public Text[] starsTexts;

    void Start()
    {
        if (levelButtons == null || levelButtons.Length == 0)
        {
            Debug.LogError("Заполните массив Level Buttons в инспекторе Canvas!");
            return;
        }

        int unlocked = GlobalData.SharedInstance.unlockedLevels;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i;

            if (i < 3)
            {
                if (i < unlocked)
                {
                    levelButtons[i].interactable = true;
                    levelButtons[i].onClick.RemoveAllListeners();
                    levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));

                    if (starsTexts != null && i < starsTexts.Length && starsTexts[i] != null)
                    {
                        float time = GlobalData.SharedInstance.levelTimes[i];
                        starsTexts[i].text = GetStarsString(time);
                    }
                }
                else
                {
                    levelButtons[i].interactable = false;
                    if (starsTexts != null && i < starsTexts.Length && starsTexts[i] != null)
                    {
                        starsTexts[i].text = "🔒";
                    }
                }
            }
            else
            {
                levelButtons[i].interactable = false;

                Text btnText = null;
                foreach (Text t in levelButtons[i].GetComponentsInChildren<Text>())
                {
                    if (t.gameObject.name == "Text (Legacy)")
                    {
                        btnText = t;
                        break;
                    }
                }

                if (btnText != null)
                {
                    btnText.text = "Скоро...";
                    btnText.color = new Color(1f, 1f, 1f, 0.3f);
                }

                if (starsTexts != null && i < starsTexts.Length && starsTexts[i] != null)
                {
                    starsTexts[i].text = "🔒";
                    starsTexts[i].color = new Color(1f, 1f, 1f, 0.2f);
                }
            }
        }
    }

    public void LoadLevel(int levelIndex)
    {
        GlobalData.SharedInstance.nextSceneIndex = levelIndex + 3;
        SceneManager.LoadScene("LoadingScreen");
    }

    string GetStarsString(float time)
    {
        if (time <= 0f) return "☆☆☆";
        if (time < 20f) return "★★★";
        if (time < 45f) return "★★☆";
        return "★☆☆";
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}