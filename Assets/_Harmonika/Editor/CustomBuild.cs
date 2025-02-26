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

public class CustomBuild
{
    public string TestJson
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

        // Parse JSON and attempt to download the cardBack image
        try
        {
            GameConfig config = JsonUtility.FromJson<GameConfig>(configJson);
            Debug.Log("JSON parsed successfully!");

            if (!string.IsNullOrEmpty(config.cardBack))
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) UnityWebClient/1.0");

                    byte[] imageData = client.DownloadData(config.cardBack);
                    string imagePath = Path.Combine(resourcesPath, "cardBack.bytes");
                    File.WriteAllBytes(imagePath, imageData);
                    config.cardBack = imagePath;
                    Debug.Log("Custom image downloaded successfully at: " + imagePath);
                }

            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error downloading image: " + e.Message);
        }

        // Parse JSON and attempt to download the cardsList images
        try
        {
            GameConfig config = JsonUtility.FromJson<GameConfig>(configJson);
            Debug.Log("JSON parsed successfully!");

            for (int i = 0; i < config.cardsList.Count; i++)
            {
                if (!string.IsNullOrEmpty(config.cardsList[i]))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) UnityWebClient/1.0");

                        byte[] imageData = client.DownloadData(config.cardsList[i]);
                        string imagePath = Path.Combine(resourcesPath, $"cardsList[{i}].bytes");
                        File.WriteAllBytes(imagePath, imageData);
                        config.cardBack = imagePath;
                        Debug.Log("Custom image downloaded successfully at: " + imagePath);
                    }

                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error downloading image: " + e.Message);
        }

        // Save JSON to file
        File.WriteAllText(Path.Combine(resourcesPath, "gameConfig.json"), configJson);
        Debug.Log("Game config saved at: " + Path.Combine(resourcesPath, "gameConfig.json"));

        // Save JSON to Build directory for debugging
        string buildJsonFilePath = Path.Combine(androidBuildPath, "gameConfig_debug.json");
        File.WriteAllText(buildJsonFilePath, configJson);
        Debug.Log("Game config also saved at: " + buildJsonFilePath);

        AssetDatabase.Refresh();

        // Execute the build
        BuildPipeline.BuildPlayer(
            new[] { "Assets/_Harmonika/MemoryGameTemplate/MemoryGame.unity" },
            androidBuildPath + "MemoryGame.apk",
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
        public string cardBack;
        public List<string> cardsList;
        public string userLogo;
        public List<StorageItemConfig> storageItems;
        public List<LeadDataConfig> leadDataConfig;
        public string gameName;
        public string primaryColor;
        public string secondaryColor;
        public string tertiaryColor;
        public string neutralColor;
    }
}