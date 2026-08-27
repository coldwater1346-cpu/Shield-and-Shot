//using UnityEngine;
//using System;
//using System.Collections.Generic;


//namespace Shield_Shot.DataManagement.Json
//{
//    // JSON에 하나의 아이템 정보 저장 클래스
//    [System.Serializable]
//    public class SaveItemInfo
//    {
//        public string itemId;     // 원본 데이터 ID (예: "WP_01")
//        public string uniqueId;     // 인스턴스 고유 ID (예: GUID 문자열)
//        public int enhanceLevel;    // 현재 강화 수치 (예: 3)
//        public bool isEquipped;
//    }

//    [System.Serializable]
//    public class TotalUserData
//    {
//        public int gold = 0;
//        public int diamond = 0;
//        public int currentStage = 1;

//        // SaveItemInfo 객체의 리스트를 저장
//        public List<SaveItemInfo> ownedItems = new List<SaveItemInfo>();
//    }
//}