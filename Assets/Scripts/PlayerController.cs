using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float speed = 10f;
    private Rigidbody rb;

    [Header("Сбор предметов")]
    public int totalPickups;
    private int count;

    [Header("UI")]
    public Text countText;
    public Text winText;
    public Text timerText;

    [Header("Ветер")]
    public Vector3 windForce = new Vector3(2f, 0f, 0f);

    private float timer;
    private bool isGameOver;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        timer = 0f;
        isGameOver = false;

        if (totalPickups == 0)
            totalPickups = GameObject.FindGameObjectsWithTag("PickUp").Length;

        UpdateCountText();
        if (winText != null) winText.text = "";
    }

    void Update()
    {
        if (!isGameOver)
        {
            timer += Time.deltaTime;
            if (timerText != null)
                timerText.text = "Время: " + timer.ToString("F1") + " с";
        }

        if (transform.position.y < -5f)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(h, 0f, v);
        rb.AddForce(movement * speed);
        rb.AddForce(windForce);
    }

    void OnTriggerEnter(Collider other)
    {
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
                boostDir = Camera.main.transform.forward;
            boostDir.y = 0f;
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
            countText.text = "Собрано: " + count + " / " + totalPickups;

        if (count >= totalPickups && totalPickups > 0 && !isGameOver)
        {
            isGameOver = true;
            StartCoroutine(WinSequence());
        }
    }

    IEnumerator WinSequence()
    {
        for (int i = 5; i > 0; i--)
        {
            if (winText != null)
                winText.text = "Победа!\nСледующий уровень через: " + i;
            yield return new WaitForSeconds(1f);
        }

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else if (winText != null)
            winText.text = "Все уровни пройдены!";
    }
}