using UnityEngine;

namespace Managers
{
    // Shared velocity + decay, used by both Enemy and PlayerController
    public class KnockbackHandler
    {
        private readonly float _decay;
        private Vector2 _velocity;

        public KnockbackHandler(float decay)
        {
            _decay = decay;
        }

        public void Apply(Vector2 knockback)
        {
            _velocity += knockback;
        }

        // Call once per frame, returns this frame's displacement
        public Vector2 Tick(float deltaTime)
        {
            if (_velocity == Vector2.zero) return Vector2.zero;

            Vector2 displacement = _velocity * deltaTime;
            _velocity = Vector2.Lerp(_velocity, Vector2.zero, 1 - Mathf.Exp(-_decay * deltaTime));
            return displacement;
        }
    }
}
