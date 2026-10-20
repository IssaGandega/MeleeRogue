using System.Collections;
using Controller;
using Interfaces;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class Enemy : MonoBehaviour, IDamageable
    {
        public enum EnemyState
        {
            Idle,
            Attacking
        }

        [SerializeField] private EnemyData data;

        protected EnemyData Data => data;

        [SerializeField] private float knockbackDecay = 10f;

        public EnemyState State { get; protected set; } = EnemyState.Idle;

        private int _currentHealth;
        private KnockbackHandler _knockback;
        private SpriteRenderer _spriteRenderer;
        private Color _defaultColor;

        protected virtual void Awake()
        {
            _currentHealth = data.MaxHealth;
            _knockback = new KnockbackHandler(knockbackDecay);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _defaultColor = _spriteRenderer.color;
        }

        private void Update()
        {
            Move();
            Attack();
            transform.position += (Vector3)_knockback.Tick(Time.deltaTime);
        }

        protected abstract void Move();
        protected abstract void Attack();

        public void TakeDamage(int damage, Vector2 knockback)
        {
            _currentHealth -= damage;
            _knockback.Apply(knockback);

            // damage == 0: a pure knockback bump
            if (damage > 0)
            {
                bool staggered = !data.HyperArmor && OnStagger();
                if (!staggered)
                    StartCoroutine(FlashColor(Color.red, 0.15f));

                OnDamaged();
            }

            if (_currentHealth <= 0)
                Die();
        }

        // Body touching the player: knockback both ways, damage only to the player
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var player = collision.GetComponent<PlayerController>();
            if (player == null) return;

            Vector2 pushDirection = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            TakeDamage(0, pushDirection * data.BodyContactKnockback);
            player.GetComponent<IDamageable>()?.TakeDamage(data.BodyContactDamage, -pushDirection * data.BodyContactKnockback);
        }

        // Optional hook for a child to react to being hit
        protected virtual void OnDamaged() { }

        protected virtual bool OnStagger() => false;

        protected virtual void Die()
        {
            Destroy(gameObject);
        }

        protected IEnumerator FlashColor(Color color, float duration)
        {
            _spriteRenderer.color = color;
            yield return new WaitForSeconds(duration);
            _spriteRenderer.color = _defaultColor;
        }
    }
}
