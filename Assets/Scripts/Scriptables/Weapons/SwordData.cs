using System.Collections;
using UnityEngine;

namespace Data.Weapons
{
    [CreateAssetMenu(fileName = "SwordData", menuName = "Scriptable Objects/Weapons/SwordData")]
    public class SwordData : WeaponData
    {
        [SerializeField] private float startAngle;
        [SerializeField] private float endAngle;
        [SerializeField] private float duration;

        [Header("HandJuice")]
        [SerializeField] private Vector2 lungeDirection = Vector2.up;
        [SerializeField] private float lungeDistance = 0.15f;

        protected override IEnumerator PlayActivePhase(int index, Transform weaponTransform, Transform handTransform, Collider2D hitbox)
        {
            hitbox.enabled = true;

            bool animateHand = handTransform != null;
            Vector3 defaultHandPos = animateHand ? handTransform.localPosition : Vector3.zero;
            Vector3 lungedHandPos = animateHand
                ? defaultHandPos + (Vector3)lungeDirection.normalized * lungeDistance
                : Vector3.zero;

            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                float smoothT = SmoothStep(t);

                float angle = Mathf.LerpAngle(startAngle, endAngle, smoothT);
                weaponTransform.localEulerAngles = new Vector3(0, 0, angle);

                // Hand lunges OUT during the swing
                if (animateHand) handTransform.localPosition = Vector3.Lerp(defaultHandPos, lungedHandPos, smoothT);

                yield return null;
            }

            weaponTransform.localEulerAngles = new Vector3(0, 0, endAngle);
            if (animateHand) handTransform.localPosition = lungedHandPos;
            hitbox.enabled = false;
        }
    }
}
