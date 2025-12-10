using UnityEngine;

public interface IKnockbackable
{
    // Any class implementing this must define this function
    void StartKnockback(Vector2 velocity, float duration);
}