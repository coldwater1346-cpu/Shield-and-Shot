
using UnityEngine;
using Shield_Shot.DataManagement.InventorySystem;


namespace Shield_Shot.DataManagement.PlayerData
{
    public static class PlayerLoadData
    {
        public static Item MainWeaponItem;
        public static Item SubWeaponItem;
        public static Item ShieldItem;



        public static void ClearMainWeapon()
        {
            MainWeaponItem = null;
        }

        public static void ClearSubWeapon()
        {
            SubWeaponItem = null;
        }

        public static void ClearShield()
        {
            ShieldItem = null;
        }
    }
}