using UnityEngine;

public class SwingBall : MonoBehaviour
{
    [Header("Required Positions")]
    public Transform startPosition;
    public Transform bouncePosition;
    public Transform targetPosition;
    
    [Header("Swing Settings")]
    [Range(-1f, 1f)]
    public int swingValue;

    [Header("Physics Settings")]
    public float ballSpeed = 25f;
    [Range(0, 1)] public float swingIntensity = 2f;
    
    private Rigidbody _rb;
    private float _journeyLength;
    private float _startTime;
    private bool _isBowling;
    private Vector3 _actualBouncePos;
    private Vector3 _actualTargetPos;
    
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
        
        _actualTargetPos = targetPosition.position;
        
        _journeyLength = Vector3.Distance(startPosition.position, _actualTargetPos);
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

            // lateral direction for pre-bounce: perpendicular to (bounce - start)
            Vector3 segDirPre = (_actualBouncePos - startPosition.position).sqrMagnitude > 1e-6f
                ? (_actualBouncePos - startPosition.position).normalized
                : transform.forward;
            Vector3 lateralPreDir = Vector3.Cross(segDirPre, Vector3.up).normalized;
            if (lateralPreDir == Vector3.zero) lateralPreDir = transform.right;

            // lateral shape: 0 at t=0 and t=1 (ensures exact bounce at _actualBouncePos)
            float lateralShapePre = Mathf.Sin(Mathf.PI * tPre); // 0..1..0
            float growthPre = Mathf.Pow(tPre, 0.9f); // makes swing small at release
            float lateralAmountPre = swingValue * swingIntensity * lateralShapePre * growthPre;

            transform.position = basePre + lateralPreDir * lateralAmountPre;

            // rotation around flight direction
            transform.Rotate(segDirPre, 360f * swingValue * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.forward * 300f * Time.fixedDeltaTime, Space.Self);
            return;
        }

        // --- AT/AFTER BOUNCE: ensure exact bounce point at bounce time, then continue straight (REFLECT) ---
        // compute small samples around bounce to approximate incoming direction
        float eps = 0.0006f;
        float sampleBeforeF = Mathf.Clamp01(bounceFrac - eps);

        Vector3 sampleBefore = PreBounceBasePos(Mathf.Clamp01(sampleBeforeF / Mathf.Max(bounceFrac, Mathf.Epsilon)));
        Vector3 sampleAt     = PreBounceBasePos(1f); // guaranteed _actualBouncePos (no lateral)
        // sampleAfter we approximate by moving slightly forward from bounce along the pre-bounce tangent
        Vector3 preTangent = (sampleAt - sampleBefore).sqrMagnitude > 1e-6f ? (sampleAt - sampleBefore).normalized : (_actualBouncePos - startPosition.position).normalized;
        
        // incoming direction (towards bounce)
        Vector3 incomingDir = (sampleAt - sampleBefore).sqrMagnitude > 1e-6f ? (sampleAt - sampleBefore).normalized : preTangent;
        if (incomingDir.sqrMagnitude < 1e-6f) incomingDir = (startPosition.position - _actualBouncePos).normalized;

        // reflect incoming direction across the ground normal to get a straight post-bounce trajectory
        Vector3 groundNormal = Vector3.up; // assume flat ground
        Vector3 reflectedDir = Vector3.Reflect(incomingDir, groundNormal).normalized;

        // remaining distance after bounce
        float remainingDist = Mathf.Max(0f, _journeyLength - distToBounce);

        // map time into post-bounce 0..1
        float timeToBounce = totalTime * bounceFrac;
        float timeAfterBounce = Mathf.Max(0f, timeSinceStart - timeToBounce);
        float postTotalTime = Mathf.Max(0.0001f, totalTime - timeToBounce);
        float tPost = Mathf.Clamp01(timeAfterBounce / postTotalTime); // 0..1 over post-bounce

        // Continuation end = bounce + reflectedDir * remainingDist
        Vector3 continuationEnd = _actualBouncePos + reflectedDir * remainingDist;

        // base post-bounce position: linear interpolation from bounce -> continuationEnd
        Vector3 basePost = Vector3.Lerp(_actualBouncePos, continuationEnd, tPost);

        // small post-bounce arc to make bounce feel physical (peaks early)
        float postArcMul = 0.6f;
        float postHeight = postArcMul * 4f * tPost * (1f - tPost) * 0.25f;
        basePost.y += postHeight;

        // AFTER BOUNCE: we want "straight" flight — do NOT apply lateral swing here.
        transform.position = basePost;

        // rotate visually around reflected direction to show seam/spin (optional)
        transform.Rotate(reflectedDir, 360f * swingValue * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward * 300f * Time.fixedDeltaTime, Space.Self);
    }
    
    void OnBallComplete()
    {
        print("Swing ball reached target");
        LevelManager.Instance.SetUp();
    }
}