using UnityEngine;
using TMPro; // Подключаем TextMeshPro для работы с текстом

public class BudgetManager : MonoBehaviour
{
    // Статическая ссылка, чтобы любой другой скрипт (например, диалоги) мог легко достучаться до бюджета
    public static BudgetManager Instance;

    [Header("Настройки бюджета")]
    [SerializeField] private int currentBudget = 1000; // Текущий бюджет (по умолчанию 1000)

    [Header("UI Элементы")]
    [SerializeField] private TextMeshProUGUI budgetText; // Сюда перетащим текст с экрана

    private void Awake()
    {
        // Настраиваем Singleton (чтобы к скрипту был удобный доступ из любого места)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // При старте игры сразу обновляем циферку на экране
        UpdateBudgetUI();
    }

    // Метод для добавления денег (например, заработал или дали)
    public void AddMoney(int amount)
    {
        currentBudget += amount;
        UpdateBudgetUI();
    }

    // Метод для вычитания денег (например, потратил)
    public void SpendMoney(int amount)
    {
        currentBudget -= amount;
        if (currentBudget < 0) currentBudget = 0; // Чтобы бюджет не уходил в минус (по желанию)
        UpdateBudgetUI();
    }

    // Метод, который возвращает текущее число бюджета (понадобится для проверки концовок)
    public int GetCurrentBudget()
    {
        return currentBudget;
    }

    // Функция, которая обновляет текст на экране
    private void UpdateBudgetUI()
    {
        if (budgetText != null)
        {
            // Выведет на экран: Бюджет: 1000 руб.
            budgetText.text = "Бюджет: " + currentBudget.ToString("N0") + " руб.";
        }
    }
}