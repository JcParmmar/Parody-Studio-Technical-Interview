using UnityEngine;

public class MarkerMover : MonoBehaviour
{
        public Transform marker;
        public float speedOfMove = 2f;

        [Header("Marker Position restrictions")]
        [SerializeField] private float maxXMove;
        [SerializeField] private float minXMove;
        [SerializeField] private float maxZMove;
        [SerializeField] private float minZMove;

        private InputSystem _input;

        private void Start()
        {
                _input = InputSystem.Instance;
        }

        private void Update()
        {
                if (_input.Move != Vector2.zero)
                {
                        marker.position += (_input.Move.x * marker.right + _input.Move.y * marker.forward) * (Time.deltaTime * speedOfMove);
                        marker.position = new Vector3(Mathf.Clamp(marker.position.x, minXMove, maxXMove),marker.position.y, Mathf.Clamp(marker.position.z, minZMove, maxZMove));
                }
        }
}