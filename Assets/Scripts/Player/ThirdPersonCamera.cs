using UnityEngine;
using UnityEngine.EventSystems;

namespace Zazerkalye.Player
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 7.4f;
        public float Height = 2.2f;
        public float FollowSmooth = 18f;
        public float LookSmooth = 14f;
        public float ShakeDecay = 8f;
        public float YawSpeed = 140f;
        public float MouseSensitivity = 2.4f;
        public float MinPitch = 8f;
        public float MaxPitch = 55f;
        public float MinDistance = 4.2f;
        public float MaxDistance = 14f;

        float _yaw = 0f;
        float _pitch = 22f;
        Vector3 _shake;
        float _shakeMag;
        Vector3 _lastMouse;
        bool _hasLastMouse;

        public void Punch(float amount = 0.18f) => _shakeMag = Mathf.Max(_shakeMag, amount);

        public void SnapToTarget()
        {
            if (Target == null) return;
            ApplyPose(1f);
            _shakeMag = 0f;
            _shake = Vector3.zero;
        }

        void LateUpdate()
        {
            if (Target == null || !Target.gameObject.activeInHierarchy) return;
            ReadOrbit();
            ApplyPose(1f - Mathf.Exp(-FollowSmooth * Time.deltaTime));
        }

        void ReadOrbit()
        {
            float dt = Time.deltaTime;
            if (Input.GetKey(KeyCode.Q)) _yaw -= YawSpeed * dt;
            if (Input.GetKey(KeyCode.E)) _yaw += YawSpeed * dt;

            var mouse = Input.mousePosition;
            var delta = _hasLastMouse ? mouse - _lastMouse : Vector3.zero;
            _lastMouse = mouse;
            _hasLastMouse = true;

            bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            bool orbit = !overUi && (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2));
            if (orbit)
            {
                _yaw += delta.x * MouseSensitivity * 0.18f;
                _pitch -= delta.y * MouseSensitivity * 0.18f;
            }
            _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.0001f)
                Distance = Mathf.Clamp(Distance - scroll * 1.1f, MinDistance, MaxDistance);
        }

        void ApplyPose(float t)
        {
            var rot = Quaternion.Euler(_pitch, _yaw, 0f);
            var desired = Target.position + Vector3.up * Height + rot * new Vector3(0f, 0f, -Distance);
            if (_shakeMag > 0.001f)
            {
                _shake = Random.insideUnitSphere * _shakeMag;
                _shakeMag = Mathf.Lerp(_shakeMag, 0f, Time.deltaTime * ShakeDecay);
            }
            else _shake = Vector3.zero;
            transform.position = Vector3.Lerp(transform.position, desired + _shake, t);
            var look = Target.position + Vector3.up * 1.4f;
            var lookRot = Quaternion.LookRotation((look - transform.position).normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 1f - Mathf.Exp(-LookSmooth * Time.deltaTime));
        }
    }
}
