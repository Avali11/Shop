
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Cassier_Dialogue : MonoBehaviour
{
    public static bool IsDialogueActive { get; private set; }

    [Header("Настройки игрока")]
    public GameObject playerObject;
    public float triggerDistance = 2.5f;

    [Header("UI элементы диалога")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI characterText;

    private readonly string[] dialogueLines =
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

    private int currentLineIndex;
    private bool dialogueStarted;
    private bool dialogueCompleted;

    private MonoBehaviour movementScript;
    private MonoBehaviour inputScript;

    private void Start()
    {
        if (playerObject != null)
        {
            movementScript =
                playerObject.GetComponent("FirstPersonMovement")
                as MonoBehaviour;

            inputScript =
                playerObject.GetComponent("Input")
                as MonoBehaviour;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (playerObject == null || dialogueCompleted)
            return;

        // Если кассир уже разговаривает, F относится только к нему.
        if (dialogueStarted)
        {
            if (Input.GetKeyDown(KeyCode.F))
                AdvanceDialogue();

            return;
        }

        // Не начинаем разговор во время диалога Леры.
        if (LeraNPCController.IsDialogueActive)
            return;

        // Не обрабатываем то же нажатие F, которым закончился
        // диалог Леры.
        if (Time.frameCount <=
            LeraNPCController.BlockInteractionsThroughFrame)
            return;

        float distance = Vector3.Distance(
            transform.position,
            playerObject.transform.position
        );

        if (distance <= triggerDistance &&
            Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        if (dialogueStarted ||
            dialogueCompleted ||
            LeraNPCController.IsDialogueActive)
            return;

        dialogueStarted = true;
        IsDialogueActive = true;
        currentLineIndex = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        SetPlayerControl(false);
        DisplayCurrentLine();
    }

    private void AdvanceDialogue()
    {
        if (!dialogueStarted)
            return;

        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
            EndDialogueAndLoadScene();
        else
            DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (characterText != null)
            characterText.text = dialogueLines[currentLineIndex];
    }

    private void EndDialogueAndLoadScene()
    {
        dialogueStarted = false;
        dialogueCompleted = true;
        IsDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        PlayerPrefs.SetInt("BoughtItems", 1);
        PlayerPrefs.Save();

        SetPlayerControl(true);

        SceneManager.LoadScene("HomeScene");
    }

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

    private void OnDisable()
    {
        if (dialogueStarted)
        {
            dialogueStarted = false;
            IsDialogueActive = false;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            SetPlayerControl(true);
        }
    }
}
