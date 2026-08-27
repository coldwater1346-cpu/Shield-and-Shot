using Shield_Shot.DataManagement.InventorySystem;
using UnityEngine;


namespace Shield_Shot.DataManagement.Login
{
    public class InventoryInitializer
    {
        public void Initialize()
        {
            if (InventoryManager.Instance == null)
                throw new System.Exception("InventoryManager.Instance가 없습니다.");

            InventoryManager.Instance.InitInventoryData();
        }
    }
}