using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_Controller : MonoBehaviour
{
    public static MainMenu_Controller Instance;

    [Header("Кнопки главного меню")]
    public Button startButton;
    public Button continueButton;
    public Button exitButton;

    private const string MainMenuSceneName = "MainMenuScene";
    private const string HomeSceneName = "HomeScene";

    // Запоминаем имена кнопок, чтобы находить их после загрузки меню.
    private string startButtonName = "StartButton";
    private string continueButtonName = "ContinueButton";
    private string exitButtonName = "ExitButton";

    // Показывает, что игра загружается через Continue.
    private bool loadedFromContinue = false;

    private void Awake()
    {
        // Не допускаем появления нескольких главных контроллеров.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Запоминаем имена кнопок из Inspector.
        if (startButton != null)
            startButtonName = startButton.gameObject.name;

        if (continueButton != null)
            continueButtonName = continueButton.gameObject.name;

        if (exitButton != null)
            exitButtonName = exitButton.gameObject.name;

        SceneManager.sceneLoaded += OnSceneLoaded;

        SaveDatabase.InitializeDatabase();

        Debug.Log("Главный контроллер запущен. База данных инициализирована.");
    }

    private void Start()
    {
        // Подключаем кнопки при первом открытии меню.
        if (SceneManager.GetActiveScene().name == MainMenuSceneName)
        {
            ReconnectMenuButtons(SceneManager.GetActiveScene());
            UpdateButtonVisibility();
        }
    }

    // =====================================================
    // ПОДКЛЮЧЕНИЕ КНОПОК ГЛАВНОГО МЕНЮ
    // =====================================================

    private void ReconnectMenuButtons(Scene scene)
    {
        startButton = FindMenuButton(scene, startButton, startButtonName);
        continueButton = FindMenuButton(scene, continueButton, continueButtonName);
        exitButton = FindMenuButton(scene, exitButton, exitButtonName);

        // Подключаем кнопки к действующему контроллеру.
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(ClickNewGame);
            startButton.onClick.AddListener(ClickNewGame);
        }
        else
        {
            Debug.LogError("Не найдена кнопка StartButton.");
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(ClickContinue);
            continueButton.onClick.AddListener(ClickContinue);
        }
        else
        {
            Debug.LogError("Не найдена кнопка ContinueButton.");
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitGame);
            exitButton.onClick.AddListener(ExitGame);
        }
        else
        {
            Debug.LogError("Не найдена кнопка ExitButton.");
        }

        Debug.Log("Кнопки главного меню подключены к действующему контроллеру.");
    }

    private Button FindMenuButton(Scene scene, Button currentButton, string buttonName)
    {
        // Если ссылка уже действительна и кнопка принадлежит нужной сцене,
        // оставляем её.
        if (currentButton != null && currentButton.gameObject.scene == scene)
        {
            return currentButton;
        }

        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button button in allButtons)
        {
            if (button.gameObject.scene != scene)
                continue;

            if (button.gameObject.name == buttonName)
            {
                return button;
            }
        }

        return null;
    }

    // =====================================================
    // ОБНОВЛЕНИЕ КНОПКИ CONTINUE
    // =====================================================

    public void UpdateButtonVisibility()
    {
        if (continueButton == null)
        {
            Debug.LogWarning("Кнопка Continue не найдена.");
            return;
        }

        bool hasSave = SaveDatabase.HasSaveData();

        continueButton.gameObject.SetActive(hasSave);

        Debug.Log("Наличие сохранения: " + hasSave);
    }

    // =====================================================
    // НАЧАЛО НОВОЙ ИГРЫ
    // =====================================================

    public void ClickNewGame()
    {
        loadedFromContinue = false;

        // Удаляем предыдущее сохранение.
        if (!SaveDatabase.ClearSave())
        {
            Debug.LogError("Не удалось очистить сохранение. Новая игра не запущена.");
            return;
        }

        Debug.Log("Старое сохранение очищено. Начинается новая игра.");

        Time.timeScale = 1f;

        SceneManager.LoadScene(HomeSceneName);
    }

    // =====================================================
    // ПРОДОЛЖЕНИЕ ИГРЫ
    // =====================================================

    public void ClickContinue()
    {
        if (!SaveDatabase.TryLoadGame(out string savedScene, out Vector3 savedPosition))
        {
            Debug.LogWarning("Не удалось загрузить сохранение.");
            UpdateButtonVisibility();
            return;
        }

        if (string.IsNullOrEmpty(savedScene) ||
            savedScene == MainMenuSceneName ||
            !Application.CanStreamedLevelBeLoaded(savedScene))
        {
            Debug.LogError("Сохранённая сцена указана неправильно: " + savedScene);
            return;
        }

        loadedFromContinue = true;

        Time.timeScale = 1f;

        Debug.Log("Продолжаем игру. Загружается сцена: " + savedScene);

        SceneManager.LoadScene(savedScene);
    }

    // =====================================================
    // СОХРАНЕНИЕ ИГРЫ
    // =====================================================

    public bool SaveCurrentGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == MainMenuSceneName)
        {
            Debug.LogWarning("Нельзя сохранить игру из главного меню.");
            return false;
        }

        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Не найден игрок! Проверьте тег Player.");
            return false;
        }

        Vector3 playerPosition = player.transform.position;

        bool saved = SaveDatabase.SaveGame(currentScene, playerPosition);

        if (saved)
        {
            Debug.Log(
                "Игра успешно сохранена! Сцена: " + currentScene +
                ", позиция игрока: " + playerPosition
            );
        }
        else
        {
            Debug.LogError("Не удалось сохранить игру в SQLite.");
        }

        return saved;
    }

    // =====================================================
    // ОБРАБОТКА ЗАГРУЗКИ СЦЕН
    // =====================================================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Загружена сцена: " + scene.name);

        if (scene.name == MainMenuSceneName)
        {
            // После возвращения в меню заново подключаем все кнопки.
            ReconnectMenuButtons(scene);
            UpdateButtonVisibility();
            return;
        }

        if (!loadedFromContinue)
            return;

        if (!SaveDatabase.TryLoadGame(out string savedScene, out Vector3 savedPosition))
        {
            Debug.LogError("Не удалось получить сохранённые данные после загрузки сцены.");
            loadedFromContinue = false;
            return;
        }

        if (scene.name != savedScene)
            return;

        loadedFromContinue = false;

        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Не удалось найти игрока после загрузки сохранения.");
            return;
        }

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        player.transform.position = savedPosition;

        if (controller != null)
            controller.enabled = true;

        Debug.Log("Позиция игрока успешно восстановлена: " + savedPosition);
    }

    // =====================================================
    // ВЫХОД ИЗ ИГРЫ
    // =====================================================

    public void ExitGame()
    {
        Debug.Log("Игрок вышел из игры.");
        Application.Quit();
    }

    // =====================================================
    // УДАЛЕНИЕ ПОДПИСКИ ПРИ УНИЧТОЖЕНИИ КОНТРОЛЛЕРА
    // =====================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}