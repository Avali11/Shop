using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Cassier_Dialogue : MonoBehaviour
{
    [Header("Настройки Игрока")]
    public GameObject playerObject; // Сюда в инспекторе перетащи игрока (Игоря)
    public float triggerDistance = 2.5f; // Дистанция, на которой можно начать разговор

    [Header("UI Элементы Диалога")]
    public GameObject dialoguePanel; // Панелька диалога (которая появляется)
    public TextMeshProUGUI characterText; // Текст внутри панельки, куда пишутся реплики

    // Массив (список) реплик для диалога. Ровно 10 штук, по одной на каждый клик.
    private string[] dialogueLines = new string[]
    {
        "Кассир: Здравствуйте! Вы готовы оплатить ваши покупки?",
        "Игорь: Да, здравствуйте. Вот всё, что я выбрал в магазине.",
        "Кассир: Хорошо, давайте пробивать товары. Пакет вам нужен?",
        "Игорь: Нет, спасибо, у меня свой рюкзак.",
        "Кассир: Карту нашего магазина будете сканировать?",
        "Игорь: Нет, у меня нет вашей бонусной карты.",
        "Кассир: Очень жаль, сегодня могли быть хорошие скидки.",
        "Кассир: Итак, с вас за всё ровно 1500 рублей.",
        "Игорь: Оплатил картой. Проверяйте, прошла оплата?",
        "Кассир: Да, всё отлично! Спасибо за покупку, хорошего дня!"
    };

    private int currentLineIndex = 0; // Номер реплики, которую мы сейчас читаем
    private bool dialogueStarted = false; // Флаг: начался ли диалог

    private MonoBehaviour movementScript; // Скрипт ходьбы игрока
    private MonoBehaviour inputScript;    // Скрипт ввода игрока

    void Start()
    {
        // Ищем компоненты управления на игроке, чтобы отключить их во время болтовни
        if (playerObject != null)
        {
            movementScript = playerObject.GetComponent("FirstPersonMovement") as MonoBehaviour;
            inputScript = playerObject.GetComponent("Input") as MonoBehaviour;
        }

        // При старте игры обязательно прячем панель диалога
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (playerObject == null) return;

        // Считаем расстояние между кассиром и игроком
        float distance = Vector3.Distance(transform.position, playerObject.transform.position);

        // ЕСЛИ ДИАЛОГ ЕЩЕ НЕ НАЧАТ: проверяем расстояние и нажатие кнопки F
        if (!dialogueStarted)
        {
            if (distance <= triggerDistance && Input.GetKeyDown(KeyCode.F))
            {
                StartDialogue();
            }
        }
        // ЕСЛИ ДИАЛОГ УЖЕ ИДЕТ: теперь тоже ждем нажатия кнопки F, чтобы листать дальше
        else
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                AdvanceDialogue();
            }
        }
    }


    // Метод, который запускает разговор
    void StartDialogue()
    {
        dialogueStarted = true;
        currentLineIndex = 0; // Начинаем с самой первой реплики (ноль)

        if (dialoguePanel != null) dialoguePanel.SetActive(true); // Показываем панель диалога
        SetPlayerControl(false); // Стопаем Игоря (чтобы не убежал во время разговора)

        DisplayCurrentLine(); // Показываем первую строчку текста
    }

    // Метод, который переключает реплики по клику мышки
    void AdvanceDialogue()
    {
        currentLineIndex++; // Переходим к следующей строчке (увеличиваем индекс на 1)

        // Если мы прочитали все 10 строчек, то завершаем диалог и уходим на другую сцену
        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogueAndLoadScene();
        }
        else
        {
            DisplayCurrentLine(); // Если реплики еще есть, показываем следующую
        }
    }

    // Метод просто выводит текущую реплику на экран
    void DisplayCurrentLine()
    {
        if (characterText != null)
        {
            characterText.text = dialogueLines[currentLineIndex];
        }
    }

    // Конец диалога: загружаем сцену дома
    void EndDialogueAndLoadScene()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Оставляем в памяти невидимую "записку" для мамы, что продукты куплены!
        PlayerPrefs.SetInt("BoughtItems", 1);
        PlayerPrefs.Save();

        // Перекидываем игрока обратно домой в HomeScene
        SceneManager.LoadScene("HomeScene");
    }

    // Включение/выключение управления игроком и показ мышки
    private void SetPlayerControl(bool state)
    {
        if (movementScript != null) movementScript.enabled = state;
        if (inputScript != null) inputScript.enabled = state;

        if (!state)
        {
            // Если диалог идет: освобождаем мышку, чтобы кликать по экрану
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Если диалог кончился: прячем мышку обратно в центр экрана
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
