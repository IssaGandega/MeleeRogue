using System.Collections;
using UnityEngine;
using Weapons;

namespace Controller
{
    [RequireComponent(typeof(InputReader))]
    // ReSharper disable Unity.PerformanceAnalysis
    public class PlayerController : MonoBehaviour
    {
        public enum PlayerState
        {
            Idle,
            Dashing,
            Attacking
        }

        public PlayerState State { get; private set; } = PlayerState.Idle;

        private InputReader _inputReader;

        [Header("PlayerStats")]
        [SerializeField] private float playerSpeed = 5f;
        [SerializeField] private float rotationSpeed = 800f;
        
        [Header("Dash")]
        [SerializeField] private float dashDuration = .5f;
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashSpeedModifier = .1f;
        private readonly Cooldown _dashCooldown = new Cooldown(1f);
        private readonly InputBuffer _dashBuffer = new InputBuffer(.15f);

        [Header("Attack")] 
        [SerializeField] private float attackSpeedModifier = .5f;
        [SerializeField] private Weapon currentWeapon;
        
        private readonly Cooldown _attackCooldown = new Cooldown(2f);
        private readonly InputBuffer _attackBuffer = new InputBuffer(.15f);

        private Coroutine _activeAttackCoroutine;

        [Header("Knockback")]
        [SerializeField] private float knockbackDecay = 10f;
        private KnockbackHandler _knockback;

        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();
            _knockback = new KnockbackHandler(knockbackDecay);
        }

        public void ApplyKnockback(Vector2 knockback)
        {
            _knockback.Apply(knockback);
        }

        private void OnEnable()
        {
            _inputReader.DashRequested += OnDashRequested;
            _inputReader.AttackRequested += OnAttackRequested;
        }

        private void OnDisable()
        {
            _inputReader.DashRequested -= OnDashRequested;
            _inputReader.AttackRequested -= OnAttackRequested;
        }

        private void OnDashRequested()
        {
            _dashBuffer.Request();
        }

        private void OnAttackRequested()
        {
            _attackBuffer.Request();
        }

        private void Update()
        {
            Move();
            LookAt();
            TryConsumeDashBuffer();
            TryConsumeAttackBuffer();
            transform.position += (Vector3)_knockback.Tick(Time.deltaTime);
        }

        private void Move()
        {
            float speed;
            switch (State)
            {
                case PlayerState.Idle:
                    speed = playerSpeed;
                    break;
                case PlayerState.Dashing:
                    speed = playerSpeed * dashSpeedModifier;
                    break;
                case PlayerState.Attacking:
                    speed = playerSpeed * attackSpeedModifier;
                    break;
                default:
                    speed = playerSpeed;
                    break;
            }
                
            if (_inputReader.MoveDirection != Vector2.zero)
            {
                transform.position += (Vector3)(_inputReader.MoveDirection * (speed * Time.deltaTime));
            }
        }

        private void LookAt()
        {
            if (State == PlayerState.Attacking) return;

            Vector2 toAim = (Vector2)(_inputReader.AimPosition - transform.position);
            if (toAim.sqrMagnitude < 0.0001f) return; // cursor right on top of the player: no meaningful direction, keep current rotation

            var angle = Vector2.SignedAngle(Vector2.up, toAim.normalized);
            transform.eulerAngles = new Vector3(0,0,Mathf.MoveTowardsAngle(transform.eulerAngles.z, angle, rotationSpeed * Time.deltaTime));
        }

        private void TryConsumeDashBuffer()
        {
            if (!_dashBuffer.HasFreshRequest() || State == PlayerState.Dashing || !_dashCooldown.IsReady() ||
                _inputReader.MoveDirection == Vector2.zero) return;

            StartCoroutine(DashCoroutine());

            IEnumerator DashCoroutine()
            {
                // Cancel any attack in progress
                if (State == PlayerState.Attacking)
                    CancelAttack();

                State = PlayerState.Dashing;
                var dashStartTime = Time.time;
                Vector3 dashDirection = _inputReader.MoveDirection;
                    
                while (dashStartTime + dashDuration > Time.time)
                {
                    transform.position += (Vector3)(dashDirection * (dashSpeed * Time.deltaTime));
                    yield return null;
                }
                _dashBuffer.Consume();
                _dashCooldown.Trigger();
                
                //Same as attack if I want to cancel dash someday
                if (State == PlayerState.Dashing)
                    State = PlayerState.Idle;
            }
        }

        private void TryConsumeAttackBuffer()
        {
            if (!_attackBuffer.HasFreshRequest() || State == PlayerState.Attacking || State == PlayerState.Dashing ||
                !currentWeapon.GetCooldown().IsReady()) return;

            _activeAttackCoroutine = StartCoroutine(AttackCoroutine());

            IEnumerator AttackCoroutine()
            {
                State = PlayerState.Attacking;
                yield return currentWeapon.Attack();
                FinishAttack();
            }
        }
        
        private void FinishAttack()
        {
            _activeAttackCoroutine = null;
            _attackBuffer.Consume();
            currentWeapon.GetCooldown().Trigger();

            if (State == PlayerState.Attacking)
                State = PlayerState.Idle;
        }

        // Called when a dash interrupts an attack
        private void CancelAttack()
        {
            if (_activeAttackCoroutine != null)
                StopCoroutine(_activeAttackCoroutine);

            currentWeapon.CancelAnim();
            FinishAttack();
        }
    }
}