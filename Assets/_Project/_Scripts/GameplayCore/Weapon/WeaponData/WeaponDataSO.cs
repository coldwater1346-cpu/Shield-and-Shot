using Shield_Shot.InputSystem.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "WeaponSystem/WeaponData")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Weapon Identity")]
    public string WeaponName;
    public WeaponType Type;
    public GameObject WeaponPrefab;

    [Header("Weapon General Stats")]
    [Tooltip("발사 후 재입력 불가 쿨타임 (초). 스나이퍼는 재장전 시간.")]
    public float FireRate = 0.5f;

    [Header("Critical Settings")]
    [Tooltip("크리티컬 발생 시 데미지 배율")]
    public float CriticalDamageMultiplier = 2.0f;

    [Header("Weapon Projectile Base Stats")]
    public float BaseDamage = 10f;
    public float BaseSpeed = 15f;
    public float BaseScale = 1f;

    [Header("Charge Multipliers (at 100% charge)")]
    [Tooltip("차징 100%시 데미지 배율")]
    public float MaxDamageMultiplier = 2.5f;

    [Tooltip("차징 100%시 속도 배율")]
    public float MaxSpeedMultiplier = 1.5f;
}