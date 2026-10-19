using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings_Controller : MonoBehaviour
{
    [Header("Player Object")]
    public GameObject playerObject; // Сюда в инспекторе тащим Игоря

    [Header("UI Panels")]
    public GameObject settingsPanel; // Сюда тащим саму панель настроек

    private bool isPaused = false;   // Флаг паузы

    // Скрипты ходьбы Игоря, чтобы заморозить его при паузе
    private MonoBehaviour movementScript;
    private MonoBehaviour inputScript;

    void Start()
    {
        // Находим компоненты управления на Игоре
        if (playerObject != null)
        {
            movementScript = playerObject.GetComponent("FirstPersonMovement") as MonoBehaviour;
            inputScript = playerObject.GetComponent("Input") as MonoBehaviour;
        }

        // При старте игры панель всегда скрыта
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Открываем или закрываем панель строго по кнопке Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                OpenSettings();
        }
    }

    // МЕТОД: ОТКРЫТЬ НАСТРОЙКИ (Вызывается по Escape)
    public void OpenSettings()
    {
        isPaused = true;
        Time.timeScale = 0f; // Ставим игру на паузу

        if (settingsPanel != null) settingsPanel.SetActive(true); // Включаем панель

        SetPlayerControl(false); // Замораживаем Игоря, освобождаем мышку
    }

    // МЕТОД: ВЕРНУТЬСЯ В ИГРУ (Вызывается повторным нажатием Escape)
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Возвращаем время в норму

        if (settingsPanel != null) settingsPanel.SetActive(false); // Прячем панель

        SetPlayerControl(true); // Игорь снова ходит, мышка прячется в центр
    }

    // МЕТОД ДЛЯ КНОПКИ: SAVE GAME (Исправленный под нашу SQL базу данных)
    public void ClickSaveGame()
    {
        // Вызываем сохранение через наш главный вечный контроллер меню
        if (MainMenu_Controller.Instance != null)
        {
            MainMenu_Controller.Instance.SaveCurrentGame();
            Debug.Log("Game Saved Successfully through SQL System!");
        }
        else
        {
            Debug.LogError("Ошибка: На сцене не найден вечный MainMenu_Controller!");
        }
    }

    // МЕТОД ДЛЯ КНОПКИ: EXIT TO MENU
    public void ClickExitToMenu()
    {
        // Сохраняем игру перед выходом, если главный контроллер на месте
        if (MainMenu_Controller.Instance != null)
        {
            MainMenu_Controller.Instance.SaveCurrentGame();
        }

        Time.timeScale = 1f; // Возвращаем время в норму перед сменой сцены
        SceneManager.LoadScene("MainMenuScene"); // Уходим в главное меню
    }

    // Логика блокировки Игоря и мышки
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
