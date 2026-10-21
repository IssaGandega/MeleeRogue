using System;
using System.Collections;
using Scriptables.Events;
using UnityEngine;

namespace Managers
{
    public class HitStop : MonoBehaviour
    {
        
        [SerializeField] private HitEventChannel hitEventChannel;
        private Coroutine _activeFreeze;
        
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
            if (_activeFreeze != null)
                StopCoroutine(_activeFreeze);
            _activeFreeze = StartCoroutine(StopTimer());
            
            IEnumerator StopTimer()
            {
                var duration = hitInfo.Magnitude * hitInfo.HitStopDuration;
                Time.timeScale = 0;
                yield return new WaitForSecondsRealtime(duration);
                Time.timeScale = 1;
            }
        }
    }
}
