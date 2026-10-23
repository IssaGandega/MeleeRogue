using System.Collections;
using Controller;
using Interfaces;
using Scriptables.Weapons;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private Transform visualTransform;
        [SerializeField] private Vector3 gripRestPosition;
        [SerializeField] private Vector2 visualHeightOffset;
        [SerializeField] private Transform shadowTransform;
        [SerializeField] private bool overlaps;
        [SerializeField] private PlayerController.EquippedWeapon weaponType;
        


        private Collider2D _hitbox;
        private SpriteRenderer _spriteRenderer;
        private PlayerController _playerController;

        public PlayerController.EquippedWeapon Type => weaponType;

        // Index dans weaponData.attackAnims, avance d'un coup à chaque attaque et boucle (voir Attack()
        // ci-dessous / WeaponData.CurrentAnim). Une seule entrée dans attackAnims = reste à 0, comme avant.
        private int _comboIndex;

        // Deadline (Time.time) pour enchaîner le prochain coup sans reset le combo - posée à la fin de
        // chaque attaque (ComboBuffer secondes après). Démarre à 0 : le tout premier coup, Time.time (> 0
        // dès la 1ère frame) est forcément après, donc _comboIndex part bien à 0.
        private float _comboDeadline;

        private Cooldown _cooldown;

        // Rest pose, captured once in Awake
        private float _restAngle;
        private Vector3 _restPos;
        private Vector3 _restScale;

        private Coroutine _activeAttack;

        private void Awake()
        {
            _cooldown = new Cooldown(weaponData.AttackCooldown);
            _hitbox = GetComponent<Collider2D>();
            _spriteRenderer = visualTransform != null ? visualTransform.GetComponent<SpriteRenderer>() : null;
            _playerController = GetComponentInParent<PlayerController>();

            var rb = GetComponent<Rigidbody2D>();
            if (rb.bodyType != RigidbodyType2D.Kinematic)
            {
                Debug.LogWarning($"{name}: Weapon's Rigidbody2D wasn't Kinematic, forcing it.", this);
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            _restAngle = transform.localEulerAngles.z;
            _restPos = transform.localPosition;
            _restScale = visualTransform != null ? visualTransform.localScale : Vector3.one;

            if (shadowTransform != null)
            {
                shadowTransform.gameObject.SetActive(true);
                shadowTransform.localScale = Vector3.zero;
            }
        }

        private void Update()
        {
            ApplyVisualHeightOffset();
            ApplySpriteFlip();
        }

        private void ApplySpriteFlip()
        {
            if (_spriteRenderer == null || _playerController == null) return;
            _spriteRenderer.flipX = _playerController.IsFacingLeft;
            if (overlaps)
            {
                _spriteRenderer.sortingOrder = _playerController.IsFacingLeft ? 4 : 2;
            }

        }


        private void OnValidate()
        {
            ApplyVisualHeightOffset();
        }

        private void ApplyVisualHeightOffset()
        {
            if (visualTransform == null) return;

            float parentAngle = transform.parent != null ? transform.parent.eulerAngles.z : 0f;
            Vector3 counterRotatedOffset = Quaternion.Euler(0, 0, -parentAngle) * (Vector3)visualHeightOffset;

            visualTransform.localPosition = gripRestPosition + counterRotatedOffset;
        }

        public IEnumerator Attack(float mirrorSign)
        {
            if (Time.time > _comboDeadline) _comboIndex = 0; // trop tard : on recommence le combo au début

            if (weaponData.HyperArmor) { _playerController.armor = true; }
            int index = _comboIndex;
            _comboIndex = weaponData.AttacksCount > 0 ? (_comboIndex + 1) % weaponData.AttacksCount : 0;

            _activeAttack = StartCoroutine(weaponData.PlayHit(index, transform, visualTransform, shadowTransform, _hitbox, mirrorSign));
            yield return _activeAttack;
            _activeAttack = null;
            if (weaponData.HyperArmor) { _playerController.armor = false; }
            _comboDeadline = Time.time + weaponData.ComboBuffer; // fenêtre pour enchaîner, ouverte à la fin du coup
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
            weaponData.ForceIdle(transform, visualTransform, _restAngle, _restPos, _restScale);
            if (weaponData.HyperArmor) { _playerController.armor = false; }
            if (shadowTransform != null) shadowTransform.localScale = Vector3.zero;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((targetLayers.value & (1 << collision.gameObject.layer)) != 0)
            {
                Vector2 knockback = (Vector2)(collision.transform.position - transform.position).normalized
                                     * weaponData.KnockbackForce;
                collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(weaponData.Damage, knockback, weaponData.HitWeight);
            }
        }

        public Cooldown GetCooldown()
        {
            return _cooldown;
        }
    }
}
