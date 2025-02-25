using System.Collections;
using TMPro;
using UnityEngine;

public class AnimatedText : MonoBehaviour
{
    [Header("Text Settings")]
    public TMP_Text textToAnimate;
    public string[] textList;
    [Range(0.1f, 3f)]
    public float timeToAnimate = 0.5f;
    
    void Awake()
    {
        StartCoroutine(LoadingAnimatedText());
    }

    IEnumerator LoadingAnimatedText()
    {
        while (true)
        {
            for (int i = 0; i < textList.Length; i++)
            {
                textToAnimate.text = textList[i];
                yield return new WaitForSeconds(timeToAnimate);
            }
        }
    }
}
