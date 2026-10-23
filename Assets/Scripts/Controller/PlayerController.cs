using System;
using System.Collections;
using Inputs;
using Interfaces;
using Managers;
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

        public enum EquippedWeapon
        {
            Sword,
            Spear,
            Hammer
        }

        public PlayerState State{ get; private set; } = PlayerState.Idle;

        public bool IsFacingLeft { get; private set; }

        private InputReader _inputReader;

        [Header("PlayerStats")]
        [SerializeField] private float playerSpeed = 5f;
        [SerializeField] private float rotationSpeed = 800f;

        [Header("Dash")]
        [SerializeField] private float dashDuration = .5f;
        [SerializeField] private float dashSpeed = 20f;
        private readonly Cooldown _dashCooldown = new Cooldown(1f);
        private readonly InputBuffer _dashBuffer = new InputBuffer(.15f);

        [Header("Attack")]
        [SerializeField] private float attackSpeedModifier = .5f;
        [SerializeField] private Weapon currentWeapon;
        [SerializeField] private Transform weaponArm;

        private readonly InputBuffer _attackBuffer = new InputBuffer(.15f);

        private Coroutine _activeAttackCoroutine;

        [Header("Knockback")]
        [SerializeField] private float knockbackDecay = 10f;
        private KnockbackHandler _knockback;

        [Header("Squash & Stretch")]
        [SerializeField] private Transform visualTransform;
        [SerializeField] private float scaleRecoverySpeed = 12f;
        [SerializeField] private float dashStretchIntensity = 0.3f;
        private SquashStretch _squashStretch;

        private Animator _animator;
        private string _currentFacing = "Down";
        private string _currentAnimState;
        private Vector2 _toAim;
        public bool armor;

        private Weapon[] weapons;
        private IInteractible _nearbyInteractible;

        public IInteractible NearbyInteractible
        {
            get => _nearbyInteractible;
            set => _nearbyInteractible = value;
        }

        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();
            _knockback = new KnockbackHandler(knockbackDecay);
            _animator = GetComponentInChildren<Animator>();
            _squashStretch = new SquashStretch(visualTransform, scaleRecoverySpeed);
            currentWeapon = GetComponentInChildren<Weapon>();
            weapons = GetComponentsInChildren<Weapon>(true);
        }

        public void EquipWeapon(EquippedWeapon weapon)
        {
            Weapon target = Array.Find(weapons, w => w.Type == weapon);
            if (target == null)
            {
                Debug.LogError($"EquipWeapon: aucune Weapon enfant taguée {weapon}.", this);
                return;
            }

            if (currentWeapon)
            {
                if (State == PlayerState.Attacking)
                    CancelAttack();
                currentWeapon.gameObject.SetActive(false);
            }
            target.gameObject.SetActive(true);
            currentWeapon = target;
            
        }

        public void ApplyKnockback(Vector2 knockback)
        {
            _knockback.Apply(knockback);
        }

        public void Squash(Vector2 axis, float intensity)
        {
            _squashStretch.Squash(axis, intensity);
        }

        private void OnEnable()
        {
            _inputReader.DashRequested += OnDashRequested;
            _inputReader.AttackRequested += OnAttackRequested;
            _inputReader.InteractRequested += OnInteractRequested;
        }

        private void OnDisable()
        {
            _inputReader.DashRequested -= OnDashRequested;
            _inputReader.AttackRequested -= OnAttackRequested;
            _inputReader.InteractRequested -= OnInteractRequested;
        }

        private void OnDashRequested()
        {
            _dashBuffer.Request();
        }

        private void OnAttackRequested()
        {
            _attackBuffer.Request();
        }

        private void OnInteractRequested()
        {
            if (_nearbyInteractible==null) return;
            _nearbyInteractible.Interact();
        }

        private void Update()
        {
            Move();
            LookAt();
            TryConsumeDashBuffer();
            TryConsumeAttackBuffer();
            UpdateFacing();
            transform.position += (Vector3)_knockback.Tick(Time.deltaTime);
            _squashStretch.Tick(Time.deltaTime);
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
                    //speed = playerSpeed * dashSpeedModifier; DashControl
                    return;
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

            _toAim = (Vector2)(_inputReader.AimPosition - weaponArm.position);
            if (_toAim.sqrMagnitude < 0.0001f) return; // cursor right on top of the player: no meaningful direction, keep current rotation

            IsFacingLeft = _toAim.x < 0f;


            var angle = Vector2.SignedAngle(Vector2.down, _toAim.normalized);
            weaponArm.eulerAngles = new Vector3(0,0,Mathf.MoveTowardsAngle(weaponArm.eulerAngles.z, angle, rotationSpeed * Time.deltaTime));
        }

        private static readonly (string Name, float Angle)[] FacingDirections =
        {
            ("Up", 0f),
            ("Up_Right", -45f),
            ("Down_Right", -135f),
            ("Down", 180f),
            ("Down_Left", 135f),
            ("Up_Left", 45f),
        };

        private void UpdateFacing()
        {
            // Freeze facing during attack, matching the weapon aim freeze above
            if (State != PlayerState.Attacking && State != PlayerState.Dashing)
            {
                _currentFacing = ClosestFacing(_toAim).Name;
            }

            //Add Attack State
            string motion = State == PlayerState.Dashing ? "Dash" : _inputReader.MoveDirection != Vector2.zero ? "Walk" : "Idle";
            string state = $"Player_{motion}_{_currentFacing}";

            if (state == _currentAnimState) return; // Play() restarts the clip, so only call it on an actual change
            _currentAnimState = state;
            _animator.Play(state);
        }

        private static (string Name, float Angle) ClosestFacing(Vector2 direction)
        {
            if (direction == Vector2.zero) return FacingDirections[3]; // "Down": no input, default to down
            if (direction.y == 0f) return direction.x > 0f ? FacingDirections[2] : FacingDirections[4]; // no pure left/right sprite: fall back to down

            return ClosestFacing(Vector2.SignedAngle(Vector2.up, direction));
        }

        private static (string Name, float Angle) ClosestFacing(float angle)
        {
            var closest = FacingDirections[0];
            float smallestDiff = float.MaxValue;

            foreach (var candidate in FacingDirections)
            {
                float diff = Mathf.Abs(Mathf.DeltaAngle(angle, candidate.Angle)); // DeltaAngle avoids the 0/360 wraparound
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    closest = candidate;
                }
            }

            return closest;
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
                _squashStretch.Stretch(dashDirection, dashStretchIntensity);

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
            if (!currentWeapon) return;
            if (!_attackBuffer.HasFreshRequest() || State == PlayerState.Attacking || State == PlayerState.Dashing ||
                !currentWeapon.GetCooldown().IsReady()) return;

            float mirrorSign = AimMirrorSign();
            _activeAttackCoroutine = StartCoroutine(AttackCoroutine());

            IEnumerator AttackCoroutine()
            {
                State = PlayerState.Attacking;
                yield return currentWeapon.Attack(mirrorSign);
                FinishAttack();
            }
        }

        // > 0 right, < 0 left
        private float AimMirrorSign() => IsFacingLeft ? -1f : 1f;

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
