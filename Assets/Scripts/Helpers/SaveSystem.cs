using UnityEngine;
using System.Collections.Generic;

public static class SaveSystem
{
    private const string SAVE_KEY_PREFIX = "RatsOnline_";

    public static void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(SAVE_KEY_PREFIX + key, value);
    }

    public static int LoadInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(SAVE_KEY_PREFIX + key, defaultValue);
    }

    public static void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(SAVE_KEY_PREFIX + key, value);
    }

    public static float LoadFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(SAVE_KEY_PREFIX + key, defaultValue);
    }

    public static void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(SAVE_KEY_PREFIX + key, value);
    }

    public static string LoadString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(SAVE_KEY_PREFIX + key, defaultValue);
    }

    public static void SaveBool(string key, bool value)
    {
        PlayerPrefs.SetInt(SAVE_KEY_PREFIX + key, value ? 1 : 0);
    }

    public static bool LoadBool(string key, bool defaultValue = false)
    {
        return PlayerPrefs.GetInt(SAVE_KEY_PREFIX + key, defaultValue ? 1 : 0) == 1;
    }

    public static void SaveObject<T>(string key, T obj)
    {
        string json = JsonUtility.ToJson(obj);
        SaveString(key, json);
    }

    public static T LoadObject<T>(string key, T defaultValue = default)
    {
        string json = LoadString(key, "");
        if (string.IsNullOrEmpty(json))
            return defaultValue;

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch
        {
            Debug.LogWarning($"Failed to deserialize {key}. Returning default value.");
            return defaultValue;
        }
    }

    public static void SaveList<T>(string key, List<T> list)
    {
        ListWrapper<T> wrapper = new ListWrapper<T> { items = list };
        SaveObject(key, wrapper);
    }

    public static List<T> LoadList<T>(string key)
    {
        ListWrapper<T> wrapper = LoadObject(key, new ListWrapper<T>());
        return wrapper.items ?? new List<T>();
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(SAVE_KEY_PREFIX + key);
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(SAVE_KEY_PREFIX + key);
    }

    [System.Serializable]
    private class ListWrapper<T>
    {
        public List<T> items;
    }
}
