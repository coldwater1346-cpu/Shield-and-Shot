using UnityEngine;
namespace Shield_Shot.InputSystem.Data
{
    public enum WeaponType
    {
        None,
        Bow,  // 활: 드래그 후 대기 시 차징
        Rifle,   // 총: 그냥 꾹 누르면 차징 (드래그 무시)
        Sniper,
        Shotgun,
        Laser,
        Shield
    }
}

