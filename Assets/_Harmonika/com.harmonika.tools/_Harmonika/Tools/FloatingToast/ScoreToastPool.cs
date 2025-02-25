using UnityEngine;
using System.Collections.Generic;

public class ScoreToastPool : MonoBehaviour
{
    public static ScoreToastPool Instance { get; private set; }

    [SerializeField] private GameObject scoreToastPrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private Transform _parentTransform;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (_parentTransform == null) _parentTransform = transform;

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            AddToastToPool();
        }
    }

    private void AddToastToPool()
    {
        GameObject toast = Instantiate(scoreToastPrefab, _parentTransform);
        toast.SetActive(false);
        pool.Enqueue(toast);
    }

    public void SpawnToast(Vector2 position, string score)
    {
        if (pool.Count == 0)
        {
            AddToastToPool();
        }

        GameObject toast = pool.Dequeue();
        toast.SetActive(true);

        // Posiciona o objeto no local correto e inicializa
        RectTransform toastRect = toast.GetComponent<RectTransform>();
        toastRect.anchoredPosition = position;

        ScoreToast toastScript = toast.GetComponent<ScoreToast>();
        toastScript.Initialize(score);
    }

    public void ReturnToast(GameObject toast)
    {
        toast.SetActive(false);
        pool.Enqueue(toast);
    }
}
