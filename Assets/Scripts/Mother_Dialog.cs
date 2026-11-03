
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Mother_Dialog : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject playerObject; // Сюда в инспекторе перетащи Игоря (Mini First Person Controller)
    public float triggerDistance = 2.5f; // Дистанция для начала разговора

    [Header("UI Elements")]
    public GameObject dialoguePanel; // Узкая нижняя панель диалога
    public TextMeshProUGUI characterText; // Текст TextMeshPro внутри панели
    public GameObject gameOverPanel; // Экран GAME OVER, который появится в самом конце

    // 1. Стартовый диалог (Начало игры)
    private string[] startLines = new string[]
    {
        "Мама: Игорь, сынок, ты уже проснулся? Отлично.",
        "Игорь: Да, мам. Что-то нужно сделать?",
        "Мама: Возьми список покупок и сходи, пожалуйста, в магазин.",
        "Мама: Купи всё самое необходимое. Деньги я перевела тебе на карту.",
        "Игорь: Хорошо, я пошел. Скоро вернусь."
    };

    // 2. Финальный диалог (Конец игры)
    private string[] finalLines = new string[]
    {
        "Мама: О, Игорь, ты уже вернулся! Как быстро.",
        "Игорь: Да, мам, всё купил и оплатил на кассе.",
        "Мама: Какой ты молодец, всё правильно выбрал! Спасибо за помощь.",
        "Игорь: Рад помочь, мам!"
    };

    private int currentLineIndex = 0; // Индекс текущей строчки диалога
    private bool dialogueStarted = false; // Идет ли диалог прямо сейчас
    private bool isFinalDialogue = false; // Какой диалог включать: старт или финал
    private bool gameOverActive = false; // Активен ли финальный экран

    private MonoBehaviour movementScript; // Скрипт ходьбы Игоря
    private MonoBehaviour inputScript;    // Скрипт крутилки камеры Игоря

    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockState;
    private float previousTimeScale = 1f;

    void Start()
    {
        // Если игрок не назначен в Inspector, ищем его по тегу Player
        if (playerObject == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");

            if (foundPlayer != null)
                playerObject = foundPlayer;
        }

        // Ищем компоненты управления на игроке, чтобы остановить его во время беседы
        if (playerObject != null)
        {
            movementScript = playerObject.GetComponent("FirstPersonMovement") as MonoBehaviour;
            inputScript = playerObject.GetComponent("Input") as MonoBehaviour;
        }

        // При старте сцены прячем панель диалога и экран конца игры
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Проверяем, купил ли игрок товары в магазине
        if (PlayerPrefs.HasKey("BoughtItems") && PlayerPrefs.GetInt("BoughtItems") == 1)
        {
            isFinalDialogue = true;
        }
    }

    void Update()
    {
        if (playerObject == null) return;

        // Во время Game Over полностью блокируем дальнейшие действия
        if (gameOverActive) return;

        // Считаем расстояние до Игоря
        float distance = Vector3.Distance(transform.position, playerObject.transform.position);

        // Если диалог не начат, ждём нажатия F на нужной дистанции
        if (!dialogueStarted)
        {
            if (distance <= triggerDistance && Input.GetKeyDown(KeyCode.F))
            {
                StartConversation();
            }
        }
        // Если диалог идёт, листаем реплики по кнопке F
        else
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                AdvanceConversation();
            }
        }
    }

    // Запуск разговора
    void StartConversation()
    {
        dialogueStarted = true;
        currentLineIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        SetPlayerControl(false);

        DisplayCurrentLine();
    }

    // Листание реплик на кнопку F
    void AdvanceConversation()
    {
        currentLineIndex++;

        // Выбираем правильный массив реплик
        string[] activeLines = isFinalDialogue ? finalLines : startLines;

        // Если дочитали до конца списков
        if (currentLineIndex >= activeLines.Length)
        {
            EndConversation();
        }
        else
        {
            DisplayCurrentLine();
        }
    }

    // Вывод текста на экран
    void DisplayCurrentLine()
    {
        if (characterText != null)
        {
            characterText.text = isFinalDialogue
                ? finalLines[currentLineIndex]
                : startLines[currentLineIndex];
        }
    }

    // Завершение разговора
    void EndConversation()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (!isFinalDialogue)
        {
            // Если это начало игры, загружаем магазин
            SceneManager.LoadScene("ShopScene");
        }
        else
        {
            // Если это финал, показываем Game Over и замораживаем игру
            ShowGameOver();
        }
    }

    // Показываем финальный экран и блокируем движение и камеру
    void ShowGameOver()
    {
        gameOverActive = true;

        // Запоминаем состояние времени и курсора
        previousTimeScale = Time.timeScale;
        previousCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;

        // Останавливаем игровое время
        Time.timeScale = 0f;

        // Отключаем управление игроком
        SetPlayerControl(false);

        // Показываем экран Game Over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Показываем курсор, чтобы можно было нажать кнопку
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // МЕТОД ДЛЯ КНОПКИ ВНУТРИ ПАНЕЛИ GAME OVER
    public void ClickExitToMenuFromGameOver()
    {
        // Удаляем настоящее сохранение из SQLite
        bool saveCleared = SaveDatabase.ClearSave();

        if (saveCleared)
        {
            Debug.Log("Финал игры: сохранение очищено.");
        }
        else
        {
            Debug.LogWarning("Финал игры: не удалось подтвердить очистку сохранения.");
        }

        // Удаляем временный флаг покупки
        PlayerPrefs.DeleteKey("BoughtItems");
        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.Save();

        // Возвращаем нормальное состояние времени перед сменой сцены
        Time.timeScale = 1f;

        // Оставляем курсор свободным для главного меню
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Уходим обратно в главное меню
        SceneManager.LoadScene("MainMenuScene");
    }

    // Включение/выключение управления игроком и показ мышки
    private void SetPlayerControl(bool state)
    {
        if (movementScript != null)
            movementScript.enabled = state;

        if (inputScript != null)
            inputScript.enabled = state;

        if (!state)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnDestroy()
    {
        // Не оставляем игру на паузе, если объект уничтожается при смене сцены
        Time.timeScale = 1f;
    }
}
