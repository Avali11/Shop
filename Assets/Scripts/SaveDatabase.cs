using System;
using System.IO;
using SQLite;
using UnityEngine;

// ---------------------------------------------------------
// МОДЕЛЬ ДАННЫХ
// ---------------------------------------------------------

[Table("GameSave")]
public class GameSaveRecord
{
    [PrimaryKey]
    public int Id { get; set; }

    [Column("scene_name")]
    public string SceneName { get; set; }

    [Column("pos_x")]
    public double PosX { get; set; }

    [Column("pos_y")]
    public double PosY { get; set; }

    [Column("pos_z")]
    public double PosZ { get; set; }
}

// ---------------------------------------------------------
// ОСНОВНОЙ КЛАСС БАЗЫ ДАННЫХ
// ---------------------------------------------------------

public static class SaveDatabase
{
    private static readonly string dbPath =
        Path.Combine(Application.persistentDataPath, "GameSave.db");

    // -----------------------------------------------------
    // СОЗДАНИЕ И ПОДКЛЮЧЕНИЕ К БАЗЕ
    // -----------------------------------------------------

    private static SQLiteConnection OpenDatabase()
    {
        Directory.CreateDirectory(Application.persistentDataPath);

        SQLiteConnection database = new SQLiteConnection(dbPath);

        database.CreateTable<GameSaveRecord>();

        return database;
    }

    public static void InitializeDatabase()
    {
        try
        {
            using (SQLiteConnection database = OpenDatabase())
            {
                Debug.Log("SQLite database initialized: " + dbPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Database initialization error: " + e);
        }
    }

    // -----------------------------------------------------
    // ПРОВЕРКА НАЛИЧИЯ СОХРАНЕНИЯ
    // -----------------------------------------------------

    public static bool HasSaveData()
    {
        try
        {
            using (SQLiteConnection database = OpenDatabase())
            {
                GameSaveRecord save = database.Find<GameSaveRecord>(1);

                return save != null &&
                       !string.IsNullOrEmpty(save.SceneName);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Save checking error: " + e);
            return false;
        }
    }

    // -----------------------------------------------------
    // СОХРАНЕНИЕ ИГРЫ
    // -----------------------------------------------------

    public static bool SaveGame(string sceneName, Vector3 position)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Невозможно сохранить игру: название сцены пустое.");
            return false;
        }

        try
        {
            using (SQLiteConnection database = OpenDatabase())
            {
                GameSaveRecord save = new GameSaveRecord
                {
                    Id = 1,
                    SceneName = sceneName,
                    PosX = position.x,
                    PosY = position.y,
                    PosZ = position.z
                };

                // Обновляем существующую запись или создаём новую.
                database.InsertOrReplace(save);

                // Проверяем, что запись действительно появилась.
                GameSaveRecord verification = database.Find<GameSaveRecord>(1);

                if (verification == null ||
                    verification.SceneName != sceneName)
                {
                    Debug.LogError("Проверка сохранения не пройдена.");
                    return false;
                }

                Debug.Log(
                    "SQLite: игра сохранена. Сцена: " + sceneName +
                    ", позиция: " + position
                );

                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Game saving error: " + e);
            return false;
        }
    }

    // -----------------------------------------------------
    // ПОЛУЧЕНИЕ НАЗВАНИЯ СОХРАНЁННОЙ СЦЕНЫ
    // -----------------------------------------------------

    public static string GetSavedScene()
    {
        try
        {
            using (SQLiteConnection database = OpenDatabase())
            {
                GameSaveRecord save = database.Find<GameSaveRecord>(1);

                return save != null ? save.SceneName : string.Empty;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Scene loading error: " + e);
            return string.Empty;
        }
    }

    // -----------------------------------------------------
    // ПОЛУЧЕНИЕ СОХРАНЁННЫХ КООРДИНАТ
    // -----------------------------------------------------

    public static Vector3 LoadPositionStatic()
    {
        try
        {
            using (SQLiteConnection database = OpenDatabase())
            {
                GameSaveRecord save = database.Find<GameSaveRecord>(1);

                if (save == null)
                    return Vector3.zero;

                return new Vector3(
                    (float)save.PosX,
                    (float)save.PosY,
                    (float)save.PosZ
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Position loading error: " + e);
            return Vector3.zero;
        }
    }

    // -----------------------------------------------------
    // ЗАГРУЗКА СЦЕНЫ И КООРДИНАТ
    // -----------------------------------------------------

    public static bool TryLoadGame(
        out string sceneName,
        out Vector3 position)
    {
        sceneName = string.Empty;
        position = Vector3.zero;

        try
        {
            using (SQLiteConnection database = OpenDatabase())
            {
                GameSaveRecord save = database.Find<GameSaveRecord>(1);

                if (save == null || string.IsNullOrEmpty(save.SceneName))
                    return false;

                sceneName = save.SceneName;

                position = new Vector3(
                    (float)save.PosX,
                    (float)save.PosY,
                    (float)save.PosZ
                );

                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Game loading error: " + e);
            return false;
        }
    }

    // -----------------------------------------------------
    // УДАЛЕНИЕ СОХРАНЕНИЯ
    // -----------------------------------------------------

    public static bool ClearSave()
    {
        try
        {
            using (SQLiteConnection database = OpenDatabase())
            {
                database.Delete<GameSaveRecord>(1);
            }

            // Удаляем данные старой системы сохранений.
            PlayerPrefs.DeleteKey("SQL_Scene");
            PlayerPrefs.DeleteKey("SQL_X");
            PlayerPrefs.DeleteKey("SQL_Y");
            PlayerPrefs.DeleteKey("SQL_Z");

            PlayerPrefs.Save();

            Debug.Log("Save data cleared successfully!");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Save clearing error: " + e);
            return false;
        }
    }
}