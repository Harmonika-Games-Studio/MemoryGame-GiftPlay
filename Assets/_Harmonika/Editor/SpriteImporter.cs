using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteImporter : EditorWindow
{
    [MenuItem("Tools/Convert PNGs to Single Sprite")]
    public static void ConvertPNGsToSingle()
    {
        string resourcesPath = "Assets/Resources";
        string[] files = Directory.GetFiles(resourcesPath, "*.png", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string assetPath = file.Replace("\\", "/"); // Corrige caminho no Windows
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter != null && textureImporter.spriteImportMode != SpriteImportMode.Single)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.SaveAndReimport();

                Debug.Log($"[SpriteImporter] Converted to Single: {assetPath}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("[SpriteImporter] Conversion complete!");
    }
}
