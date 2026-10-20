using System.Collections;
using Interfaces;
using UnityEngine;

namespace Controller
{
    // Test-only feedback so enemy attacks have something visible to confirm against
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerHitFeedback : MonoBehaviour, IDamageable
    {
        private SpriteRenderer _spriteRenderer;
        private Color _defaultColor;
        private PlayerController _playerController;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _defaultColor = _spriteRenderer.color;
            _playerController = GetComponent<PlayerController>();
        }

        public void TakeDamage(int damage, Vector2 knockback)
        {
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
