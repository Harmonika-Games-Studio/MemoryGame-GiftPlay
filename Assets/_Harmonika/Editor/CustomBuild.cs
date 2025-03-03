using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Http;
using System;
using Harmonika.Tools;
using System.Collections.Generic;

public static class CustomBuild
{
    public static void BuildWithCustomAssets()
    {
        string authenticationUser = GetCommandLineArgument("-authenticationUser");
        string authenticationPassword = GetCommandLineArgument("-authenticationPassword");
        string id = GetCommandLineArgument("-id");

        // Ensure the Resources directory exists
        if (!Directory.Exists(HarmonikaConstants.RESOURCES_PATH))
        {
            Directory.CreateDirectory(HarmonikaConstants.RESOURCES_PATH);
        }

        // Ensure the Builds/Android directory exists
        if (!Directory.Exists(HarmonikaConstants.ANDROID_BUILD_PATH))
        {
            Directory.CreateDirectory(HarmonikaConstants.ANDROID_BUILD_PATH);
        }

        string configJson = "";

        Debug.Log("CustomBuild.cs -> === STARTING CUSTOM BUILD ===");

        // Downloading JSON
        try
        {
            using (var client = new HttpClient())
            {
                string tokenBase64 = Base64Encode($"{authenticationUser}:{authenticationPassword}");
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {tokenBase64}");

                configJson = client.GetStringAsync($"https://giftplay.com.br/builds/getGameConfig/{id}").Result;
                Debug.Log("CustomBuild.cs -> Received json: " + configJson);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("CustomBuild.cs -> Error downloading json: " + e.Message);
        }

        // Ensure JSON is not empty
        if (string.IsNullOrEmpty(configJson))
        {
            Debug.LogError("CustomBuild.cs -> JSON is empty or missing!");
            return;
        }

        // Save JSON to file
        File.WriteAllText(Path.Combine(HarmonikaConstants.RESOURCES_PATH, "debugGameConfig.json"), configJson);
        File.WriteAllText(Path.Combine(HarmonikaConstants.RESOURCES_PATH, "gameConfig.json"), JsonUtility.ToJson(DownloadMemoryGameAssets(configJson)));
        Debug.Log("CustomBuild.cs -> Game config saved at: " + Path.Combine(HarmonikaConstants.RESOURCES_PATH, "gameConfig.json"));

        ConvertPNGsToSingleBeforeBuild();

        // Execute the build
        BuildPipeline.BuildPlayer(
            new[] { "Assets/_Harmonika/MemoryGameTemplate/MemoryGame.unity" },
            HarmonikaConstants.ANDROID_BUILD_PATH + "MemoryGame.apk",
            BuildTarget.Android,
            BuildOptions.None
        );

        Debug.Log("CustomBuild.cs -> Build completed successfully!");
    }

    private static object DownloadMemoryGameAssets(string configJson)
    {
        MemoryGameConfig gameConfig = new();

        gameConfig = JsonUtility.FromJson<MemoryGameConfig>(configJson);
        Debug.Log("CustomBuild.cs -> JSON parsed successfully!");
        
        List<string> cardsList = new();
        
        for (int i = 0; i < gameConfig.cardsList.Count; i++)
        {
            cardsList.Add(Path.GetFileNameWithoutExtension(DownloadImage(gameConfig.cardsList[i], $"card-{i}.png")));
        }
        
        gameConfig.cardsList = cardsList;
        gameConfig.cardBack = Path.GetFileNameWithoutExtension(DownloadImage(gameConfig.cardBack, "cardBack.png"));
        gameConfig.userLogo = Path.GetFileNameWithoutExtension(DownloadImage(gameConfig.userLogo, "userLogo.png"));
        return gameConfig;
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

    //public static string DownloadImage(string url, string name)
    //{
    //    if (string.IsNullOrEmpty(url)) return null;
    //    try
    //    {
    //        using (WebClient client = new WebClient())
    //        {
    //            string imagePath = Path.Combine(HarmonikaConstants.RESOURCES_PATH, name);
    //            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) UnityWebClient/1.0");

    //            byte[] imageData = client.DownloadData(url);
    //            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

    //            if (!texture.LoadImage(imageData)) return null;

    //            File.WriteAllBytes(imagePath, texture.EncodeToPNG());
    //            Debug.Log($"CustomBuild.cs -> Image saved at: {imagePath}");

    //            SetTextureToSingle(imagePath);

    //            return imagePath;
    //        }
    //    }
    //    catch (System.Exception e)
    //    {
    //        Debug.LogError("CustomBuild.cs -> Error downloading image: " + e.Message);
    //        return null;
    //    }
    //}

    public static string DownloadImage(string url, string name)
    {
        if (string.IsNullOrEmpty(url)) return null;
        try
        {
            using (WebClient client = new WebClient())
            {
                string directoryPath = Path.Combine(Application.dataPath, "Resources");
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                string imagePath = Path.Combine(directoryPath, name);
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) UnityWebClient/1.0");

                byte[] imageData = client.DownloadData(url);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                if (!texture.LoadImage(imageData)) return null;

                File.WriteAllBytes(imagePath, texture.EncodeToPNG());
                Debug.Log($"[CustomBuild] Image saved at: {imagePath}");

                // Caminho relativo pra Unity reconhecer
                string relativePath = "Assets/Resources/" + name;

                // Força a importação e converte para Single
                AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);

                return relativePath;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[CustomBuild] Error downloading image: " + e.Message);
            return null;
        }
    }

    public static void ConvertPNGsToSingleBeforeBuild()
    {
        string resourcesPath = "Assets/Resources";
        string[] files = Directory.GetFiles(resourcesPath, "*.png", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string assetPath = file.Replace("\\", "/");
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter != null && textureImporter.spriteImportMode != SpriteImportMode.Single)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.SaveAndReimport();

                Debug.Log($"[Build Process] Converted to Single: {assetPath}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("[Build Process] Conversion complete before build!");
    }

    public static Texture2D LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("CustomBuild.cs -> File not found: " + filePath);
            return null;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }

        Debug.LogError("CustomBuild.cs -> Failed to load texture from file: " + filePath);
        return null;
    }

    public static Sprite LoadSpriteFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("CustomBuild.cs -> File not found: " + filePath);
            return null;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (texture.LoadImage(fileData))
        {
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        Debug.LogError("CustomBuild.cs -> Failed to load sprite from file: " + filePath);
        return null;
    }
}