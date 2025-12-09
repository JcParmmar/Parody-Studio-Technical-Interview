using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private void Awake()
    {
        if (Instance && Instance != this)
            Destroy(this);
        else Instance = this;
    }

    [SerializeField] AccuracyValue spinAndSwingAccuracy;
    [SerializeField] GameObject ball;
    [SerializeField] Transform leftHandStartingPoint;
    [SerializeField] Transform rightHandStartingPoint;
    [SerializeField] MarkerMover markerPoint;
    [SerializeField] Transform stump;

    [SerializeField] private float effectMutl;

    [Header("UI")]
    [SerializeField] private GameObject accuracySlider;
    [SerializeField] private GameObject swingSpinButtonParent;
    [SerializeField] private GameObject moveMarkerTextParent;
    [SerializeField] private Button swingButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button throwButton;
    [SerializeField] private Button switchHandButton;

    private bool _isLeftHand;
    private bool _isSpin;
    private bool _readyToThrow;
    private bool _markerMoving;
    private bool _oldTap;

    private InputSystem _input;
    private SwingBall _swing;
    private SpinBall _spin;

    private void Start()
    {
        _input = InputSystem.Instance;
        
        _swing =ball.GetComponent<SwingBall>();
        _spin = ball.GetComponent<SpinBall>();
        
        swingButton.onClick.AddListener(() =>
            {
                _isSpin = false;
                
                swingButton.image.color = Color.green;
                spinButton.image.color = Color.white;

                SetMarkerPosition();
            }
        );

        spinButton.onClick.AddListener(() =>
            {
                _isSpin = true;
                
                swingButton.image.color = Color.white;
                spinButton.image.color = Color.green;

                SetMarkerPosition();
            }
        );
        
        throwButton.onClick.AddListener(ThrowBall);
        
        switchHandButton.onClick.AddListener(ChangeHand);
        
        spinButton.onClick?.Invoke();
        
        SetUp();
    }

    private void Update()
    {
        if (_readyToThrow && _input.ThrowBall && !_oldTap)
        {
            _oldTap = true;
            ThrowBall();
        }
        
        if (_markerMoving && _input.ThrowBall && !_oldTap)
        {
            _oldTap = true;
            ReadyToThrow();
        }

        if (!_input.ThrowBall && _oldTap) _oldTap = false;
    }

    public void SetUp()
    {
        ball.transform.position = _isLeftHand ? leftHandStartingPoint.position : rightHandStartingPoint.position;
        _swing.enabled = false;
        _spin.enabled = false;
        
        accuracySlider.SetActive(false);
        throwButton.gameObject.SetActive(false);
        moveMarkerTextParent.SetActive(false);
        swingSpinButtonParent.SetActive(true);
        switchHandButton.gameObject.SetActive(true);
    }

    private void SetMarkerPosition()
    {
        swingSpinButtonParent.SetActive(false);
        moveMarkerTextParent.SetActive(true);
        
        markerPoint.enabled = true;
        _markerMoving = true;
    }

    private void ReadyToThrow()
    {
        accuracySlider.SetActive(true);
        throwButton.gameObject.SetActive(true);
        moveMarkerTextParent.SetActive(false);
        
        spinAndSwingAccuracy.StartMoving();
        _markerMoving = false;
        _readyToThrow = true;
    }

    private void ThrowBall()
    {
        markerPoint.enabled = false;
        _readyToThrow = false;
        switchHandButton.gameObject.SetActive(false);

        if (_isSpin)
        {
            _spin.enabled = true;

            _spin.startPosition = _isLeftHand ? leftHandStartingPoint : rightHandStartingPoint;
            _spin.bouncePosition = markerPoint.transform;
            _spin.targetPosition = stump;

            var t = spinAndSwingAccuracy.StopMoving();
            _spin.spinIntensity = math.abs(t) * effectMutl;
        
            _spin.ThrowBall();
        }
        else
        {
            _swing.enabled = true;

            _swing.startPosition = _isLeftHand ? leftHandStartingPoint : rightHandStartingPoint;
            _swing.bouncePosition = markerPoint.transform;
            _swing.targetPosition = stump;

            var t = spinAndSwingAccuracy.StopMoving();
            _swing.swingIntensity = (1 - math.abs(t)) * effectMutl;
        
            _swing.ThrowBall();
        }
    }

    private void ChangeHand()
    {
        _isLeftHand = !_isLeftHand;
        ball.transform.position = _isLeftHand ? leftHandStartingPoint.position : rightHandStartingPoint.position;
    }
}