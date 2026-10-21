using System;
using Interfaces;
using UnityEngine;

namespace Scriptables.Events
{
    public struct HitInfo
    {
        public float Magnitude;
        public float HitStopDuration;
        public Vector2 Position;
        public IDamageable Target;

        public HitInfo(float magnitude, Vector2 position, IDamageable target)
        {
            Magnitude = magnitude;
            Position = position;
            Target = target;
            HitStopDuration = 0;
        }
        
        public HitInfo(float magnitude, float hitStopDuration,Vector2 position, IDamageable target)
        {
            Magnitude = magnitude;
            HitStopDuration = hitStopDuration;
            Position = position;
            Target = target;
        }
    };
    
    [CreateAssetMenu(fileName = "HitEventChannel", menuName = "Events/HitEventChannel")]
    public class HitEventChannel : ScriptableObject
    {
        public event Action<HitInfo> OnRaised;
        public void Raise(HitInfo info) => OnRaised?.Invoke(info);
    }
}