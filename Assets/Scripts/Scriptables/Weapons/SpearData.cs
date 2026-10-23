using System.Collections;
using UnityEngine;

namespace Scriptables.Weapons
{
    [CreateAssetMenu(fileName = "SpearData", menuName = "Scriptable Objects/Weapons/SpearData")]
    public class SpearData : WeaponData
    {
        protected override IEnumerator PlayActivePhase(int index, Transform weaponTransform, Transform visualTransform, Transform shadowTransform, Collider2D hitbox, float mirrorSign)
        {
            yield return PlayGenericActivePhase(index, weaponTransform, visualTransform, shadowTransform, hitbox, mirrorSign);
        }
    }
}
