using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave => File.Exists(SavePath);

    public static void Write(SaveData data)
    {
        // Write to a temp file first, then swap it in, so a crash mid-write can't
        // leave a half-written (corrupt) save behind.
        string temp = SavePath + ".tmp";
        File.WriteAllText(temp, JsonUtility.ToJson(data, true));

        if (File.Exists(SavePath)) File.Delete(SavePath);
        File.Move(temp, SavePath);
    }

    public static bool TryRead(out SaveData data)
    {
        data = null;
        if (!HasSave) return false;

        try
        {
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            return data != null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: couldn't read the save, starting fresh. ({e.Message})");
            return false;
        }
    }

    public static void Delete()
    {
        if (HasSave) File.Delete(SavePath);
    }
}
