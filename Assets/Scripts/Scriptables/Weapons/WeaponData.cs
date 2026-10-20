using System.Collections;
using UnityEngine;

namespace Data.Weapons
{
    public abstract class WeaponData : ScriptableObject
    {
        [SerializeField] private float attackCooldown;
        [SerializeField] private float comboBuffer; // TODO(combo): unused until the combo system exists.
        [SerializeField] private int damage;
        [SerializeField] private float knockbackForce;

        [SerializeField] private float returnDuration;

        public float AttackCooldown => attackCooldown;
        public float ComboBuffer => comboBuffer;
        public int Damage => damage;
        public float KnockbackForce => knockbackForce;

        public IEnumerator PlayHit(int index, Transform weaponTransform, Transform handTransform, Collider2D hitbox)
        {
            float restAngle = weaponTransform.localEulerAngles.z;
            bool animateHand = handTransform != null;
            Vector3 restHandPos = animateHand ? handTransform.localPosition : Vector3.zero;

            yield return PlayActivePhase(index, weaponTransform, handTransform, hitbox);

            float fromAngle = weaponTransform.localEulerAngles.z;
            Vector3 fromHandPos = animateHand ? handTransform.localPosition : Vector3.zero;
            yield return ReturnToIdle(weaponTransform, handTransform, fromAngle, restAngle, fromHandPos, restHandPos, animateHand);
        }

        protected abstract IEnumerator PlayActivePhase(int index, Transform weaponTransform, Transform handTransform, Collider2D hitbox);

        protected static float SmoothStep(float t) => t * t * (3f - 2f * t);

        protected IEnumerator ReturnToIdle(Transform weaponTransform, Transform handTransform,
            float fromAngle, float toAngle, Vector3 fromHandPos, Vector3 toHandPos, bool animateHand)
        {
            var elapsedTime = 0f;
            while (elapsedTime < returnDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / returnDuration);
                float smoothT = SmoothStep(t);

                float angle = Mathf.LerpAngle(fromAngle, toAngle, smoothT);
                weaponTransform.localEulerAngles = new Vector3(0, 0, angle);

                if (animateHand) handTransform.localPosition = Vector3.Lerp(fromHandPos, toHandPos, smoothT);

                yield return null;
            }
            weaponTransform.localEulerAngles = new Vector3(0, 0, toAngle);
            if (animateHand) handTransform.localPosition = toHandPos;
        }

        public void ForceIdle(Transform weaponTransform, Transform handTransform, float restAngle, Vector3 restHandPos)
        {
            weaponTransform.localEulerAngles = new Vector3(0, 0, restAngle);
            if (handTransform != null) handTransform.localPosition = restHandPos;
        }
    }
}
