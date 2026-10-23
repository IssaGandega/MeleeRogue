using Interfaces;
using Managers;
using Scriptables.Events;
using UnityEngine;

namespace Controller
{
    public class PlayerHitFeedback : MonoBehaviour, IDamageable
    {
        private SpriteRenderer _spriteRenderer;
        private PlayerController _playerController;
        private HitFlash _hitFlash;
        private HitEventChannel _hitEventChannel;

        [SerializeField] private float hitStopDuration;
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private float flashDuration = 0.12f;
        [SerializeField] private float squashIntensity = 0.3f;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _playerController = GetComponent<PlayerController>();
            _hitFlash = new HitFlash(this, _spriteRenderer);
            _hitEventChannel = GameManager.Instance.HitEventChannel;
        }

        public void TakeDamage(int damage, Vector2 knockback, float weight)
        {
            if (_playerController.State == PlayerController.PlayerState.Dashing) return;

            var hit = new HitInfo(weight, hitStopDuration, (Vector2)transform.position, this);

            _hitEventChannel.Raise(hit);
            if (_playerController.armor == false)
            {
                if (knockback != Vector2.zero)
                    _playerController.ApplyKnockback(knockback);

                _hitFlash.Flash(hitColor, flashDuration);

                if (knockback != Vector2.zero)
                    _playerController.Squash(knockback, squashIntensity);
            }
            else
            {
                if (knockback != Vector2.zero)
                    _playerController.ApplyKnockback(knockback/10);
                _hitFlash.Flash(Color.gray3, flashDuration);
                if (knockback != Vector2.zero)
                    _playerController.Squash(knockback/10, squashIntensity/10);
            }
            
        }
    }
}
