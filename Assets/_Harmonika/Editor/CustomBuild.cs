using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Http;
using System;

public class CustomBuild
{
    public static void BuildWithCustomAssets()
    {
        string authenticationUser = GetCommandLineArgument("-authenticationUser");
        string authenticationPassword = GetCommandLineArgument("-authenticationPassword");
        string id = GetCommandLineArgument("-id");

        string configJson = "";

        Debug.Log("=== STARTING CUSTOM BUILD ===");

        // Downloading JSON
        try
        {
            using (var client = new HttpClient())
            {
                string tokenBase64 = Base64Encode($"{authenticationUser}:{authenticationPassword}");
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {tokenBase64}");

                // Sincronizando requisição JSON
                configJson = client.GetStringAsync($"https://giftplay.com.br/builds/getGameConfig/{id}").Result;
                Debug.Log("Received json: " + configJson);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error processing json: " + e.Message);
        }

        // Ensure JSON is not empty
        if (string.IsNullOrEmpty(configJson))
        {
            Debug.LogError("json argument is empty or missing!");
            return;
        }

        // Ensure the Resources directory exists
        string resourcesPath = "Assets/Resources/";
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }

        // Ensure the Builds/Android directory exists
        string androidBuildPath = "Builds/Android/";
        if (!Directory.Exists(androidBuildPath))
        {
            Directory.CreateDirectory(androidBuildPath);
        }

        // Save JSON to file
        File.WriteAllText(Path.Combine(resourcesPath, "gameConfig.json"), configJson);
        Debug.Log("Game config saved at: " + Path.Combine(resourcesPath, "gameConfig.json"));

        // Save JSON to Build directory for debugging
        string buildJsonFilePath = Path.Combine(androidBuildPath, "gameConfig_debug.json");
        File.WriteAllText(buildJsonFilePath, configJson);
        Debug.Log("Game config also saved at: " + buildJsonFilePath);

        // Parse JSON and attempt to download the image
        try
        {
            GameConfig config = JsonUtility.FromJson<GameConfig>(configJson);
            Debug.Log("JSON parsed successfully!");

            if (!string.IsNullOrEmpty(config.customImageUrl))
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) UnityWebClient/1.0");

                    byte[] imageData = client.DownloadData(config.customImageUrl);
                    string imagePath = Path.Combine(resourcesPath, "customImage.bytes");
                    File.WriteAllBytes(imagePath, imageData);
                    Debug.Log("Custom image downloaded successfully at: " + imagePath);
                }

            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error downloading image: " + e.Message);
        }

        AssetDatabase.Refresh();

        // Execute the build
        BuildPipeline.BuildPlayer(
            new[] { "Assets/_Harmonika/PublicDemo/PublicDemo.unity" },
            androidBuildPath + "DemoBuild.apk",
            BuildTarget.Android,
            BuildOptions.None
        );

        Debug.Log("Build completed successfully!");
    }

    private static string GetCommandLineArgument(string name)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith(name + "="))
            {
                string value = args[i].Substring(args[i].IndexOf('=') + 1);
                return value.Trim('"').Trim();
            }
            else if (args[i] == name && i + 1 < args.Length)
            {
                return args[i + 1].Trim();
            }
        }
        return null;
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    [System.Serializable]
    private class GameConfig
    {
        public string colorSphere;
        public string colorPrism;
        public string colorSquare;
        public string customImageUrl;
    }
}