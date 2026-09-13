using UnityEngine;

namespace Zazerkalye.Player
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new(0.6f, 3.8f, -7.4f);
        public float FollowSmooth = 9f;
        public float LookSmooth = 12f;
        public float ShakeDecay = 8f;

        Vector3 _shake;
        float _shakeMag;

        public void Punch(float amount = 0.18f) => _shakeMag = Mathf.Max(_shakeMag, amount);

        public void SnapToTarget()
        {
            if (Target == null) return;
            transform.position = Target.position + Offset;
            var look = Target.position + Vector3.up * 1.4f;
            transform.rotation = Quaternion.LookRotation((look - transform.position).normalized, Vector3.up);
            _shakeMag = 0f;
            _shake = Vector3.zero;
        }

        void LateUpdate()
        {
            if (Target == null || !Target.gameObject.activeInHierarchy) return;
            var desired = Target.position + Offset;
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
