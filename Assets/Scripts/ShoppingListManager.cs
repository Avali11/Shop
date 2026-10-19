using UnityEngine;
using TMPro;

public class ShoppingListManager : MonoBehaviour
{
    public static ShoppingListManager Instance;

    [Header("UI Элемент Текста (TextMeshPro)")]
    public TextMeshProUGUI listText;

    // Переменные статуса для нового списка продуктов
    private bool hasMilk = false;
    private bool hasEggs = false;
    private bool hasChicken = false;
    private bool hasFries = false;
    private bool hasApples = false;

    void Awake()
    {
        // Создаем синглтон, чтобы продукты могли легко достучаться до менеджера
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateListUI();
    }

    // Этот метод вызывается из скрипта ProductItem при нажатии на E
    public void BuyProduct(string productName)
    {
        switch (productName)
        {
            case "Молоко": hasMilk = true; break;
            case "Яйца": hasEggs = true; break;
            case "Курица": hasChicken = true; break;
            case "Картошка фри": hasFries = true; break;
            case "Яблоки": hasApples = true; break;
        }

        // Обновляем отображение блокнота на экране
        UpdateListUI();
    }

    // Метод формирования текста с зачеркиваниями тегом <s>
    void UpdateListUI()
    {
        if (listText == null) return;

        string text = "Список покупок:\n";

        text += hasMilk ? "<s>- Молоко</s>\n" : "- Молоко\n";
        text += hasEggs ? "<s>- Яйца</s>\n" : "- Яйца\n";
        text += hasChicken ? "<s>- Курица</s>\n" : "- Курица\n";
        text += hasFries ? "<s>- Картошка фри</s>\n" : "- Картошка фри\n";
        text += hasApples ? "<s>- Яблоки</s>" : "- Яблоки";

        listText.text = text;
    }
}