
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class LeraNPCController : MonoBehaviour
{
    public static bool IsDialogueActive { get; private set; }

    // Не даём другому NPC обработать то же нажатие F,
    // которым завершили реплику Леры.
    public static int BlockInteractionsThroughFrame { get; private set; } = -1;

    [Header("Игрок")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Behaviour[] playerControls;

    [Header("Передвижение Леры")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private float chaseDistance = 5f;
    [SerializeField] private float stopDistance = 4.5f;

    [Header("Автоматический диалог")]
    [SerializeField] private float eventDelay = 120f;
    [SerializeField] private float cameraTurnSpeed = 360f;

    [Header("Интерфейс")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    private bool activated;
    private bool dialogueActive;
    private bool automaticDialogue;
    private bool controlsLocked;

    private float timer;
    private int currentLine;

    private readonly string[] firstDialogue =
    {
        "Привет... Ты можешь купить мне яблоки?",
        "Игорь: Я пока не знаю. Мне ещё нужно купить продукты.",
        "Пожалуйста! Мне правда очень нужны яблоки. Я давно их хочу...",
        "Игорь: Ладно, я подумаю.",
        "Игорь: Сначала совершу покупки, а если останутся деньги, то куплю тебе яблоки.",
        "Правда? Хорошо... Я буду ждать."
    };

    private readonly string[] repeatDialogue =
    {
        "Игорь! Ты уже решил насчёт яблок?",
        "Игорь: Я же сказал, что подумаю. Мне нужно закончить покупки.",
        "Но мне правда нужны яблоки... Пожалуйста, не забудь про меня!",
        "Игорь: Хорошо, хорошо. Я постараюсь."
    };

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;
            agent.isStopped = true;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (player == null)
            return;

        // Приоритет всегда у уже открытого диалога Леры.
        if (dialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                BlockInteractionsThroughFrame = Time.frameCount;
                ShowNextLine();
            }

            return;
        }

        // Не начинаем разговор, пока говорит кассир.
        if (Cassier_Dialogue.IsDialogueActive)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        // Первый разговор запускается только по F рядом с Лерой.
        if (!activated)
        {
            if (distance <= interactionDistance &&
                Input.GetKeyDown(KeyCode.F))
            {
                StartDialogue(false);
            }

            return;
        }

        // После первого разговора Лера следует за игроком.
        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;

            if (distance > chaseDistance)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                agent.isStopped = true;
            }
        }

        // Во время диалога кассира таймер не увеличивается.
        timer += Time.deltaTime;

        if (timer >= eventDelay)
        {
            timer = 0f;
            StartDialogue(true);
        }
    }

    private void StartDialogue(bool automatic)
    {
        if (dialogueActive || Cassier_Dialogue.IsDialogueActive)
            return;

        dialogueActive = true;
        IsDialogueActive = true;
        automaticDialogue = automatic;
        currentLine = 0;
        timer = 0f;

        if (agent != null)
            agent.isStopped = true;

        // Блокируем и движение, и обзор на всё время разговора.
        SetPlayerControls(false);
        controlsLocked = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        DisplayCurrentLine();

        // При автоматическом напоминании камера плавно
        // поворачивается к Лере без управления мышью.
        if (automaticDialogue && playerCamera != null)
            StartCoroutine(TurnCameraToLera());
    }

    private void DisplayCurrentLine()
    {
        if (dialogueText == null)
            return;

        string[] lines = activated ? repeatDialogue : firstDialogue;

        if (currentLine >= 0 && currentLine < lines.Length)
            dialogueText.text = lines[currentLine];
    }

    private void ShowNextLine()
    {
        if (!dialogueActive)
            return;

        string[] lines = activated ? repeatDialogue : firstDialogue;

        currentLine++;

        if (currentLine < lines.Length)
            DisplayCurrentLine();
        else
            EndDialogue();
    }

    private IEnumerator TurnCameraToLera()
    {
        while (playerCamera != null && dialogueActive)
        {
            Vector3 direction =
                transform.position - playerCamera.position;

            if (direction.sqrMagnitude < 0.001f)
                break;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            playerCamera.rotation = Quaternion.RotateTowards(
                playerCamera.rotation,
                targetRotation,
                cameraTurnSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(
                playerCamera.rotation,
                targetRotation) < 0.5f)
            {
                playerCamera.rotation = targetRotation;
                break;
            }

            yield return null;
        }
    }

    private void EndDialogue()
    {
        // Блокируем кассира на кадр завершения,
        // чтобы то же нажатие F не запустило его диалог.
        BlockInteractionsThroughFrame = Time.frameCount;

        dialogueActive = false;
        IsDialogueActive = false;

        if (!activated)
            activated = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        StopAllCoroutines();

        if (controlsLocked)
        {
            SetPlayerControls(true);
            controlsLocked = false;
        }

        if (agent != null)
            agent.isStopped = false;

        automaticDialogue = false;
        timer = 0f;
    }

    private void SetPlayerControls(bool state)
    {
        if (playerControls == null)
            return;

        foreach (Behaviour control in playerControls)
        {
            if (control != null)
                control.enabled = state;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (dialogueActive)
        {
            dialogueActive = false;
            IsDialogueActive = false;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (controlsLocked)
            {
                SetPlayerControls(true);
                controlsLocked = false;
            }

            if (agent != null)
                agent.isStopped = false;
        }
    }
}
