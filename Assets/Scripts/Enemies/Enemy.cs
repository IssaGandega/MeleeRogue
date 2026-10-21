using Controller;
using Interfaces;
using Managers;
using Scriptables.Enemies;
using Scriptables.Events;
using UnityEngine;

namespace Enemies
{
    //[RequireComponent(typeof(SpriteRenderer))]
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

        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private Color staggerColor = new Color(1f, 0.7f, 0.1f);
        [SerializeField] private float flashDuration = 0.12f;

        [SerializeField] private Transform visualTransform;
        [SerializeField] private float squashIntensity = 0.3f;
        [SerializeField] private float scaleRecoverySpeed = 12f;

        public EnemyState State { get; protected set; } = EnemyState.Idle;

        private int _currentHealth;
        private KnockbackHandler _knockback;
        private SpriteRenderer _spriteRenderer;
        private HitFlash _hitFlash;
        private SquashStretch _squashStretch;
        private HitEventChannel _hitEventChannel;

        protected HitFlash FlashFx => _hitFlash;
        protected SquashStretch SquashFx => _squashStretch;
        protected SpriteRenderer BodyRenderer => _spriteRenderer;

        protected virtual void Awake()
        {
            _currentHealth = data.MaxHealth;
            _knockback = new KnockbackHandler(knockbackDecay);
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _hitFlash = new HitFlash(this, _spriteRenderer);
            _squashStretch = new SquashStretch(visualTransform, scaleRecoverySpeed);
            _hitEventChannel = GameManager.Instance.HitEventChannel;
        }

        private void Update()
        {
            Move();
            Attack();
            transform.position += (Vector3)_knockback.Tick(Time.deltaTime);
            _squashStretch.Tick(Time.deltaTime);
            OnUpdate();
        }

        protected abstract void Move();
        protected abstract void Attack();

        // Hook pour un enfant qui a besoin de sa propre logique par frame (flip, état d'anim, ...).
        // Ne JAMAIS redéfinir un second "private void Update()" dans une sous-classe : Unity ne garde
        // qu'une seule méthode Update par type concret, celle du type le plus dérivé -- Update() ici,
        // dans Enemy (mouvement, attaque, knockback, retour du squash), arrêterait de tourner.
        protected virtual void OnUpdate() { }

        public void TakeDamage(int damage, Vector2 knockback, float weight)
        {
            _currentHealth -= damage;
            _knockback.Apply(knockback);

            // damage == 0: a pure knockback bump
            if (damage > 0)
            {
                bool staggered = !data.HyperArmor && OnStagger();
                _hitFlash.Flash(staggered ? staggerColor : hitColor, flashDuration);
                if (knockback != Vector2.zero)
                    _squashStretch.Squash(knockback, squashIntensity);
                var hit = new HitInfo(weight, data.HitStopDuration, (Vector2)transform.position, this);
                _hitEventChannel.Raise(hit);
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
            if (player.State == PlayerController.PlayerState.Dashing) return;
            Vector2 pushDirection = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            TakeDamage(0, pushDirection * data.BodyContactKnockback, 0f);
            player.GetComponent<IDamageable>()?.TakeDamage(data.BodyContactDamage, -pushDirection * data.BodyContactKnockback, 0f);
        }

        // Optional hook for a child to react to being hit
        protected virtual void OnDamaged() { }

        protected virtual bool OnStagger() => false;

        protected virtual void Die()
        {
            Destroy(gameObject);
        }
    }
}
