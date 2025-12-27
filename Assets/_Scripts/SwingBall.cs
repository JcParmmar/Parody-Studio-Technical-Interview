using UnityEngine;

public class SwingBall : MonoBehaviour
{
    [Header("Required Positions")]
    public Transform startPosition;
    public Transform bouncePosition;
    
    [Header("Swing Settings")]
    [Range(-1f, 1f)]
    public int swingValue;

    [Header("Physics Settings")]
    public float ballSpeed = 25f;
    [Range(0, 1)] public float swingIntensity = 2f;
    
    private Rigidbody _rb;
    private float _journeyLength = 20f;
    private float _startTime;
    private bool _isBowling;
    private bool _postBounceInit;
    private Vector3 _postBounceVelocity;
    private Vector3 _actualBouncePos;
    private Vector3 _lateralPreDir;

    private bool _hasBounced;
    private float _postBounceStartTime;
    private Vector3 _continuationDirection;
    private float _postBounceJourneyLength;
    // private Vector3 _actualTargetPos;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate()
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
        
        _actualBouncePos = bouncePosition.position;
        
        // _actualTargetPos = targetPosition.position;
        
        // _journeyLength = Vector3.Distance(startPosition.position, _actualTargetPos);
        _startTime = Time.time;
        
        _isBowling = true;
    }
    
    void UpdateBallTrajectory()
    {
        // time mapping
        float timeSinceStart = Time.time - _startTime;
        float totalTime = Mathf.Max(0.0001f, _journeyLength / ballSpeed);
        float overallFrac = Mathf.Clamp01(timeSinceStart / totalTime);

        if (overallFrac >= 1f)
        {
            _isBowling = false;
            OnBallComplete();
            return;
        }

        // where bounce occurs in fraction of whole journey
        float distToBounce = Vector3.Distance(startPosition.position, _actualBouncePos);
        float bounceFrac = Mathf.Clamp01(distToBounce / _journeyLength);

        // Helper: base (no-lateral) pre-bounce position (strictly interpolates start->bounce)
        Vector3 PreBounceBasePos(float tPre)
        {
            tPre = Mathf.Clamp01(tPre);
            Vector3 p = Vector3.Lerp(startPosition.position, _actualBouncePos, tPre);
            // vertical arc pre-bounce (peaks mid of segment)
            float preArcMul = 2f;
            p.y += preArcMul * 4f * tPre * (1f - tPre) * 0.25f;
            return p;
        }

        // --- BEFORE BOUNCE: follow start -> bounce parametric path (with swing) ---
        if (overallFrac < bounceFrac)
        {
            float tPre = Mathf.Clamp01(overallFrac / Mathf.Max(bounceFrac, Mathf.Epsilon)); // 0..1 in pre segment
            Vector3 basePre = PreBounceBasePos(tPre);

            var segDirPre = (_actualBouncePos - startPosition.position).normalized;
            _lateralPreDir = Vector3.Cross(segDirPre, Vector3.up).normalized;
            if (_lateralPreDir == Vector3.zero) _lateralPreDir = transform.right;

            // lateral shape: 0 at t=0 and t=1 (ensures exact bounce at _actualBouncePos)
            float lateralShapePre = Mathf.Sin(Mathf.PI * tPre); // 0..1..0
            float growthPre = Mathf.Pow(tPre, 0.9f); // makes swing small at release
            float lateralAmountPre = swingValue * swingIntensity * lateralShapePre * growthPre;

            transform.position = basePre + _lateralPreDir * lateralAmountPre;

            // rotation around flight direction
            transform.Rotate(segDirPre, 360f * swingValue * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.forward * (300f * Time.fixedDeltaTime), Space.Self);

            print(_lateralPreDir);
            return;
        }

        if (!_postBounceInit)
        {
            _postBounceVelocity = _lateralPreDir;
            
            transform.position = _actualBouncePos;
            _postBounceInit = true;
            return;
        }
        
        float distanceToBounce = Vector3.Distance(startPosition.position, _actualBouncePos);
        float bounceProgress = distanceToBounce / _journeyLength;
        float currentProgress;
        currentProgress = (overallFrac - bounceProgress) / (1f - bounceProgress);
            
        // Calculate how far to travel from bounce point
        float postBounceDistance = currentProgress * 10;
        transform.position += _actualBouncePos + _lateralPreDir * Time.deltaTime * postBounceDistance;
        
        transform.Rotate(_postBounceVelocity.normalized, 360f * swingValue * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward * (300f * Time.fixedDeltaTime), Space.Self);
    }
    
    void OnBallComplete()
    {
        print("Swing ball reached target");
        LevelManager.Instance.SetUp();
    }
}