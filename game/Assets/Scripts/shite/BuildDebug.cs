using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BuildDebug : MonoBehaviour
    {
        string myLog = "*begin log\n";
        string filename = "";
        bool doShow = false;
        int kChars = 700;
        void OnEnable() { Application.logMessageReceived += Log; DontDestroyOnLoad(gameObject); }
        void OnDisable() { Application.logMessageReceived -= Log; }
        void Update() { if (Input.GetKeyDown(KeyCode.BackQuote)) { doShow = !doShow; } }

        public void Log(string logString, string stackTrace, LogType type)
        {
            // for onscreen...
            myLog = myLog + " " + logString + "\n";
            if (myLog.Length > kChars) { myLog = myLog.Substring(myLog.Length - kChars); }
            
            try { System.IO.File.AppendAllText(filename, logString + " "); }
            catch { }
        }

        void OnGUI()
        {
            if (!doShow) { return; }
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
            GUI.TextArea(new Rect(10, 10, 540, 370), myLog);
        }
    }
}