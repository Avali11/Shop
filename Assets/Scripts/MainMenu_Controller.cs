using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_Controller : MonoBehaviour
{
    public static MainMenu_Controller Instance;

    [Header("UI Panels")]
    public GameObject mainMenu;

    [Header("UI Buttons")]
    public Button continueButton;

    private bool loadedFromContinue = false;

    void Awake()
    {
        // Делаем этот скрипт вечным, чтобы он управлял загрузкой на всех сценах
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // При запуске игры управляем видимостью кнопки "Продолжить"
        UpdateButtonVisibility();
    }

    // Проверка базы данных для показа кнопки
    public void UpdateButtonVisibility()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(SaveDatabase.HasSaveData());
        }
    }

    // НАЖАТИЕ КНОПКИ: НАЧАТЬ (Новая игра)
    public void ClickNewGame()
    {
        loadedFromContinue = false;

        // Полностью очищаем базу данных SQL
        SaveDatabase.ClearSave();

        // Загружаем стартовую сцену сюжета
        SceneManager.LoadScene("HomeScene");
    }

    // НАЖАТИЕ КНОПКИ: ПРОДОЛЖИТЬ
    public void ClickContinue()
    {
        if (SaveDatabase.HasSaveData())
        {
            loadedFromContinue = true;
            string savedScene = SaveDatabase.GetSavedScene();
            SceneManager.LoadScene(savedScene);
        }
    }

    // МЕТОД ДЛЯ КНОПКИ "СОХРАНИТЬ" В НАСТРОЙКАХ (Внутри HomeScene и ShopScene)
    public void SaveCurrentGame()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Vector3 playerPos = player.transform.position;

            // Записываем данные в SQL формат
            SaveDatabase.SaveGame(currentScene, playerPos);
            Debug.Log("Игра успешно сохранена в SQL базу!");
        }
    }

    // Автоматическое перемещение игрока и обновление кнопок при загрузке сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Обновляем видимость кнопки "Продолжить", если мы вернулись в главное меню
        if (scene.name == "MainMenuScene")
        {
            UpdateButtonVisibility();
            return;
        }

        // Игрока двигаем ТОЛЬКО если нажали кнопку "Продолжить"
        if (loadedFromContinue && SaveDatabase.HasSaveData() && scene.name == SaveDatabase.GetSavedScene())
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;

                // Перемещаем в координаты из базы данных
                player.transform.position = SaveDatabase.LoadPositionStatic();

                if (controller != null) controller.enabled = true;

                // Сбрасываем флаг, чтобы при обычных переходах сюжет не ломался
                loadedFromContinue = false;
            }
        }
    }

    // НАЖАТИЕ КНОПКИ: ВЫХОД
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
