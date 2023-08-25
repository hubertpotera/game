using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    [DefaultExecutionOrder(-5)]
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("another singleton matey");
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        public bool BossKilled = false;
        
        public bool UsingDashInvulnerability = false;   // I-frames on dash
        public bool UsingRiposte = false;               // Fast attack after parry
        public bool UsingBloodRage = false;             // More damage, speed at low hp
        public bool UsingRampage = false;               // each next attack is faster
        public bool PerksChosen = false;

        public int Head1Level = -1;
        public int Head2Level = -1;
        public int Body1Level = 0;
        public int Body2Level = -1;

        public int TotalKilled = 0;
        public int KilledThisLevel = 0;
        
        public List<string> Killed = new List<string>();

        public void NextLevel(GameObject player, string levelName, float lightTemperature)
        {
            StartCoroutine(ChangeLevel(player, levelName, lightTemperature));
        }

        private IEnumerator ChangeLevel(GameObject player, string levelName, float lightTemperature)
        {
            KilledThisLevel = 0;
            TheLight.Instance.Source.color = Color.black;

            Scene currentScene = SceneManager.GetActiveScene();

            Destroy(SoundManager.Instance.gameObject);
            Destroy(LevelGenerator.Instance.gameObject);
            Destroy(Camera.main.gameObject);
            player.GetComponent<Inventory>().DestroyHolders();
            player.SetActive(false);
            player.GetComponent<FellaVisuals>().DestroyArmourDisplays();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(2);
            
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(levelName));
            player = Instantiate(player, Vector3.zero, Quaternion.identity);
            player.SetActive(true);
            LevelGenerator.Instance.Player = player.transform;
            asyncOperation = SceneManager.UnloadSceneAsync(currentScene);
            
            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            TheLight.Instance.Source.color = Color.white;
            TheLight.Instance.Source.colorTemperature = lightTemperature;
        }

        public void PlayerDeath()
        {
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            TheLight.Instance.Source.color = Color.black;
            for (int i = 0; i < CombatFella.AllTheFellas.Count; i++)
            {
                Destroy(CombatFella.AllTheFellas[i].gameObject);
            }
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
            Destroy(gameObject);
        }

        public void AddKilled(CombatFella lad)
        {
            Texture2D token = (Texture2D)lad.Visuals.LadRenderer.material.mainTexture;
            Inventory inventory = lad.Inventory;

            byte[] bytes = ImageConversion.EncodeToPNG(CombineTextures(token, inventory));

            Killed.Add(Misc.BytesToString(bytes));
            TotalKilled += 1;
            KilledThisLevel += 1;
        }

        private Texture2D CombineTextures(Texture2D token, Inventory inventory)
        {
            Texture2D result = new Texture2D(token.width, token.height);

            //TODO check if i have to copy the textures, no idea why im doing it this way
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
