using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTimeManager : MonoBehaviour
{
    private float sceneStartTime;
    private string timeFilePath;
    private float previousTime = 0f;
    private string sceneName;
    private static string saveDirectory = "/Scripts/Save/SaveTime/";

    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        timeFilePath = Application.dataPath + saveDirectory + "scene_time_" + sceneName + ".txt";

        // Ensure the directory exists
        string dir = Application.dataPath + saveDirectory;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Load previous time from file
        if (File.Exists(timeFilePath))
        {
            if (float.TryParse(File.ReadAllText(timeFilePath), out previousTime))
            {
                Debug.Log($"Loaded previous time for {sceneName}: {previousTime}");
            }
        }

        sceneStartTime = Time.time;
    }

    void OnDisable()
    {
        SaveSceneTime();
    }

    void OnApplicationQuit()
    {
        SaveSceneTime();
        Debug.Log("Application Quit");
    }

    private void SaveSceneTime()
    {
        float currentSessionTime = Time.time - sceneStartTime;
        float totalTime = previousTime + currentSessionTime;
        // Ensure the directory exists before saving
        string dir = Application.dataPath + saveDirectory;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(timeFilePath, totalTime.ToString());
        Debug.Log($"Saved total time for {sceneName}: {totalTime}");
    }

    public static float GetSceneTime(string sceneName)
    {
        string path = Application.dataPath + "/Scripts/Save/SaveTime/scene_time_" + sceneName + ".txt";
        Debug.Log("Loading scene time from: " + path);
        if (File.Exists(path))
        {
            return float.Parse(File.ReadAllText(path));
        }
        return 0f;
    }
}

