using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class SwingBall2 : MonoBehaviour
{
    [Header("Delivery Settings")] 
    public float deliverySpeed;

    [Space(15), Header("physics Turning")] 
    [Range(-100, 100)] public int swingStrength;

    [Space(15), Header("Bounce Settings")] 
    [Range(0f, 1f)] public float bounciness;
    [Range(0f, 1f)] public float gripLoss;

    private Transform _startPosition;
    private Transform _bouncePosition;

    private Rigidbody rb;
    private bool hasBounced = false;
    private bool isThrowen = false;
    private Vector3 currentVelocity;
    private float effectiveSwing;
    private float gravity = -9.81f;
    private float accuracy;
    
    private float swingIntensity = 0.8f;

    public void ThrowBall(Transform startPo, Transform bouncePos, float acc)
    {
        _startPosition = startPo;
        _bouncePosition = bouncePos;
        accuracy = acc;

        transform.position = _startPosition.position;
        hasBounced = false;;

        // 1. Calculate horizontal distance and flight time
        Vector3 startPos = transform.position;
        float horizontalDistance = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), 
                                                   new Vector3(_bouncePosition.position.x, 0, _bouncePosition.position.z));
        float timeToBounce = horizontalDistance / deliverySpeed;

        // 2. Calculate the swing acceleration
        float accelMag = swingStrength * accuracy * swingIntensity;

        // 3. Find the direction to the target
        Vector3 dirToTarget = (_bouncePosition.position - startPos).normalized;
        
        // 4. Calculate the 'Swing Axis' (sideways)
        Vector3 swingAxis = Vector3.Cross(dirToTarget, Vector3.up).normalized;

        // 5. CALCULATE INITIAL VELOCITY (The Magic Formula)
        // V_initial = (Distance / Time) - (0.5 * Acceleration * Time)
        // This subtraction cancels out the drift perfectly.
        Vector3 baseVel = dirToTarget * deliverySpeed;
        
        // Vertical correction for gravity
        baseVel.y = (_bouncePosition.position.y - startPos.y - (0.5f * gravity * timeToBounce * timeToBounce)) / timeToBounce;

        // Horizontal correction for swing
        // We subtract exactly half of the acceleration * time to compensate for the curve
        Vector3 swingCorrection = swingAxis * (0.5f * accelMag * timeToBounce);
        
        currentVelocity = baseVel - swingCorrection;
        isThrowen = true;
    }

    void FixedUpdate()
    {
        if (!isThrowen) return;

        float dt = Time.fixedDeltaTime;

        // Update Position
        transform.position += currentVelocity * dt;

        // Apply Gravity
        currentVelocity.y += gravity * dt;

        if (!hasBounced)
        {
            // APPLY SWING
            // We use the same swingAxis used in the launch calculation for perfect accuracy
            Vector3 dirToTarget = (_bouncePosition.position - _startPosition.position).normalized;
            Vector3 swingAxis = Vector3.Cross(dirToTarget, Vector3.up).normalized;
            
            float accelMag = swingStrength * accuracy * swingIntensity;
            currentVelocity += swingAxis * accelMag * dt;

            CheckForImpact();
        }
    }

    void CheckForImpact()
    {
        // Detect bounce when ball passes target height
        if (transform.position.y <= _bouncePosition.position.y && currentVelocity.y < 0)
        {
            hasBounced = true;

            StartCoroutine(OnBallComplete());
            
            // Snap to ground
            transform.position = new Vector3(transform.position.x, _bouncePosition.position.y, transform.position.z);

            // STOP SWING logic happens automatically because hasBounced is true
            
            // PHYSICS: Bounce up and continue straight on the tangent
            currentVelocity.y = -currentVelocity.y * bounciness;
            currentVelocity.x *= (1 - gripLoss);
            currentVelocity.z *= (1 - gripLoss);
        }
    }
    
    IEnumerator OnBallComplete()
    {
        yield return new WaitForSeconds(2f);
        isThrowen = false;
        print("Swing ball reached target");
        LevelManager.Instance.SetUp();
    }
}