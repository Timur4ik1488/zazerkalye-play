using UnityEngine;

namespace Zazerkalye.Player
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new(0.6f, 3.4f, -6.2f);
        public float FollowSmooth = 8f;
        public float LookSmooth = 10f;
        public float ShakeDecay = 8f;

        Vector3 _shake;
        float _shakeMag;

        public void Punch(float amount = 0.18f) => _shakeMag = Mathf.Max(_shakeMag, amount);

        void LateUpdate()
        {
            if (Target == null) return;
            var flat = Vector3.ProjectOnPlane(Target.forward, Vector3.up).normalized;
            if (flat.sqrMagnitude < 0.01f) flat = Vector3.forward;
            var desired = Target.position + Vector3.up * Offset.y - flat * Mathf.Abs(Offset.z)
                          + Vector3.Cross(Vector3.up, flat) * Offset.x;
            if (_shakeMag > 0.001f)
            {
                _shake = Random.insideUnitSphere * _shakeMag;
                _shakeMag = Mathf.Lerp(_shakeMag, 0f, Time.deltaTime * ShakeDecay);
            }
            else _shake = Vector3.zero;
            transform.position = Vector3.Lerp(transform.position, desired + _shake, 1f - Mathf.Exp(-FollowSmooth * Time.deltaTime));
            var look = Target.position + Vector3.up * 1.4f;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation((look - transform.position).normalized, Vector3.up),
                1f - Mathf.Exp(-LookSmooth * Time.deltaTime));
        }
    }
}
