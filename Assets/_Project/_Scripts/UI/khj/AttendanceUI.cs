using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RewardData
{
    public int _day;
    public Sprite _icon;
    public string _itemName;
}

public class AttendanceUI : MonoBehaviour
{
    [SerializeField] private AttendanceManager _manager;
    [SerializeField] private List<RewardData> _rewards;
    [SerializeField] private Transform _gridParent;
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private GameObject _dailyPopup;

    void Start()
    {
        if(_manager.CheckReceiveReward())
        {
            _dailyPopup.SetActive(true);
            RefreshAttendnaceUI();
        }
        else
        {
            _dailyPopup.SetActive(false);
        }
    }

    private void RefreshAttendnaceUI()
    {
        foreach(var reward in _rewards)
        {
            GameObject slot = Instantiate(_slotPrefab, _gridParent);
            //TODO: 슬롯 내부 이미지를 리워드 데이터로 세팅
        }
    }

    public void OnclickClaim()
    {
        _manager.ClaimReward();
        _dailyPopup.SetActive(false);
        Debug.Log("보상 수령 완료");
    }
}
