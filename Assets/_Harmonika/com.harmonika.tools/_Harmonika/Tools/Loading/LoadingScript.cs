using Harmonika.Tools;
using System.Collections;
using UnityEngine;

public class LoadingScript : MonoBehaviour
{
    private static LoadingScript _instance;

    [SerializeField] private CanvasGroup _loadingScreen;
    bool _loading = false;
    Coroutine coroutine;

    public static LoadingScript Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LoadingScript>();

                if (_instance == null)
                {
                    Debug.LogError("Houve uma tentativa de acesso a uma instância vazia do LoadingScript, mas o objeto não está na cena." +
                        "\nPor favor adicione o prefab de LoadingScript na cena antes de continuar!");
                }
            }
            return _instance;
        }
    }

    public bool Loading
    {
        get => _loading;
        set 
        {
            _loading = value;
            
            Turn turn;
            if (Loading) turn = Turn.on; 
            else turn = Turn.off;

            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(SwitchCanvasGroupAnimated(_loadingScreen, turn));
        }
    }

    void Awake()
    {
        #region Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion
    }

    IEnumerator SwitchCanvasGroupAnimated(CanvasGroup group, Turn turn, float duration = .1f)
    {
        float targetAlpha = (turn == Turn.on) ? .97f : 0;
        float startAlpha = group.alpha;
        float time = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        group.alpha = targetAlpha;
        if (turn == Turn.on)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }

}
