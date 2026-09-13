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
                var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key)) ?? new SaveData();
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
            data ??= new SaveData();
            data.Bestiary ??= new List<string> { "sator", "fantasmagor" };
            if (!data.Bestiary.Contains(id)) data.Bestiary.Add(id);
            return data;
        }

        /// <summary>Wiki: 2 кукича = 1 пакич.</summary>
        public static SaveData AddKukichi(SaveData data, int amount)
        {
            data ??= new SaveData();
            if (amount <= 0) return data;
            data.Kukichi += amount;
            data.Pakichi += amount / 2;
            return data;
        }
    }
}
