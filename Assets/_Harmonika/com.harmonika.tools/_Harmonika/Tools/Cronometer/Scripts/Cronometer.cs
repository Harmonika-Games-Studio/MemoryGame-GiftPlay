using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Cronometer : MonoBehaviour
{
    [SerializeField] TMP_Text _timerText;
    public int totalTimeInSeconds;
    public bool useFormat;
    public UnityEvent onEndTimer;

    private Coroutine _timerRoutine;
    private int _remainingTime;

    public void StartTimer()
    {
        if (_timerRoutine != null)
            StopCoroutine(_timerRoutine);
        _timerRoutine = StartCoroutine(Timer());
    }

    public void EndTimer()
    {
        _timerText.text = "00:00";
        _remainingTime = 0;
        
        if (_timerRoutine != null )
            StopCoroutine( _timerRoutine );
    }

    public void ResetTimer()
    {
        EndTimer();
        _timerRoutine = StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        _remainingTime = totalTimeInSeconds;
        while (_remainingTime > 0)
        {
            int minutes = _remainingTime / 60;
            int seconds = _remainingTime % 60;

            if (useFormat) _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            else _timerText.text = _remainingTime.ToString();

            yield return new WaitForSeconds(1);

            _remainingTime--;
        }
        onEndTimer?.Invoke();
    }
}
