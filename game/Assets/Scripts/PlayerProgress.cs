using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Game
{
    public static class SaveData
    {
        public static Progression Progress;

        private static string _path = System.IO.Path.Combine(Application.persistentDataPath, "SaveData.txt");

        static SaveData()
        {
            Debug.Log("Saving Data");
            Debug.Log(_path);
            Progress = new Progression();
            Load();
        }

        public static void Load()
        {
            Debug.Log("Loading Data");
            try
            {
                string data = File.ReadAllText(_path);
                Progress = JsonUtility.FromJson<Progression>(data);
            }
            catch
            {
                Save();
            }
        }

        public static void Save()
        {
            string json = JsonUtility.ToJson(Progress);
            File.WriteAllText(_path, json);
        }
    }

    public class Progression
    {
        public int maxBossKilled = 0;
        
    }
}
