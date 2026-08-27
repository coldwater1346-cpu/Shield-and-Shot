using UnityEngine;
using Shield_Shot.InputSystem.Data;


namespace Shield_Shot.DataManagement.InventorySystem
{
    [CreateAssetMenu(fileName = "WeaponItemData", menuName = "Scriptable Objects/ItemData/WeaponItemData")]
    public class WeaponItemData : ItemData
    {
        // 무기 세부 분류 추가 (Bow, Rifle)
        [Header("Weapon Type")]
        public WeaponType weaponType;

        // ItemData의 공통 정보를 상속받고 무기에 필요한 능력치를 추가로 관리

        [Header("Weapon Stat")]
     
        

       
        [Header("Prefab")]
        public GameObject WeaponPrefab; 

        [Header("Weapon Identity")]
        public string WeaponName;
        public WeaponType Type;
        

        [Header("Weapon General Stats")]
        public float FireRate = 0.5f;

        [Header("Weapon Projectile Base Stats")]
        public int BaseDamage=10;
        public float BaseSpeed = 15f;
        public float BaseScale = 1f;

        [Header("Charge Multipliers (at 100% charge)")]
        [Tooltip("차징 100%시 데미지 배율")]
        public float MaxDamageMultiplier = 2.5f;

        public float CriticalDamageMultiplier = 2.0f;

        [Tooltip("차징 100%시 속도 배율")]
        public float MaxSpeedMultiplier = 1.5f;

        //[Tooltip("차징 100%시 스케일 배율")]
        //public float MaxScaleMultiplier = 1.8f;



    }
}