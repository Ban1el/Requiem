using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount, float pushBackValue, Vector2 hitPosition);
}