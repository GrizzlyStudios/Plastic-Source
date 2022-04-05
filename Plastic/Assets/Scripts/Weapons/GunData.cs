using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Weapon/Gun")]
public class GunData : ScriptableObject
{
    [Header("Info")]
    public new string name;

    [Header("Shooting")]
    public int damage;
    public float maxDistance;
    public float fireRate;
    public bool automatic;

    [Header("Reloading")]
    public int magSize;
    public float reloadTime;
    public int maxReserveAmmo;

    [Header("Aiming")]
    public float ironAimInTime;
    public float aimOutTime;
    public float weaponFOV;
    public float aimWeaponFOV;
    public float aimFOV;

    [Header("Animation")]
    public Animator animator;

    [Header("Recoil")]
    public float recoilX;
    public float recoilY;
    public float snappiness;
    public float returnSpeed;
    public float recoilCounterSpeed;
}
