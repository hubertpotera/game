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
        public int Head1Level = -1;
        public int Head2Level = -1;
        public int Body1Level = 0;
        public int Body2Level = -1;
        
        public List<string> Killed = new List<string>();    //TODO this doesnt get saved in the json file (make a seperate struct for this? change byte array to string or other shite?)

        public bool UnlockedDashInvulnerability = false;    // As it says
        public bool UnlockedRiposte = false;                // Fast attack after parry
        public bool UnlockedBloodRage = false;              // More damage, speed at low hp
        public bool UnlockedRampage = false;                // each next attack is faster
        
        public bool UsingDashInvulnerability = false;
        public bool UsingRiposte = false;
        public bool UsingBloodRage = false;
        public bool UsingRampage = false;

        public void AddKilled(CombatFella lad)
        {
            Debug.Log("noted");
            Texture2D token = (Texture2D)lad.Visuals.LadRenderer.material.mainTexture;
            Debug.Log(token.name);
            Inventory inventory = lad.Inventory;

            byte[] bytes = ImageConversion.EncodeToPNG(CombineTextures(token, inventory));

            Killed.Add(Misc.BytesToString(bytes));
        }

        private Texture2D CombineTextures(Texture2D token, Inventory inventory)
        {
            Texture2D result = new Texture2D(token.width, token.height);

            Texture2D ladTexture = new Texture2D(token.width, token.height);
            Graphics.CopyTexture(token, ladTexture);
            Texture2D body1Texture = new Texture2D(token.width, token.height);
            if(inventory.Body1 != null)
                Graphics.CopyTexture(inventory.Body1.Texture, body1Texture);
            Texture2D body2Texture = new Texture2D(token.width, token.height);
            if(inventory.Body2 != null)
                Graphics.CopyTexture(inventory.Body2.Texture, body2Texture);
            Texture2D head1Texture = new Texture2D(token.width, token.height);
            if(inventory.Head1 != null)
                Graphics.CopyTexture(inventory.Head1.Texture, head1Texture);
            Texture2D head2Texture = new Texture2D(token.width, token.height);
            if(inventory.Head2 != null)
                Graphics.CopyTexture(inventory.Head2.Texture, head2Texture);
            ladTexture.Apply();
            body1Texture.Apply();
            body2Texture.Apply();
            head1Texture.Apply();
            head2Texture.Apply();

            for (int y = 0; y < token.height; y++)
            {
                for (int x = 0; x < token.width; x++)
                {
                    Color pixel = ladTexture.GetPixel(x,y);
                    if(inventory.Body1 != null && body1Texture.GetPixel(x,y).a > 0)
                        pixel = body1Texture.GetPixel(x,y);
                    if(inventory.Body2 != null && body2Texture.GetPixel(x,y).a > 0)
                        pixel = body2Texture.GetPixel(x,y);
                    if(inventory.Head1 != null && head1Texture.GetPixel(x,y).a > 0)
                        pixel = head1Texture.GetPixel(x,y);
                    if(inventory.Head2 != null && head2Texture.GetPixel(x,y).a > 0)
                        pixel = head2Texture.GetPixel(x,y);
                    result.SetPixel(x,y, pixel);
                }
            }

            result.Apply();
            return result;
        }
    }
}
