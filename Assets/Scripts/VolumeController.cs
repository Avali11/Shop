using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Перетащи сюда твой Slider")]
    public Slider volumeSlider;

    private AudioSource audioSource;

    void Start()
    {
        // Ищем объект с музыкой по тегу
        GameObject musicObject = GameObject.FindWithTag("GameController");

        if (musicObject != null)
        {
            audioSource = musicObject.GetComponent<AudioSource>();
        }

        if (volumeSlider != null && audioSource != null)
        {
            // Настраиваем границы ползунка под громкость (от 0 до 1)
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            // Загружаем сохранённую громкость. Если её ещё нет (первый запуск) — ставим 1f (максимум)
            float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);

            // Ставим ползунок и музыку на этот уровень
            volumeSlider.value = savedVolume;
            audioSource.volume = savedVolume;

            // Вешаем событие: при сдвиге ползунка запускать метод OnVolumeChanged
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    // Этот метод вызывается автоматически каждый раз, когда игрок двигает ползунок
    void OnVolumeChanged(float value)
    {
        if (audioSource != null)
        {
            audioSource.volume = value; // Меняем громкость музыки прямо сейчас

            // Запоминаем выбор игрока в память устройства
            PlayerPrefs.SetFloat("GameVolume", value);
            PlayerPrefs.Save();
        }
    }

    private void OnDestroy()
    {
        // На всякий случай убираем слушателя при уничтожении объекта, чтобы не было ошибок
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}