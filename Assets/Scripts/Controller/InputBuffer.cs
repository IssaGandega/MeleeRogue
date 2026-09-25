using UnityEngine;

namespace Controller
{
    public class InputBuffer
    {
        private readonly float _bufferTime;
        private float? _requestTime;

        public InputBuffer(float bufferTime)
        {
            this._bufferTime = bufferTime;
        }
    
        public void Request()
        {
            _requestTime = Time.time;
        }
    
        public bool HasFreshRequest()
        {
            if (_requestTime == null) return false;
            if (Time.time - _requestTime > _bufferTime)
            {
                _requestTime = null;
                return false;
            }

            return true;
        }

        public void Consume()
        {
            _requestTime = null;
        }
    }
}
