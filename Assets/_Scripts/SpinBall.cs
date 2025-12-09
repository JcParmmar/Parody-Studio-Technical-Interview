using UnityEngine;

public class SpinBall : MonoBehaviour
{
    [Header("Required Positions")]
    public Transform startPosition;
    public Transform bouncePosition;
    public Transform targetPosition;
    
    [Header("Spin Settings")]
    [Range(-1f, 1f)]
    public float spinValue;
    
    [Header("Physics Settings")]
    public float ballSpeed = 25f;
    [Range(0, 1)] public float spinIntensity = 3f;
    
    private Rigidbody _rb;
    private float _journeyLength;
    private float _startTime;
    private bool _isBowling;
    private bool _hasBounced;
    private Vector3 _actualBouncePos;
    private Vector3 _actualTargetPos;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_isBowling)
        {
            UpdateBallTrajectory();
        }
    }
    
    public void ThrowBall()
    {
        transform.position = startPosition.position;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.useGravity = false;
        
        _hasBounced = false;
        
        _actualBouncePos = bouncePosition.position;
        
        _actualTargetPos = targetPosition.position;
        
        _journeyLength = Vector3.Distance(startPosition.position, _actualTargetPos);
        _startTime = Time.time;
        
        _isBowling = true;
    }
    
    void UpdateBallTrajectory()
    {
        float distanceCovered = (Time.time - _startTime) * ballSpeed;
        float fractionOfJourney = distanceCovered / _journeyLength;
        
        if (fractionOfJourney >= 1f)
        {
            _isBowling = false;
            OnBallComplete();
            return;
        }
        
        float distanceToBounce = Vector3.Distance(startPosition.position, _actualBouncePos);
        float bounceProgress = distanceToBounce / _journeyLength;
        
        Vector3 basePosition;
        
        if (fractionOfJourney < bounceProgress)
        {
            // BEFORE BOUNCE - NO spin effect yet, travels straight
            float currentProgress = fractionOfJourney / bounceProgress;
            basePosition = Vector3.Lerp(startPosition.position, _actualBouncePos, currentProgress);
            
            // Parabolic height
            float height = 4f * currentProgress * (1f - currentProgress) * 2f;
            basePosition.y += height;
            
            // NO lateral movement before bounce for spin balls
        }
        else
        {
            // AFTER BOUNCE - Spin turns the ball here
            if (!_hasBounced)
            {
                _hasBounced = true;
                OnBounce();
            }
            
            float currentProgress = (fractionOfJourney - bounceProgress) / (1f - bounceProgress);
            basePosition = Vector3.Lerp(_actualBouncePos, _actualTargetPos, currentProgress);
            
            // APPLY SPIN - turns sharply after bounce
            float spinAmount = spinValue * spinIntensity * currentProgress;
            basePosition.x += spinAmount;
        }
        
        transform.position = basePosition;
        
        // Visual rotation - heavy spin
        if (Mathf.Abs(spinValue) > 0.1f)
        {
            float spinSpeed = spinValue * 1500f;
            transform.Rotate(Vector3.up * spinSpeed * Time.fixedDeltaTime, Space.World);
            transform.Rotate(Vector3.forward * 800f * Time.fixedDeltaTime);
        }
    }

    private void OnBounce()
    {
        print("Spin ball bounced - turning now!");
    }

    private void OnBallComplete()
    {
        print("Spin ball reached target");
        LevelManager.Instance.SetUp();
    }
}