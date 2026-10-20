using System.Collections;
using Controller;
using Data.Weapons;
using Interfaces;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private LayerMask targetLayers;

        // Drag the hand (e.g. RightHand) here so weapon data can animate it (lunge, recoil) separately
        // from the blade's own rotation. Optional: leave empty to skip hand animation.
        [SerializeField] private Transform handTransform;

        private Collider2D _hitbox;

        // TODO(combo): always 0 for now, will pick which hit of the combo PlayHit plays once combos exist.
        private int _comboIndex;

        private Cooldown _cooldown;

        // Rest pose, captured once in Awake before any attack touches the transform. Never changes after.
        private float _restAngle;
        private Vector3 _restHandPos;

        // Running PlayHit coroutine, if any - lets a dash cancel it cleanly.
        private Coroutine _activeAttack;

        private void Awake()
        {
            _cooldown = new Cooldown(weaponData.AttackCooldown);
            _hitbox = GetComponent<Collider2D>();

            // Must be Kinematic, not Dynamic: trigger detection needs a Rigidbody2D present, but movement
            // should stay fully script-driven. Forced here instead of trusting every weapon prefab's setup.
            var rb = GetComponent<Rigidbody2D>();
            if (rb.bodyType != RigidbodyType2D.Kinematic)
            {
                Debug.LogWarning($"{name}: Weapon's Rigidbody2D wasn't Kinematic, forcing it.", this);
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            _restAngle = transform.localEulerAngles.z;
            _restHandPos = handTransform != null ? handTransform.localPosition : Vector3.zero;
        }

        public IEnumerator Attack()
        {
            _activeAttack = StartCoroutine(weaponData.PlayHit(_comboIndex, transform, handTransform, _hitbox));
            yield return _activeAttack;
            _activeAttack = null;
        }

        // Called when a dash cancels an attack: stops the swing coroutine and snaps back to the rest pose.
        public void CancelAnim()
        {
            if (_activeAttack != null)
            {
                StopCoroutine(_activeAttack);
                _activeAttack = null;
            }

            _hitbox.enabled = false;
            weaponData.ForceIdle(transform, handTransform, _restAngle, _restHandPos);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((targetLayers.value & (1 << collision.gameObject.layer)) != 0)
            {
                // Away from the weapon's position, not the player's - ties knockback direction to the swing angle.
                Vector2 knockback = (Vector2)(collision.transform.position - transform.position).normalized
                                     * weaponData.KnockbackForce;
                collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(weaponData.Damage, knockback);
            }
        }
        
        public Cooldown GetCooldown()
        {
            return _cooldown;
        }
    }
}
