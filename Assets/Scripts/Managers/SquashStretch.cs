using UnityEngine;

namespace Managers
{
    public class SquashStretch
    {
        private readonly Transform _target;
        private readonly float _recoverySpeed;
        private readonly Vector3 _restScale;

        public SquashStretch(Transform target, float recoverySpeed)
        {
            if (target == null)
                Debug.LogError("SquashStretch: target transform not assigned.");
            _target = target;
            _recoverySpeed = recoverySpeed;
            _restScale = target != null ? target.localScale : Vector3.one;
        }

        public void Squash(Vector2 axis, float intensity) => Punch(axis, 1f - intensity, 1f + intensity * 0.5f);
        public void Stretch(Vector2 axis, float intensity) => Punch(axis, 1f + intensity, 1f - intensity * 0.5f);

        private void Punch(Vector2 axis, float along, float across)
        {
            if (_target == null) return;

            bool horizontal = Mathf.Abs(axis.x) >= Mathf.Abs(axis.y);
            Vector3 multiplier = horizontal ? new Vector3(along, across, 1f) : new Vector3(across, along, 1f);
            _target.localScale = Vector3.Scale(_restScale, multiplier);
        }

        public void Tick(float deltaTime)
        {
            if (_target == null) return;

            _target.localScale = Vector3.Lerp(_target.localScale, _restScale, 1 - Mathf.Exp(-_recoverySpeed * deltaTime));
        }
    }
}
