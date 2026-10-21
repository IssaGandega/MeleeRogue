using UnityEngine;

namespace Controller
{
    public class Cooldown
    {
        public float Duration { get; private set; }
        
        //Set to -infinity so no matter how big the cooldown, it can never be an issue on first trigger
        private float _lastTrigger = -Mathf.Infinity;

        public Cooldown(float duration)
        {
            Duration = duration;

        }

        public bool IsReady()
        {
            return Time.time > _lastTrigger + Duration;
        }

        public void Trigger()
        {
            _lastTrigger = Time.time;
        }
        
        // public void SetDuration(float duration)
        // {
        //     Duration = duration;
        // }
    }
}
