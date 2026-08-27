using BackEnd;
using Shield_Shot.GameplayCore.Monster.Core;
using System.Collections.Generic;
using UnityEngine;


namespace Shield_Shot.DataManagement.DataParsing
{
    //가장 먼저 실행
    [DefaultExecutionOrder(-100)]
    public class MonsterDataParsingManager : MonoBehaviour
    {
      
        public static MonsterDataParsingManager Instance  {get; private set; }



        //몬스터 정보 테이블 딕셔너리
        public Dictionary<string, MonsterDataSO> MonsterTable { get; private set; } = new Dictionary<string, MonsterDataSO>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

        }

        /// <summary>
        /// 뒤끝 서버에서 몬스터 테이블 차트를 로드하고 파싱하는 메서드
        /// </summary>
        public void LoadMonsterTableFromServer(string chartId)
        {
            // 1. 기존 딕셔너리 초기화 
            MonsterTable.Clear();

            // 2. 뒤끝 서버에서 차트 데이터 가져오기
            var bro = Backend.Chart.GetChartContents(chartId);
            if (!bro.IsSuccess())
            {
                Debug.LogError($"몬스터 차트 로드 실패: {bro.GetErrorCode()} - {bro.GetMessage()}");
                return;
            }


            foreach (LitJson.JsonData row in bro.FlattenRows())
            {
                // 엑셀 시트에 적은 컬럼명 문자열 가져오기
                string monsterId = row["MonsterID"].ToString();
                float maxHP = float.Parse(row["MaxHP"].ToString());
                float moveSpeed = float.Parse(row["MoveSpeed"].ToString());
                float attackDamage = float.Parse(row["AttackDamage"].ToString());

                // 엑셀에 적어둔 프리팹 경로 
                string prefabPath = row["Prefab"].ToString();

                // 4. Resources 폴더에서 경로를 기반으로 진짜 프리팹 에셋 찾아오기
                GameObject monsterPrefab = Resources.Load<GameObject>(prefabPath);
                if (monsterPrefab == null)
                {
                    Debug.LogWarning($"[경고] {prefabPath} 경로에 프리팹이 없습니다! ID: {monsterId}");
                }

                // 5. 런타임  MonsterDataSO 인스턴스 동적 생성하기
                MonsterDataSO newMonsterData = ScriptableObject.CreateInstance<MonsterDataSO>();

                // 6. SO 내부에 파싱한 데이터 세팅하기 (앞서 만든 Initialize 메서드 호출)
                newMonsterData.Initialize(monsterPrefab, maxHP, moveSpeed, attackDamage);

                // 7. 몬스터 테이블(딕셔너리)에 [ID : 데이터SO] 형태로 넣기
                if (!MonsterTable.ContainsKey(monsterId))
                {
                    MonsterTable.Add(monsterId, newMonsterData);
                    Debug.Log($"[몬스터 등록 완료] ID: {monsterId} | HP: {maxHP} | DMG: {attackDamage}");
                }
            }

            Debug.Log($" 총 {MonsterTable.Count}개의 몬스터  데이터 테이블 로드 완료");
        }

    }
}