using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AccuracyValue : MonoBehaviour
{
    public Slider slider;
    public float jumpSpeed;
    private float _currentValue;
    
    bool _running = false;

    public void StartMoving()
    {
        if (_running) return;
        _running = true;

        _currentValue = 0;
        StartCoroutine(JumpRoutine());
    }

    public float StopMoving()
    {
        _running = false;
        return _currentValue;
    }

    private IEnumerator JumpRoutine()
    {
        while (_running)
        {
            float t = Mathf.PingPong(Time.time * jumpSpeed, 2f);
            _currentValue = t - 1f;
            slider.value = _currentValue;
            yield return null;
        }
    }

}