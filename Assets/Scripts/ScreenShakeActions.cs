using System;
using UnityEngine;

/// <summary>
/// This class <c>ScreenShakeActions</c> is contains the logic for when to shake the screen.
/// </summary>
public class ScreenShakeActions : MonoBehaviour
{
    private void Start()
    {
        ShootAction.OnAnyShoot += ShootAction_OnAnyShoot;
        GrenadeProjectile.OnAnyGrendeExploded += GrenadeProjectile_OnAnyGrendeExploded;
        SwordAction.OnAnySwordHit += SwordAction_OnAnySwordHit;
    }
    
    private void ShootAction_OnAnyShoot(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake();
    }

    private void GrenadeProjectile_OnAnyGrendeExploded(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(5f);
    }

    private void SwordAction_OnAnySwordHit(object sender, EventArgs e)
    {
        ScreenShake.Instance.Shake(2f);
    }
}
