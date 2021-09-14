using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrintScreen : MonoBehaviour
{
    private const int superSize = 1;

    [Header("Settings")] public string imageName = "screenshot";

    [Header("Live Data")] public bool capturing;
    public string imageFolder;
    public int imageIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void beforeSceneLoad()
    {
        UnityExtensions.CreateGameObjectAndComponent<PrintScreen>(nameof(PrintScreen), isDontDestroyOnLoad: true);
    }

    private void Awake()
    {
        var allowedPlatform = Application.platform == RuntimePlatform.WindowsEditor
                              || Application.platform == RuntimePlatform.WindowsPlayer;
        if (!allowedPlatform)
        {
            enabled = false;
            return;
        }

        // Keep numbering from largest found index.
        imageIndex = 0;
        imageFolder = Application.persistentDataPath;
        var oldFiles = Directory.GetFiles(imageFolder, $"{imageName.Replace("-", "_")}-???-*.png");
        var today = DateTime.Now.Day;
        foreach (var oldFile in oldFiles)
        {
            if (File.GetCreationTime(oldFile).Day != today)
            {
                File.Delete(oldFile);
            }
            else
            {
                var tokens = Path.GetFileName(oldFile).Split('-');
                if (tokens.Length == 3 && int.TryParse(tokens[1], out var fileIndex))
                {
                    if (fileIndex > imageIndex)
                    {
                        imageIndex = fileIndex;
                    }
                }
            }
        }
    }

    private void OnGUI()
    {
        if (!capturing && Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Print
                || Event.current.keyCode == KeyCode.SysReq // This is actually Print Screen!
                || Event.current.keyCode == KeyCode.F6 // Works for Mac
            )
            {
                capturing = true;
            }
        }
    }

    private void LateUpdate()
    {
        if (capturing)
        {
            capturing = false;
            var sceneName = SceneManager.GetActiveScene().name;
            string filename;
            for (;;)
            {
                imageIndex += 1;
                filename = $"{imageFolder}{Path.AltDirectorySeparatorChar}{imageName}-{imageIndex:000}-{sceneName}.png";
                if (!File.Exists(filename))
                {
                    break;
                }
            }
            ScreenCapture.CaptureScreenshot(filename, superSize);
            var sep2 = Path.DirectorySeparatorChar.ToString();
            var sep1 = Path.AltDirectorySeparatorChar.ToString();
            UnityEngine.Debug.Log($"Capture screenshot: {filename.Replace(sep1, sep2)}");
        }
    }
}