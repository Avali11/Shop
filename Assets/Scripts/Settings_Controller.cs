using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings_Controller : MonoBehaviour
{
    [Header("Игрок")]
    public GameObject playerObject;

    [Header("Компоненты управления игроком")]
    public MonoBehaviour movementScript;
    public MonoBehaviour lookScript;

    [Header("Интерфейс")]
    public GameObject settingsPanel;

    private bool isPaused = false;

    private bool movementWasEnabled;
    private bool lookWasEnabled;

    private void Start()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindWithTag("Player");
        }

        if (movementScript == null || lookScript == null)
        {
            Debug.LogWarning(
                "Settings_Controller: назначьте скрипты движения " +
                "и вращения камеры в Inspector."
            );
        }

        if (movementScript != null)
            movementWasEnabled = movementScript.enabled;

        if (lookScript != null)
            lookWasEnabled = lookScript.enabled;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1f;

        SetCursorState(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                OpenSettings();
        }
    }

    // =====================================================
    // ОТКРЫТИЕ НАСТРОЕК
    // =====================================================

    public void OpenSettings()
    {
        if (isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        SetPlayerControl(false);
        SetCursorState(true);

        Debug.Log("Меню настроек открыто. Игра на паузе.");
    }

    // =====================================================
    // ВОЗВРАЩЕНИЕ В ИГРУ
    // =====================================================

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        SetPlayerControl(true);
        SetCursorState(false);

        Debug.Log("Игра продолжена.");
    }

    // =====================================================
    // СОХРАНЕНИЕ ИГРЫ
    // =====================================================

    public void ClickSaveGame()
    {
        if (MainMenu_Controller.Instance == null)
        {
            Debug.LogError("Не найден MainMenu_Controller. Сохранение невозможно.");
            return;
        }

        bool saved = MainMenu_Controller.Instance.SaveCurrentGame();

        if (saved)
        {
            Debug.Log("Сохранение успешно выполнено.");
        }
        else
        {
            Debug.LogError("Сохранение не выполнено. Проверьте ошибки выше в Console.");
        }
    }

    // =====================================================
    // ВОЗВРАЩЕНИЕ В ГЛАВНОЕ МЕНЮ
    // =====================================================

    public void ClickExitToMenu()
    {
        // Важно: здесь НЕ сохраняем игру.
        // Существующее сохранение остаётся без изменений.

        isPaused = false;
        Time.timeScale = 1f;

        SetCursorState(true);

        Debug.Log("Возвращение в главное меню без автоматического сохранения.");

        SceneManager.LoadScene("MainMenuScene");
    }

    // =====================================================
    // УПРАВЛЕНИЕ КОМПОНЕНТАМИ ИГРОКА
    // =====================================================

    private void SetPlayerControl(bool state)
    {
        if (movementScript != null)
        {
            movementScript.enabled = state
                ? movementWasEnabled
                : false;
        }

        if (lookScript != null)
        {
            lookScript.enabled = state
                ? lookWasEnabled
                : false;
        }
    }

    // =====================================================
    // УПРАВЛЕНИЕ КУРСОРОМ
    // =====================================================

    private void SetCursorState(bool menuMode)
    {
        if (menuMode)
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

    // =====================================================
    // ОЧИСТКА ПРИ ПЕРЕХОДЕ МЕЖДУ СЦЕНАМИ
    // =====================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}