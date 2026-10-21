using UnityEngine;

namespace Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(int damage, Vector2 knockback, float weight);
    }
}
