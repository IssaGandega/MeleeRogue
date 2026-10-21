using System.Collections;
using Controller;
using Interfaces;
using Managers;
using Scriptables.Enemies;
using UnityEngine;

namespace Enemies
{
    public class Pursuer : Enemy
    {
        private PursuerData _data;
        private PlayerController _player;
        private Cooldown _attackCooldown;

        private Coroutine _attackSequence;
        private Animator _animator;
        private bool _isMoving;
        private string _currentAnimState;

        private static readonly Color StaggerColor = new Color(0.54f, 0.17f, 0.89f);

        [SerializeField] private float lungeStretchIntensity = 0.3f;

        protected override void Awake()
        {
            base.Awake();
            _data = (PursuerData)Data;
            _attackCooldown = new Cooldown(_data.AttackCooldown);
            _animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            _player = GameManager.Instance.Player;
        }

        // Appelé depuis Enemy.Update() -- ne PAS redéfinir Update() ici, voir le commentaire sur OnUpdate().
        protected override void OnUpdate()
        {
            if (_player == null) return;

            BodyRenderer.flipX = _player.transform.position.x < transform.position.x;

            string state = _isMoving ? "Pursuer_Walk" : "Pursuer_Idle";
            if (state != _currentAnimState)
            {
                _currentAnimState = state;
                _animator.Play(state);
            }
        }

        protected override void Move()
        {
            if (_player == null || State == EnemyState.Attacking)
            {
                _isMoving = false;
                return;
            }

            Vector3 direction = (_player.transform.position - transform.position).normalized;
            transform.position += direction * (_data.MoveSpeed * Time.deltaTime);
            _isMoving = direction.sqrMagnitude > 0.0001f;
        }

        protected override void Attack()
        {
            if (_player == null || State == EnemyState.Attacking || !_attackCooldown.IsReady()) return;

            float distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance > _data.AttackRange) return;

            _attackSequence = StartCoroutine(AttackSequence());
        }

        // Interrupts whatever phase is running (windup, lunge, even recovery) with a longer punish window
        protected override bool OnStagger()
        {
            if (State != EnemyState.Attacking) return false;

            if (_attackSequence != null)
            {
                StopCoroutine(_attackSequence);
                _attackSequence = null;
            }

            StartCoroutine(StaggerRecovery());
            return true;
        }

        private IEnumerator StaggerRecovery()
        {
            FlashFx.Flash(StaggerColor, _data.StaggerDuration);
            yield return new WaitForSecondsRealtime(_data.StaggerDuration);
            _attackCooldown.Trigger();
            State = EnemyState.Idle;
        }

        private IEnumerator AttackSequence()
        {
            State = EnemyState.Attacking;
            FlashFx.Flash(Color.red, _data.AttackWindup);

            // Telegraph: vibrate on the spot, committing to nothing yet.
            Vector3 startPos = transform.position;
            yield return Vibrate(startPos, _data.AttackWindup);

            Vector3 direction = (_player.transform.position - startPos).normalized;
            Vector3 lungeTarget = startPos + direction * _data.AttackRange;
            SquashFx.Stretch(direction, lungeStretchIntensity);

            float elapsed = 0f;
            bool contactMade = false;
            while (elapsed < _data.LungeDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, lungeTarget, elapsed / _data.LungeDuration);

                Vector2 pushDirection = ((Vector2)_player.transform.position - (Vector2)transform.position).normalized;
                if (Vector3.Distance(transform.position, _player.transform.position) <= _data.ContactRange && _player.State != PlayerController.PlayerState.Dashing)
                {
                    _player.GetComponent<IDamageable>()?.TakeDamage(_data.ContactDamage, pushDirection * _data.ContactKnockback, _data.HitWeight);
                    contactMade = true;
                    break;
                }

                yield return null;
            }

            if (!contactMade)
                transform.position = lungeTarget;

            // Short freeze once the lunge ends
            yield return new WaitForSeconds(_data.AttackRecovery);

            _attackCooldown.Trigger();
            State = EnemyState.Idle;
            _attackSequence = null;
        }

        private IEnumerator Vibrate(Vector3 origin, float duration, float amplitude = 0.05f)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = origin + (Vector3)Random.insideUnitCircle * amplitude;
                yield return null;
            }
            transform.position = origin;
        }
    }
}
