using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreToast : MonoBehaviour
{
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private float floatDistance = 50f; 
    [SerializeField] private float duration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        Initialize("500");
    }

    public void Initialize(string score)
    {
        _tmpText.text = score;
        StartCoroutine(FloatAndFadeCoroutine());
    }

    private IEnumerator FloatAndFadeCoroutine()
    {
        Vector2 startPosition = _rectTransform.anchoredPosition;
        Vector2 targetPosition = startPosition + new Vector2(0, floatDistance);
        float elapsedTime = 0f;

        Color initialColor = _tmpText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
            float fadeFactor = Mathf.Clamp01(1 - (elapsedTime / fadeOutDuration));
            _tmpText.color = new Color(initialColor.r, initialColor.g, initialColor.b, fadeFactor);

            yield return null;
        }

        ScoreToastPool.Instance.ReturnToast(gameObject);
    }

}
