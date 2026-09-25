using UnityEngine;

namespace Controller
{
    public class Cooldown
    {
        public float Duration { get; private set; }
        private float _lastTrigger = 0;

        public Cooldown(float duration)
        {
            this.Duration = duration;
        }

        public bool IsReady()
        {
            return Time.time > _lastTrigger + Duration;
        }

        public void Trigger()
        {
            _lastTrigger = Time.time;
        }
        
        public void SetDuration(float duration)
        {
            Duration = duration;
        }
    }
}
