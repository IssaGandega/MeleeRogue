using System.Collections;
using UnityEngine;

namespace Scriptables.Weapons
{
    // Un swing complet : angle, translation, timing, ombre. WeaponData a un tableau de ceux-ci ;
    // quand une arme en a plusieurs, Weapon fait tourner l'index à chaque attaque (voir Weapon.Attack).
    [System.Serializable]
    public struct AttackAnims
    {
        [Header("Timing")]
        public float duration;
        public float returnDuration;

        [Header("Swing (rotation)")]
        public float startAngle;
        public float endAngle;
        public AnimationCurve angleCurve;

        [Header("Translation")]
        public Vector2 moveOffset;
        public AnimationCurve moveCurve;

        [Header("Impact window (hitbox)")]
        public float impactTime;
        public float hitboxWindow; // <= 0 : hitbox active jusqu'à la fin de duration.

        [Header("Shadow (optionnel)")]
        public AnimationCurve shadowScaleCurve;
    }

    public abstract class WeaponData : ScriptableObject
    {
        [SerializeField] private float attackCooldown;
        // ComboBufferWindow
        [SerializeField] private float comboBuffer;
        [SerializeField] private int damage;
        [SerializeField] private float knockbackForce;
        [SerializeField] private float hitWeight;
        [SerializeField] private bool hyperArmor;

        [SerializeField] private AttackAnims[] attackAnims = { DefaultAnim() };

        public float AttackCooldown => attackCooldown;
        public float ComboBuffer => comboBuffer;
        public int Damage => damage;
        public float KnockbackForce => knockbackForce;
        public float HitWeight => hitWeight;
        public int AttacksCount => attackAnims?.Length ?? 0;
        public bool HyperArmor => hyperArmor;

        private static AttackAnims DefaultAnim() => new AttackAnims
        {
            angleCurve = AnimationCurve.Linear(0, 0, 1, 1),
            moveCurve = AnimationCurve.Linear(0, 0, 1, 1),
            shadowScaleCurve = AnimationCurve.Constant(0, 1, 0)
        };

        // LoopOnAnimations
        protected AttackAnims CurrentAnim(int index)
        {
            if (attackAnims == null || attackAnims.Length == 0)
            {
                Debug.LogError($"{name}: no AttackAnims configured.", this);
                return default;
            }
            return attackAnims[index % attackAnims.Length];
        }

        public IEnumerator PlayHit(int index, Transform weaponTransform, Transform visualTransform, Transform shadowTransform, Collider2D hitbox, float mirrorSign)
        {
            float restAngle = weaponTransform.localEulerAngles.z;
            Vector3 restPos = weaponTransform.localPosition;
            Vector3 restScale = visualTransform != null ? visualTransform.localScale : Vector3.one;

            yield return PlayActivePhase(index, weaponTransform, visualTransform, shadowTransform, hitbox, mirrorSign);

            float fromAngle = weaponTransform.localEulerAngles.z;
            Vector3 fromPos = weaponTransform.localPosition;
            Vector3 fromScale = visualTransform != null ? visualTransform.localScale : Vector3.one;
            yield return ReturnToIdle(index, weaponTransform, visualTransform, fromAngle, restAngle, fromPos, restPos, fromScale, restScale);
        }

        protected abstract IEnumerator PlayActivePhase(int index, Transform weaponTransform, Transform visualTransform, Transform shadowTransform, Collider2D hitbox, float mirrorSign);

        protected static float SmoothStep(float t) => t * t * (3f - 2f * t);

        protected float EvaluateAngle(AttackAnims anim, float t, float mirrorSign) =>
            Mathf.LerpAngle(anim.startAngle * mirrorSign, anim.endAngle * mirrorSign, anim.angleCurve.Evaluate(t));

        protected Vector3 EvaluatePosition(AttackAnims anim, float t, Vector3 origin, float mirrorSign)
        {
            Vector2 mirroredOffset = new Vector2(anim.moveOffset.x * mirrorSign, anim.moveOffset.y);
            return origin + (Vector3)(mirroredOffset * anim.moveCurve.Evaluate(t));
        }

        protected float EvaluateShadowScale(AttackAnims anim, float t) => anim.shadowScaleCurve.Evaluate(t);

        protected bool IsInImpactWindow(AttackAnims anim, float elapsedTime) =>
            elapsedTime >= anim.impactTime && (anim.hitboxWindow <= 0f || elapsedTime <= anim.impactTime + anim.hitboxWindow);

        protected IEnumerator PlayGenericActivePhase(int index, Transform weaponTransform, Transform visualTransform, Transform shadowTransform, Collider2D hitbox, float mirrorSign)
        {
            AttackAnims anim = CurrentAnim(index);
            Vector3 defaultPos = weaponTransform.localPosition;

            var elapsedTime = 0f;
            while (elapsedTime < anim.duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / anim.duration);

                weaponTransform.localEulerAngles = new Vector3(0, 0, EvaluateAngle(anim, t, mirrorSign));
                weaponTransform.localPosition = EvaluatePosition(anim, t, defaultPos, mirrorSign);
                if (shadowTransform != null) shadowTransform.localScale = Vector3.one * EvaluateShadowScale(anim, t);
                hitbox.enabled = IsInImpactWindow(anim, elapsedTime);

                yield return null;
            }

            weaponTransform.localEulerAngles = new Vector3(0, 0, EvaluateAngle(anim, 1f, mirrorSign));
            weaponTransform.localPosition = EvaluatePosition(anim, 1f, defaultPos, mirrorSign);
            if (shadowTransform != null) shadowTransform.localScale = Vector3.one * EvaluateShadowScale(anim, 1f);
            hitbox.enabled = false;
        }


        protected IEnumerator ReturnToIdle(int index, Transform weaponTransform, Transform visualTransform,
            float fromAngle, float toAngle, Vector3 fromPos, Vector3 toPos, Vector3 fromScale, Vector3 toScale)
        {
            float returnDuration = CurrentAnim(index).returnDuration;

            var elapsedTime = 0f;
            while (elapsedTime < returnDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / returnDuration);
                float smoothT = SmoothStep(t);

                float angle = Mathf.LerpAngle(fromAngle, toAngle, smoothT);
                weaponTransform.localEulerAngles = new Vector3(0, 0, angle);
                weaponTransform.localPosition = Vector3.Lerp(fromPos, toPos, smoothT);
                if (visualTransform != null) visualTransform.localScale = Vector3.Lerp(fromScale, toScale, smoothT);

                yield return null;
            }
            weaponTransform.localEulerAngles = new Vector3(0, 0, toAngle);
            weaponTransform.localPosition = toPos;
            if (visualTransform != null) visualTransform.localScale = toScale;
        }

        public void ForceIdle(Transform weaponTransform, Transform visualTransform, float restAngle, Vector3 restPos, Vector3 restScale)
        {
            weaponTransform.localEulerAngles = new Vector3(0, 0, restAngle);
            weaponTransform.localPosition = restPos;
            if (visualTransform != null) visualTransform.localScale = restScale;
        }
    }
}
