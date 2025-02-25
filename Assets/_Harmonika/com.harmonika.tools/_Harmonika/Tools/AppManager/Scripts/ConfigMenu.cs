using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfigMenu : MonoBehaviour
{
    [SerializeField] private Button _backButton;

    [Header("Volume Config")]
    [SerializeField] private TMP_InputField _inputVolumeMusic;
    [SerializeField] private TMP_InputField _inputVolumeSFX;

    protected virtual void Awake()
    {
        _backButton.onClick.AddListener(() => AppManager.Instance.OpenMenu("MainMenu"));
        _inputVolumeMusic.onSubmit.AddListener(OnGameMusicValueChanged);
        _inputVolumeSFX.onSubmit.AddListener(OnGameSoundValueChanged);
    }

    protected virtual void Start()
    {
        UpdateInputValues();
    }

    void SaveAudioVolumeByType(Harmonika.Tools.AudioType type, float newVolume)
    {
        PlayerPrefs.SetFloat("Audio: " + type, newVolume);
        SoundSystem.Instance.ChangeAudioVolumeByType(type, newVolume);
    }

    void OnGameMusicValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value) || int.Parse(value) <= 0)
        {
            value = "0";
        }
        else if (int.Parse(value) > 100)
        {
            value = "100";
        }
         _inputVolumeMusic.text = value;
        SaveAudioVolumeByType(Harmonika.Tools.AudioType.Music, int.Parse(value));
    }

    void OnGameSoundValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value) || int.Parse(value) <= 0)
        {
            value = "0";
        }
        else if (int.Parse(value) > 100)
        {
            value = "100";
        }
        _inputVolumeSFX.text = value;
        SaveAudioVolumeByType(Harmonika.Tools.AudioType.SFX, int.Parse(value));
    }

    void UpdateInputValues()
    {
        float musicVolume = PlayerPrefs.GetFloat("Audio: " + Harmonika.Tools.AudioType.Music, AppManager.Instance.gameConfig.musicVolume);
        float sfxVolume = PlayerPrefs.GetFloat("Audio: " + Harmonika.Tools.AudioType.SFX, AppManager.Instance.gameConfig.sfxVolume);

        _inputVolumeMusic.text = musicVolume.ToString();
        _inputVolumeSFX.text = sfxVolume.ToString();

        SaveAudioVolumeByType(Harmonika.Tools.AudioType.Music, musicVolume);
        SaveAudioVolumeByType(Harmonika.Tools.AudioType.SFX, sfxVolume);
    }
}