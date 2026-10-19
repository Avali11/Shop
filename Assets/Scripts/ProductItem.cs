using UnityEngine;
using UnityEngine.UI;

public class ProductItem : MonoBehaviour
{
    [Header("Настройки продукта")]
    [Tooltip("Пиши строго одно из пяти: Молоко, Яйца, Курица, Картошка фри, Яблоки")]
    public string productName;

    [Tooltip("Цена за одну штуку этого товара (спишется из BudgetManager)")]
    public int price = 100;

    [Tooltip("Дистанция, на которой игрок может взять товар")]
    public float interactionDistance = 2.5f;

    private Transform playerTransform;
    private Outline outlineComponent;
    private bool isPlayerNear = false;

    void Start()
    {
        // Автоматически ищем игрока на сцене по его тегу "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Проверяем, есть ли на объекте компонент подсветки QuickOutline
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false; // Выключаем подсветку на старте
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Считаем расстояние между этим товаром и игроком
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionDistance)
        {
            if (!isPlayerNear)
            {
                isPlayerNear = true;
                ToggleHighlight(true); // Включаем подсветку, когда подошли
            }

            // Проверяем нажатие клавиши взаимодействия
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUp();
            }
        }
        else
        {
            if (isPlayerNear)
            {
                isPlayerNear = false;
                ToggleHighlight(false); // Выключаем подсветку, если отошли
            }
        }
    }

    void ToggleHighlight(bool state)
    {
        if (outlineComponent != null)
        {
            outlineComponent.enabled = state;
        }
    }

    void PickUp()
    {
        // 1. Списываем деньги за этот конкретный предмет из твоего BudgetManager
        if (BudgetManager.Instance != null)
        {
            BudgetManager.Instance.SpendMoney(price);
        }
        else
        {
            Debug.LogWarning("BudgetManager не найден на сцене! Деньги за " + productName + " не списались.");
        }

        // 2. Отправляем сигнал в блокнот, чтобы он вычеркнул этот продукт
        if (ShoppingListManager.Instance != null)
        {
            ShoppingListManager.Instance.BuyProduct(productName);
        }

        // 3. Удаляем эту конкретную модельку с полки магазина
        Destroy(gameObject);
    }
}