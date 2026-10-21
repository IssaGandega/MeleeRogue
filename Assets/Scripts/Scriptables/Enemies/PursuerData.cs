using Enemies;
using UnityEngine;

namespace Scriptables.Enemies
{
    [CreateAssetMenu(fileName = "PursuerData", menuName = "Scriptable Objects/Enemies/PursuerData")]
    public class PursuerData : EnemyData
    {
        [SerializeField] private int contactDamage;
        [SerializeField] private float contactKnockback;
        [SerializeField] private float attackRange;
        [SerializeField] private float attackCooldown;
        [SerializeField] private float contactRange;
        [SerializeField] private float attackWindup;
        [SerializeField] private float lungeDuration;
        [SerializeField] private float attackRecovery;
        [SerializeField] private float staggerDuration;

        public int ContactDamage => contactDamage;
        public float ContactKnockback => contactKnockback;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float ContactRange => contactRange;
        public float AttackWindup => attackWindup;
        public float LungeDuration => lungeDuration;
        public float AttackRecovery => attackRecovery;
        public float StaggerDuration => staggerDuration;
    }
}
