using UnityEngine;


namespace Shield_Shot.DataManagement.InventorySystem
{
    [CreateAssetMenu(fileName = "ShieldItemData", menuName = "Scriptable Objects/ItemData/ShieldItemData")]
    public class ShieldItemData : ItemData
    {

        // [Header("Shield Stat")]



        [Header("Prefab")]

        public GameObject ShieldPrefab;

        // public GameObject AimLinePrefab;
        // public GameObject FireVFX;
        // public AudioClip FireSFX;
    }
}