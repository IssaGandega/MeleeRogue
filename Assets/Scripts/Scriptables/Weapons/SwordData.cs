using System.Collections;
using UnityEngine;

namespace Scriptables.Weapons
{
    [CreateAssetMenu(fileName = "SwordData", menuName = "Scriptable Objects/Weapons/SwordData")]
    public class SwordData : WeaponData
    {
        protected override IEnumerator PlayActivePhase(int index, Transform weaponTransform, Transform visualTransform, Transform shadowTransform, Collider2D hitbox, float mirrorSign)
        {
            yield return PlayGenericActivePhase(index, weaponTransform, visualTransform, shadowTransform, hitbox, mirrorSign);
        }
    }
}
