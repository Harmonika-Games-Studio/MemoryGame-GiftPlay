using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Http;
using System;
using UnityEngine.Events;
using Harmonika.Tools;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static UnityEngine.Rendering.STP;

public static class CustomBuild
{
    public static string TestJson
    {
        get
        {
            List<StorageItemConfig> storageItems = new List<StorageItemConfig>
            {
                new() { _itemName = "Item1", _initialValue = 10, _prizeScore = 100 },
                new() { _itemName = "Item2", _initialValue = 5, _prizeScore = 50 }
            };

            List<LeadDataConfig> leadDataConfig = new List<LeadDataConfig>
            {
                new() { fieldName = "nome", id = LeadID.nome, isOptional = false, inputType = LeadInputType.InputField, inputDataConfig = new(KeyboardType.AlphaUpper, ParseableFields.none, "Sr. Harmonika")},
                new() { fieldName = "idade", id = LeadID.idade, isOptional = false, inputType = LeadInputType.InputField, inputDataConfig = new("Numeric", "none", "Apenas Números")},
                new() { fieldName = "telefone", id = LeadID.telefone, isOptional = false, inputType = LeadInputType.InputField, inputDataConfig = new(KeyboardType.Numeric, ParseableFields.phone, "(00) 00000-0000")},
                new() { fieldName = "cpf", id = LeadID.id, isOptional = false, inputType = LeadInputType.InputField, inputDataConfig = new("Numeric", "cpf", "000.000.000-00")},
                new() { fieldName = "email", id = LeadID.email, isOptional = false, inputType = LeadInputType.InputField, inputDataConfig = new(KeyboardType.AlphaLowerEmail, ParseableFields.none, "exemplo@harmonika.com")}
            };


            JObject rawData = new JObject
            {
                { "cardBack", "https://i.imgur.com/LDsqclp.png" },
                { "cardsList", new JArray
                    {
                        "https://draftsim.com/wp-content/uploads/2022/07/dmu-281-forest.png",
                        "https://draftsim.com/wp-content/uploads/2022/07/dmu-278-island.png",
                        "https://draftsim.com/wp-content/uploads/2022/07/dmu-280-mountain.png",
                        "https://draftsim.com/wp-content/uploads/2022/07/dmu-277-plains.png",
                        "https://draftsim.com/wp-content/uploads/2022/07/dmu-279-swamp.png",
                        "https://mtginsider.com/wp-content/uploads/2024/08/senseisdiviningtop.png"
                    }
                },
                { "userLogo", "https://logos-world.net/wp-content/uploads/2023/05/Magic-The-Gathering-Logo.png"},
                { "storageItems", JArray.FromObject(storageItems) },
                { "leadDataConfig", JArray.FromObject(leadDataConfig) },
                { "gameName", "<span style=\\\"color: #e03e2d;\\\"><em><strong>Teste<\\/strong><\\/em><\\/span>"},
                { "primaryColor", "#1BB329"},
                { "secondaryColor", "#8c9c16"},
                { "tertiaryColor", "#CD1315"},
                { "neutralColor", "#000000"}
            };

            return rawData.ToString();
        }
    }

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
        try
        {
            gameConfig = JsonUtility.FromJson<MemoryGameConfig>(configJson);
            Debug.Log("CustomBuild.cs -> JSON parsed successfully!");

            List<string> cardsArray = new();

            for (int i = 0; i < gameConfig.cardsList.Count; i++)
            {
                cardsArray.Add(Path.GetFileNameWithoutExtension(DownloadImage(gameConfig.cardsList[i], $"card-{i}.png")));
            }

            gameConfig.cardsList = cardsArray;
            gameConfig.cardBack = Path.GetFileNameWithoutExtension(DownloadImage(gameConfig.cardBack, "cardBack.png"));
            return gameConfig;
        }
        catch (System.Exception e)
        {
            Debug.LogError("CustomBuild.cs -> Error downloading image: " + e.Message);
            return null;
        }
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

    public static string DownloadImage(string url, string name)
    {
        if (string.IsNullOrEmpty(url)) return null;

        using (WebClient client = new WebClient())
        {
            string imagePath = Path.Combine(HarmonikaConstants.RESOURCES_PATH, name);
            client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) UnityWebClient/1.0");

            byte[] imageData = client.DownloadData(url);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!texture.LoadImage(imageData)) return null;

            File.WriteAllBytes(imagePath, texture.EncodeToPNG());
            Debug.Log($"CustomBuild.cs -> Image saved at: {imagePath}");
            return imagePath;
        }
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