using System.Collections;
using UnityEngine;

namespace Data.Weapons
{
    public abstract class WeaponData : ScriptableObject
    {
        [SerializeField] private float attackCooldown;
        [SerializeField] private float comboBuffer;

        public float AttackCooldown => attackCooldown;
        public float ComboBuffer => comboBuffer;

        // index: TODO(combo system) not used by any PlayHit implementation yet; will select which hit of the combo to play.
        // handTransform: the hand/arm anchor to move for weapon-specific "juice" (lunge, recoil...), separate from
        // weaponTransform which only carries the blade's own rotation. Can be null if a weapon doesn't animate the hand.
        public abstract IEnumerator PlayHit(int index, Transform weaponTransform, Transform handTransform, Collider2D hitbox);
    }
}
