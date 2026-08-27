using System;
using UnityEngine;

public class AttendanceManager : MonoBehaviour
{
    private const string _lastLoginKey = "LastLoginDate";
    private const string _attendanceCountKey = "AttendanceCount";


    //TODO: PlayerPrefs => SeverTime 교체
    public bool CheckReceiveReward()
    {
        string lastLogin = PlayerPrefs.GetString(_lastLoginKey, "");
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        return lastLogin != today;
    }

    public void ClaimReward()
    {
        PlayerPrefs.SetString(_lastLoginKey, DateTime.Now.ToString("yyyy-MM-dd"));

        int count = PlayerPrefs.GetInt(_attendanceCountKey, 0);
        PlayerPrefs.SetInt(_attendanceCountKey, count + 1);

        //TODO: 아이템 지급 및 DB 저장 로직
    }

    public int GetTotalAttendanceCount()
    {
        return PlayerPrefs.GetInt(_attendanceCountKey, 0);
    }

    public void TestAttendance()
    {
        PlayerPrefs.DeleteKey("LastLoginDate");
        PlayerPrefs.DeleteKey("AttendanceCount");
        Debug.Log("출석 데이터 초기화");
    }
}
