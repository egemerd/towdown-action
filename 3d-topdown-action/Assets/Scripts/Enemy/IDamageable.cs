using UnityEngine;
using System;



public interface IDamageable 
{
    bool IsAlive { get; }  

    void TakeDamage(HitInfo info);
    
    event Action<HitInfo> OnDamageTaken;

    event Action OnDied;
}
