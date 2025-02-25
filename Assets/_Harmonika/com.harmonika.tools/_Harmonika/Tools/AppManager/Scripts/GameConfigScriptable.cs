using Harmonika.Tools;
using UnityEngine;


/// <summary>
/// A ScriptableObject class that provides basic configuration settings for a game, 
/// including audio settings and prize item storage.
///
/// This configuration can be customized in the Unity Inspector and saved as an asset 
/// for easy reuse across different scenes or projects. The `GameConfigScriptable` 
/// asset can be created via the Unity menu at **Harmonika > ScriptableObjects > Basic Config**.
///
/// **Usage**:
/// - To use this as a base class, create a new script that inherits from `GameConfigScriptable` instead of 
///   `ScriptableObject`.
/// - Replace `"Basic Config"` with a unique name for your configuration, e.g., `"MyGame Config"`, in the 
///   `CreateAssetMenu` attribute of the derived class.
/// - Customize or extend the class to add specific configuration fields as needed for your game.
///
/// **Steps**:
/// 1. Create a new script that inherits from `GameConfigScriptable`.
/// 2. Customize the `CreateAssetMenu` name to something unique, like **"YourGame Config"**.
/// 3. In the Unity Editor, go to **Harmonika > ScriptableObjects > "YourGame Config"** to create a new 
///    instance of your custom config.
/// 4. Configure the audio settings (`musicVolume` and `sfxVolume`) and manage prize items through `storageItems`.
///
/// **Fields**:
/// - **musicVolume**: Volume level for background music, default is 100.
/// - **sfxVolume**: Volume level for sound effects, default is 100.
/// - **storageItems**: Array of `StorageItemConfig` to hold prize item configurations.
///
/// **Example**:
/// ```
/// public class MyGameConfigScriptable : GameConfigScriptable
/// {
///     // Additional game-specific configuration fields here
/// }
/// ```
/// </summary>
[CreateAssetMenu(fileName = "Basic Config", menuName = "Harmonika/ScriptableObjects/Basic Config", order = 1)]
public class GameConfigScriptable : ScriptableObject
{
    [Space(5)]
    [Header("Audio Settings")]
    public int musicVolume = 100;
    public int sfxVolume = 100;

    [Space(5)]
    [Header("Storage Prize Items")]
    public StorageItemConfig[] storageItems;
    
    [Space(5)]
    [Header("Leads Settings")]
    public bool useLeads;
}