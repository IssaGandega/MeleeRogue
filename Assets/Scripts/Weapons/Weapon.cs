using System;
using System.Collections;
using Controller;
using Data.Weapons;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D))]
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private LayerMask targetLayers;

        // Drag the hand (e.g. RightHand) here in the Inspector: lets weapon data move the
        // hand itself (lunge, recoil...) independently from the blade's own rotation.
        // Optional: leave empty and weapons simply skip any hand animation.
        [SerializeField] private Transform handTransform;

        private Collider2D _hitbox;

        // TODO(combo system): never incremented/reset yet, currently always 0.
        // Will pick which hit of the combo PlayHit plays once combos exist.
        private int _comboIndex;

        // private float _timeStamp; // TODO(combo system): unused for now, was scaffolding for combo-window timing.

        private Cooldown _cooldown;

        private void Awake()
        {
            _cooldown = new Cooldown(weaponData.AttackCooldown);
            _hitbox = GetComponent<Collider2D>();
        }

        public IEnumerator Attack()
        {
            yield return StartCoroutine(weaponData.PlayHit(_comboIndex, transform, handTransform, _hitbox));
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((targetLayers.value & (1 << collision.gameObject.layer)) != 0)
            {
                Debug.Log(gameObject.name + " collided with " + collision.gameObject.name);
                collision.gameObject.GetComponent<IDamageable>()?.TakeDamage();
            }
        }
        
        public Cooldown GetCooldown()
        {
            return _cooldown;
        }
    }
}
