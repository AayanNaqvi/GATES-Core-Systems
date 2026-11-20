using UnityEngine;

public interface IKnockbackable
{
    void KnockbackAndStun(Vector2 knockbackDir, float stunDuration);

    void StunLock();
    void StunUnlock();
}
