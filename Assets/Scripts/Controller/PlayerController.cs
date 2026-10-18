using System.Collections;
using UnityEngine;

namespace Controller
{
    public class PlayerController : MonoBehaviour
    {
        public enum PlayerState
        {
            Idle,
            Dashing,
            Attacking,
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
        [SerializeField] private float attackDuration = 1f;

        [SerializeField] private float attackSpeedModifier = .5f;
        private readonly Cooldown _attackCooldown = new Cooldown(2f);
        private readonly InputBuffer _attackBuffer = new InputBuffer(.15f);
        
        
        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();
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
            if (State.Equals(PlayerState.Attacking)) return;
            var angle = Vector2.SignedAngle(Vector2.up, (Vector2)(_inputReader.AimPosition-transform.position).normalized);
            transform.eulerAngles = new Vector3(0,0,Mathf.MoveTowardsAngle(transform.eulerAngles.z, angle, rotationSpeed * Time.deltaTime));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void TryConsumeDashBuffer()
        {
            if (!_dashBuffer.HasFreshRequest() || State == PlayerState.Dashing || !_dashCooldown.IsReady() ||
                _inputReader.MoveDirection == Vector2.zero) return;

            StartCoroutine(DashCoroutine());
            return;

            IEnumerator DashCoroutine()
            {
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
                !_attackCooldown.IsReady()) return;

            StartCoroutine(AttackCoroutine());
            return;

            IEnumerator AttackCoroutine()
            {
                State = PlayerState.Attacking;
                yield return new WaitForSeconds(attackDuration);
                _attackBuffer.Consume();
                _attackCooldown.Trigger();
                
                //Verify to not go into idle if attack has been overwritten by a dash
                if (State == PlayerState.Attacking)
                    State = PlayerState.Idle;
            }
        }
    }
}