using System;
using Controller;
using Scriptables.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class CameraManager : MonoBehaviour
    {
        
        [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
        [SerializeField] private float smoothTime = 0.25f;
        [SerializeField] private HitEventChannel hitEventChannel;
        private readonly CameraShake _shake =  new CameraShake();
        
        private void OnEnable()
        {
            hitEventChannel.OnRaised += OnHandleHit;
        }

        private void OnDisable()
        {
            hitEventChannel.OnRaised -= OnHandleHit;
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
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                _player.transform.position + offset, 
                ref _currentVelocity, 
                smoothTime
            );
        }
    }
}
