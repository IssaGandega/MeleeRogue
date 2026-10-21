using Scriptables.Events;
using UnityEngine;

namespace Managers
{
    public class CameraShake
    {
        private readonly float recoverySpeed;
        private readonly Vector2 maximumTranslationShake;
        private readonly float frequency;


        public CameraShake(float frequency,  Vector2 maximumTranslationShake, float recoverySpeed)
        {
            this.frequency = frequency;
            this.maximumTranslationShake = maximumTranslationShake;
            this.recoverySpeed = recoverySpeed;
        }
        
        private float seed;
        public float trauma = 0;


        public void Awake()
        {
            seed = Random.value;
        }
        
        public void AddTrauma(float stress)
        {
            trauma = Mathf.Clamp01(trauma + stress);
        }
        
        public Vector3 Shake()
        {
            return new Vector2(
                (Mathf.PerlinNoise(seed, Time.unscaledTime * frequency)) * .2f - .1f,
                (Mathf.PerlinNoise(seed + 1, Time.unscaledTime * frequency)) * .2f - .1f
            ) * trauma * maximumTranslationShake;
        }
        
        public void Tick(float deltaTime)
        {
            trauma = Mathf.Clamp01(trauma - recoverySpeed * deltaTime);
        }
        
    }
}
