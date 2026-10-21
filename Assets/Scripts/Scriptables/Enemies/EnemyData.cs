using UnityEngine;

namespace Enemies
{
    public abstract class EnemyData : ScriptableObject
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private float moveSpeed;

        // When true, this enemy still takes damage but never staggers
        [SerializeField] private bool hyperArmor;

        // Passive body contact with the player (not the lunge's own ContactDamage on PursuerData)
        [SerializeField] private int bodyContactDamage;
        [SerializeField] private float bodyContactKnockback;
        [SerializeField] private float hitWeight;

        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public bool HyperArmor => hyperArmor;
        public int BodyContactDamage => bodyContactDamage;
        public float BodyContactKnockback => bodyContactKnockback;
        public float HitWeight => hitWeight;
    }
}
