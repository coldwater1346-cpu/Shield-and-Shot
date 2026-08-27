using Shield_Shot.DataManagement;
using UnityEngine;

public class DataTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"{PlayerIngameLoadData.MainWeaponItem?.ItemData.ItemName}");
    }
}
