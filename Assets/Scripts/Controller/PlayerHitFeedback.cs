using System.Collections;
using Interfaces;
using Managers;
using Scriptables.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controller
{
    // Test-only feedback so enemy attacks have something visible to confirm against
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerHitFeedback : MonoBehaviour, IDamageable
    {
        private SpriteRenderer _spriteRenderer;
        private Color _defaultColor;
        private PlayerController _playerController;
        
        [SerializeField] private float hitStopDuration;
        [SerializeField] private HitEventChannel hitEventChannel;


        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _defaultColor = _spriteRenderer.color;
            _playerController = GetComponent<PlayerController>();
        }

        public void TakeDamage(int damage, Vector2 knockback, float weight)
        {
            if (_playerController.State == PlayerController.PlayerState.Dashing ) return;

            var hit = new HitInfo(weight,hitStopDuration, (Vector2)transform.position, this);
            hitEventChannel.Raise(hit);
            _playerController.ApplyKnockback(knockback);
            StartCoroutine(FlashRed());
        }

        private IEnumerator FlashRed()
        {
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            _spriteRenderer.color = _defaultColor;
        }
    }
}
