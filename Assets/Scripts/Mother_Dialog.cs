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

    private MonoBehaviour movementScript; // Скрипт ходьбы Игоря
    private MonoBehaviour inputScript;    // Скрипт крутилки камеры Игоря

    void Start()
    {
        // Ищем компоненты управления на игроке, чтобы остановить его во время беседы
        if (playerObject != null)
        {
            movementScript = playerObject.GetComponent("FirstPersonMovement") as MonoBehaviour;
            inputScript = playerObject.GetComponent("Input") as MonoBehaviour;
        }

        // При старте сцены прячем панель диалога и экран конца игры
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // ПРОВЕРКА СОСТОЯНИЯ: Если кассир в магазине оставил флаг покупки, значит это финал
        if (PlayerPrefs.HasKey("BoughtItems") && PlayerPrefs.GetInt("BoughtItems") == 1)
        {
            isFinalDialogue = true; // Переключаем маму в режим финального диалога
        }
    }

    void Update()
    {
        if (playerObject == null) return;

        // Если игра уже завершена и горит экран Game Over, блокируем всё
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;

        // Считаем расстояние до Игоря
        float distance = Vector3.Distance(transform.position, playerObject.transform.position);

        // ЕСЛИ ЕЩЕ НЕ ГОВОРИМ: свободно ходим, ждем нажатия F на правильной дистанции
        if (!dialogueStarted)
        {
            if (distance <= triggerDistance && Input.GetKeyDown(KeyCode.F))
            {
                StartConversation();
            }
        }
        // ЕСЛИ ДИАЛОГ ИДЕТ: листаем реплики строго по кнопке F
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

        if (dialoguePanel != null) dialoguePanel.SetActive(true); // Включаем плашку
        SetPlayerControl(false); // Замораживаем Игоря, чтобы не ушел посреди речи

        DisplayCurrentLine(); // Показываем первую строчку
    }

    // Листание реплик на кнопку F
    void AdvanceConversation()
    {
        currentLineIndex++;

        // Выбираем правильный массив реплик (финал или старт)
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
            characterText.text = isFinalDialogue ? finalLines[currentLineIndex] : startLines[currentLineIndex];
        }
    }

    // Завершение разговора
    void EndConversation()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false); // Прячем плашку

        if (!isFinalDialogue)
        {
            // ЕСЛИ НАЧАЛО ИГРЫ: автоматически загружаем сцену магазина ShopScene
            SceneManager.LoadScene("ShopScene");
        }
        else
        {
            // ЕСЛИ ФИНАЛ ИГРЫ: включаем панель конца игры Game Over
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                // Освобождаем курсор мыши для клика по кнопке
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    // МЕТОД ДЛЯ КНОПКИ ВНУТРИ ПАНЕЛИ GAME OVER
    public void ClickExitToMenuFromGameOver()
    {
        // Полностью очищаем память от временных флагов, чтобы при Новой игре всё началось сначала
        PlayerPrefs.DeleteKey("BoughtItems");
        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.Save();

        // Уходим обратно в главное меню
        SceneManager.LoadScene("MainMenuScene");
    }

    // Включение/выключение управления игроком и показ мышки
    private void SetPlayerControl(bool state)
    {
        if (movementScript != null) movementScript.enabled = state;
        if (inputScript != null) inputScript.enabled = state;

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
}
