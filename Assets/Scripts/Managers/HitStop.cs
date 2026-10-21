using System.Collections;
using Scriptables.Events;
using UnityEngine;

namespace Managers
{
    public class HitStop : MonoBehaviour
    {
        private HitEventChannel _hitEventChannel;
        private Coroutine _activeFreeze;

        private void Awake()
        {
            _hitEventChannel = GameManager.Instance.HitEventChannel;
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
