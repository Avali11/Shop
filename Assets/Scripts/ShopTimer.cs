using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopTimer : MonoBehaviour
{
    [Header("UI Элемент для отображения времени")]
    public TextMeshProUGUI timerText;

    [Header("Настройки времени (в секундах)")]
    public float timeRemaining = 600f; // 10 минут = 600 секунд

    private string homeSceneName = "GameScene";
    private bool timerIsRunning = false;

    void Start()
    {
        timerIsRunning = true;
    }

    void Update()
    {
        // === ПРЯМОЙ ЭФИР ДЛЯ БАБЫ ЗИНЫ ===
        if (PlayerPrefs.HasKey("ReduceTimeFromZina"))
        {
            timeRemaining -= 120f; // −2 минуты
            PlayerPrefs.DeleteKey("ReduceTimeFromZina");
            PlayerPrefs.Save();
        }

        // === ПРЯМОЙ ЭФИР ДЛЯ ДЕВОЧКИ ЛЕРЫ ===
        if (PlayerPrefs.HasKey("ReduceTimeFromLera"))
        {
            timeRemaining -= 120f; // −2 минуты (и для кассы, и для решётки одинаково)
            PlayerPrefs.DeleteKey("ReduceTimeFromLera");
            PlayerPrefs.Save();
        }

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Время вышло!");
                timeRemaining = 0;
                timerIsRunning = false;

                TimeOverTransition();
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay < 0) timeToDisplay = 0;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TimeOverTransition()
    {
        PlayerPrefs.SetInt("TimeIsOver", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(homeSceneName);
    }
}