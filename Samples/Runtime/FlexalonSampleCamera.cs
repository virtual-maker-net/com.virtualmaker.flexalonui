using UnityEngine;

namespace Flexalon.Samples
{
    // Simple camera controller.
    // Use WASD or arrows to move. Rotate with right mouse button.
    // Pan with mouse wheel button.
    public class FlexalonSampleCamera : MonoBehaviour
    {
        public float Speed = 0.2f;
        public float RotateSpeed = 0.2f;
        public float InterpolationSpeed = 20.0f;

        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private float _alpha;
        private float _beta;
        private Vector3 _mousePos;

        void Start()
        {
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
            var euler = _targetRotation.eulerAngles;
            _alpha = euler.y;
            _beta = euler.x;
        }

        void Update()
        {
#if UNITY_GUI
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
            {
                return;
            }
#endif

            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                _targetPosition += transform.forward * Speed;
            }

            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                _targetPosition += -transform.right * Speed;
            }

            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                _targetPosition += transform.right * Speed;
            }

            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                _targetPosition += -transform.forward * Speed;
            }

            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                _mousePos = Input.mousePosition;
            }

            if (Input.GetMouseButton(1))
            {
                var delta = Input.mousePosition - _mousePos;
                _alpha += delta.x * RotateSpeed;
                _beta -= delta.y * RotateSpeed;
                _targetRotation = Quaternion.Euler(_beta, _alpha, 0);
                _mousePos = Input.mousePosition;
            }

            if (Input.GetMouseButtonDown(2))
            {
                _mousePos = Input.mousePosition;
            }

            if (Input.GetMouseButton(2))
            {
                var delta = Input.mousePosition - _mousePos;
                _targetPosition -= delta.y * transform.up * Speed;
                _targetPosition -= delta.x * transform.right * Speed;
                _mousePos = Input.mousePosition;
            }

            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * InterpolationSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * InterpolationSpeed);
        }
    }
}