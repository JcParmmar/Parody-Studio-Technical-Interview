using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
        public static InputSystem Instance;

        private void Awake()
        {
                if (Instance && Instance != this) Destroy(this);
                else Instance = this;
        }

        public Vector2 Move { get; private set; }
        public bool ThrowBall { get; private set; }

        public void GetMoveInput(InputAction.CallbackContext context)
        {
                Move = context.ReadValue<Vector2>();
        }

        public void GetThrowBallInput(InputAction.CallbackContext context)
        {
                ThrowBall = context.ReadValueAsButton();
        }
}