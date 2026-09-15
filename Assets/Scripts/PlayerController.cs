using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 10f;
    private Rigidbody rb;

    [Header("Настройки ветра")]
    public Vector3 windForce = new Vector3(2f, 0f, 0f);

    [Header("Параметры уровня")]
    [Tooltip("Индекс текущего уровня: 0 = Уровень 1, 1 = Уровень 2, 2 = Уровень 3")]
    public int currentLevelIndex = 0;

    [HideInInspector]
    public int totalPickups;
    private int count;

    [Header("UI Элементы (Legacy Text)")]
    public Text countText;
    public Text winText;
    public Text timerText;

    private float timer;
    private bool isGameOver;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        timer = 0f;
        isGameOver = false;

        int activeBuildIndex = SceneManager.GetActiveScene().buildIndex;

        currentLevelIndex = activeBuildIndex - 3;

        if (currentLevelIndex < 0)
        {
            currentLevelIndex = 0;
        }

        Debug.Log($"[PlayerController] Динамический расчет: BuildIndex = {activeBuildIndex} -> LevelIndex = {currentLevelIndex}");

        totalPickups = GameObject.FindGameObjectsWithTag("PickUp").Length;

        UpdateCountText();
        if (winText != null)
            winText.text = "";
    }

    void Update()
    {
        if (!isGameOver)
        {
            timer += Time.deltaTime;
            if (timerText != null)
                timerText.text = "Время: " + timer.ToString("F1") + " с";
        }

        if (transform.position.y < -5f && !isGameOver)
        {
            RestartLevel();
        }
    }

    void FixedUpdate()
    {
        if (isGameOver) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 movement = camForward * v + camRight * h;
        rb.AddForce(movement * speed);

        rb.AddForce(windForce);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;

        if (other.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count++;
            UpdateCountText();
        }

        if (other.CompareTag("BoostZone"))
        {
            Vector3 boostDir = rb.velocity.normalized;
            if (boostDir.magnitude < 0.1f)
            {
                boostDir = Camera.main.transform.forward;
                boostDir.y = 0f;
            }
            rb.AddForce(boostDir.normalized * 10f, ForceMode.VelocityChange);
        }

        if (other.CompareTag("SlowZone"))
        {
            rb.velocity *= 0.3f;
        }
    }

    void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = "Собрано: " + count + " / " + totalPickups;
        }

        if (count >= totalPickups && totalPickups > 0 && !isGameOver)
        {
            isGameOver = true;
            StartCoroutine(WinSequence());
        }
    }

    IEnumerator WinSequence()
    {
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        float finalTime = timer;

        if (currentLevelIndex >= 0 && currentLevelIndex < GlobalData.SharedInstance.levelTimes.Length)
        {
            float bestTime = GlobalData.SharedInstance.levelTimes[currentLevelIndex];
            if (bestTime <= 0f || finalTime < bestTime)
            {
                GlobalData.SharedInstance.levelTimes[currentLevelIndex] = finalTime;
            }
        }

        int nextLevelNum = currentLevelIndex + 2;
        if (nextLevelNum > GlobalData.SharedInstance.unlockedLevels && nextLevelNum <= 3)
        {
            GlobalData.SharedInstance.unlockedLevels = nextLevelNum;
        }

        string stars = "";
        if (finalTime < 20f) stars = "★★★ (Идеально!)";
        else if (finalTime < 45f) stars = "★★☆ (Хорошо)";
        else stars = "★☆☆ (Пройдено)";

        for (int i = 5; i > 0; i--)
        {
            if (winText != null)
            {
                winText.text = $"ПОБЕДА!\nВремя: {finalTime:F1} сек\nОценка: {stars}\n\nВыход в меню через: {i}";
            }
            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene("LevelSelect");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}