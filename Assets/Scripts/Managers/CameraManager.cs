using Controller;
using Scriptables.Events;
using UnityEngine;

namespace Managers
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
        [SerializeField] private float smoothTime = 0.25f;

        [SerializeField] private float recoverySpeed;
        [SerializeField] private float frequency;
        [SerializeField] private Vector2 maximumTranslationShake;

        private HitEventChannel _hitEventChannel;
        private CameraShake _shake;

        private void Awake()
        {
            _hitEventChannel = GameManager.Instance.HitEventChannel;
            _shake = new CameraShake(frequency, maximumTranslationShake, recoverySpeed);
            _shake.Awake();
        }

        private void OnEnable()
        {
            _hitEventChannel.OnRaised += OnHandleHit;
        }

        private void OnDisable()
        {
            _hitEventChannel.OnRaised -= OnHandleHit;
        }

        private void OnHandleHit(HitInfo hitInfo)
        {
            _shake.AddTrauma(hitInfo.Magnitude);
        }

        private Vector3 _currentVelocity;
        private PlayerController _player;

        private void Start()
        {
            _player = GameManager.Instance.Player;
        }

        private void LateUpdate()
        {
            transform.position = PlayerFollow() + _shake.Shake();
            _shake.Tick(Time.unscaledDeltaTime);
        }

        private Vector3 PlayerFollow()
        {
            return Vector3.SmoothDamp(
                transform.position,
                _player.transform.position + offset,
                ref _currentVelocity,
                smoothTime
                );
        }
    }
}
