using System.Collections;
using UnityEngine;

namespace Data.Weapons
{
    [CreateAssetMenu(fileName = "SwordData", menuName = "Scriptable Objects/SwordData")]

    public class SwordData: WeaponData
    {
        [SerializeField] private float startAngle;
        [SerializeField] private float endAngle;
        [SerializeField] private float duration;
        [SerializeField] private float returnDuration;

        [Header("Hand juice")]
        [SerializeField] private Vector2 lungeDirection = Vector2.up; // local space; flip/rotate in Inspector if it lunges the wrong way
        [SerializeField] private float lungeDistance = 0.15f;

        // index: TODO(combo system) not used yet, see WeaponData.
        public override IEnumerator PlayHit(int index, Transform weaponTransform, Transform handTransform, Collider2D hitbox)
        {
            hitbox.enabled = true;
            float defaultAngle = weaponTransform.localEulerAngles.z;

            bool animateHand = handTransform != null;
            Vector3 defaultHandPos = animateHand ? handTransform.localPosition : Vector3.zero;
            Vector3 lungeOffset = animateHand ? (Vector3)lungeDirection.normalized * lungeDistance : Vector3.zero;

            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                float smoothT = t * t * (3f - 2f * t); // ease-in-out: a weighty swing instead of a robotic linear one

                float angle = Mathf.LerpAngle(startAngle, endAngle, smoothT);
                weaponTransform.localEulerAngles = new Vector3(0, 0, angle);

                if (animateHand)
                {
                    // Hand lunges out and back within the swing itself: 0 -> 1 -> 0 bell curve.
                    float lungeT = Mathf.Sin(t * Mathf.PI);
                    handTransform.localPosition = defaultHandPos + lungeOffset * lungeT;
                }

                yield return null;
            }

            weaponTransform.localEulerAngles = new Vector3(0, 0, endAngle);
            if (animateHand) handTransform.localPosition = defaultHandPos;
            hitbox.enabled = false;
            yield return Idle(weaponTransform, endAngle, defaultAngle);
        }

        private IEnumerator Idle(Transform weaponTransform, float fromAngle, float toAngle)
        {
            var elapsedTime = 0f;
            while (elapsedTime < returnDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / returnDuration);
                float smoothT = t * t * (3f - 2f * t);
                float angle = Mathf.LerpAngle(fromAngle, toAngle, smoothT);
                weaponTransform.localEulerAngles = new Vector3(0, 0, angle);
                yield return null;
            }
            weaponTransform.localEulerAngles = new Vector3(0, 0, toAngle);
        }
    }
}
