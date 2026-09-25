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

        [SerializeField] private float playerSpeed = 5f;
        [SerializeField] private float dashDuration = .1f;
        [SerializeField] private float dashSpeed = 20f;
        
        private readonly Cooldown _dashCooldown = new Cooldown(1f);
        private readonly InputBuffer _dashBuffer = new InputBuffer(.15f);
        
        
        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();
        }

        private void OnEnable()
        {
            _inputReader.DashRequested += OnDashRequested;
            _inputReader.AttackRequested += Attack;
        }

        private void OnDisable()
        {
            _inputReader.DashRequested -= OnDashRequested;
            _inputReader.AttackRequested -= Attack;
        }

        private void OnDashRequested()
        {
            _dashBuffer.Request();
        }

        private void Attack()
        {
            State = PlayerState.Attacking;
        }

        private void Update()
        {
            Move();
            TryConsumeDashBuffer();
        }

        private void Move()
        {
            float speed = State == PlayerState.Dashing ? playerSpeed * 0.1f : playerSpeed;
            if (_inputReader.MoveDirection != Vector2.zero)
            {
                transform.position += (Vector3)(_inputReader.MoveDirection * (speed * Time.deltaTime));
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void TryConsumeDashBuffer()
        {
            if (_dashBuffer.HasFreshRequest() && State != PlayerState.Dashing && _dashCooldown.IsReady() && _inputReader.MoveDirection != Vector2.zero)
            {
                IEnumerator _dashCoroutine()
                {
                    State = PlayerState.Dashing;
                    float dashStartTime = Time.time;
                    Vector3 dashDirection = _inputReader.MoveDirection;
                    
                    while (dashStartTime + dashDuration > Time.time)
                    {
                        transform.position += (Vector3)(dashDirection * (dashSpeed * Time.deltaTime));
                        yield return null;
                    }
                    _dashBuffer.Consume();
                    _dashCooldown.Trigger();
                    State = PlayerState.Idle;
                }
                
                StartCoroutine(_dashCoroutine());
            }
        }
    }
}