using System.Collections;
using Managers;
using UnityEngine;

namespace Scriptables.Weapons
{
    [CreateAssetMenu(fileName = "HammerData", menuName = "Scriptable Objects/Weapons/HammerData")]
    public class HammerData : WeaponData
    {
        [Header("Squash")]
        [SerializeField] private float squashIntensity = 0.3f;
        [SerializeField] private float scaleRecoverySpeed = 12f;

        protected override IEnumerator PlayActivePhase(int index, Transform weaponTransform, Transform visualTransform, Transform shadowTransform, Collider2D hitbox, float mirrorSign)
        {
            AttackAnims anim = CurrentAnim(index);
            var squashFx = new SquashStretch(visualTransform, scaleRecoverySpeed);
            bool impactTriggered = false;
            Vector3 defaultPos = weaponTransform.localPosition;
            hitbox.enabled = false;

            var elapsedTime = 0f;
            while (elapsedTime < anim.duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / anim.duration);

                weaponTransform.localEulerAngles = new Vector3(0, 0, EvaluateAngle(anim, t, mirrorSign));
                weaponTransform.localPosition = EvaluatePosition(anim, t, defaultPos, mirrorSign);
                if (shadowTransform != null) shadowTransform.localScale = Vector3.one * EvaluateShadowScale(anim, t);

                bool inWindow = IsInImpactWindow(anim, elapsedTime);
                hitbox.enabled = inWindow;

                if (inWindow && !impactTriggered)
                {
                    impactTriggered = true;
                    squashFx.Squash(Vector2.left, squashIntensity);
                    // TODO: déclencher la particule d'impact au sol ici, à la position de weaponTransform
                }

                squashFx.Tick(Time.deltaTime);

                yield return null;
            }

            weaponTransform.localEulerAngles = new Vector3(0, 0, EvaluateAngle(anim, 1f, mirrorSign));
            weaponTransform.localPosition = EvaluatePosition(anim, 1f, defaultPos, mirrorSign);
            if (shadowTransform != null) shadowTransform.localScale = Vector3.one * EvaluateShadowScale(anim, 1f);
            hitbox.enabled = false;
        }
    }
}
