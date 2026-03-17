using Game.Weapons;
using UnityEngine;

public class Revolver : WeaponBase
{
    private static readonly int equipHash = Animator.StringToHash("Equip");
    private static readonly int fireHash = Animator.StringToHash("Fire");
    private static readonly int idleHash = Animator.StringToHash("Idle");

    public override void OnFireStart(in FireContext context)
    {
        weaponAnimator.Play(fireHash);
    }
}
