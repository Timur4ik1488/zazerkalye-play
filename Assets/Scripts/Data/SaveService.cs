using System.Collections.Generic;
using UnityEngine;

namespace Zazerkalye.Data
{
    public static class SaveService
    {
        const string Key = "zazerkalye.save.v1";

        public static SaveData Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return new SaveData();
            try
            {
                var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key));
                data.Bestiary ??= new List<string> { "sator", "fantasmagor" };
                return data;
            }
            catch { return new SaveData(); }
        }

        public static void Write(SaveData data)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static SaveData Unlock(SaveData data, string id)
        {
            if (!data.Bestiary.Contains(id)) data.Bestiary.Add(id);
            return data;
        }
    }
}
