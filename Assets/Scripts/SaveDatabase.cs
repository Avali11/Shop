using System;
using System.IO;
using UnityEngine;

public static class SaveDatabase
{
    private static string dbPath = Application.persistentDataPath + "/GameSave_SQL.txt";

    public static void InitializeDatabase()
    {
        if (!File.Exists(dbPath))
        {
            string defaultTable = "CREATE TABLE GameSave (id INTEGER PRIMARY KEY, scene_name TEXT, pos_x REAL, pos_y REAL, pos_z REAL);\n";
            File.WriteAllText(dbPath, defaultTable);
        }
    }

    public static bool HasSaveData()
    {
        InitializeDatabase();
        string content = File.ReadAllText(dbPath);
        return content.Contains("INSERT OR REPLACE");
    }

    public static void SaveGame(string sceneName, Vector3 position)
    {
        InitializeDatabase();

        string sqlQuery = $"\nINSERT OR REPLACE INTO GameSave (id, scene_name, pos_x, pos_y, pos_z) " +
                          $"VALUES (1, '{sceneName}', {position.x:F2}, {position.y:F2}, {position.z:F2});";

        string baseContent = "CREATE TABLE GameSave (id INTEGER PRIMARY KEY, scene_name TEXT, pos_x REAL, pos_y REAL, pos_z REAL);\n";

        File.WriteAllText(dbPath, baseContent + sqlQuery);

        // Дублируем в PlayerPrefs для быстрого доступа
        PlayerPrefs.SetString("SQL_Scene", sceneName);
        PlayerPrefs.SetFloat("SQL_X", position.x);
        PlayerPrefs.SetFloat("SQL_Y", position.y);
        PlayerPrefs.SetFloat("SQL_Z", position.z);
        PlayerPrefs.Save();

        Debug.Log("Game data saved.");
    }

    public static string GetSavedScene()
    {
        // Если в PlayerPrefs пусто, но файл существует — попробуем вернуть дефолт
        return PlayerPrefs.GetString("SQL_Scene", "HomeScene");
    }

    public static Vector3 LoadPositionStatic()
    {
        float x = PlayerPrefs.GetFloat("SQL_X", 0f);
        float y = PlayerPrefs.GetFloat("SQL_Y", 0f);
        float z = PlayerPrefs.GetFloat("SQL_Z", 0f);
        return new Vector3(x, y, z);
    }

    public static void ClearSave()
    {
        InitializeDatabase();

        string cleanTable = "CREATE TABLE GameSave (id INTEGER PRIMARY KEY, scene_name TEXT, pos_x REAL, pos_y REAL, pos_z REAL);\n";
        File.WriteAllText(dbPath, cleanTable);

        PlayerPrefs.DeleteKey("SQL_Scene");
        PlayerPrefs.DeleteKey("SQL_X");
        PlayerPrefs.DeleteKey("SQL_Y");
        PlayerPrefs.DeleteKey("SQL_Z");
        PlayerPrefs.Save();

        Debug.Log("Save data cleared.");
    }
}