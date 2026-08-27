using BackEnd;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Stage;
using Shield_Shot.GameplayCore.Monster.Wave;
using System.Collections.Generic;
using UnityEngine;



namespace Shield_Shot.DataManagement.DataParsing
{
    public class StageDataParsingManager : MonoBehaviour
    {
        public static StageDataParsingManager Instance { get; private set; }


        //웨이브 테이블 딕셔너리
        public Dictionary<string, WaveSO> WaveTable = new Dictionary<string, WaveSO>();

        // 최종 스테이지 테이블 딕셔너리
        public Dictionary<string, StageSO> StageTable { get; private set; } = new Dictionary<string, StageSO>();

        //  디버그용 
        [SerializeField] private List<WaveSO> debugWaveList = new List<WaveSO>();
        // 디버그용
        [SerializeField] private List<StageSO> debugStageList = new List<StageSO>();

        public class TempSpawnGroup
        {
            public string MonsterId;
            public float Weight;
            public List<OptionalMovementBehavior> Movements = new List<OptionalMovementBehavior>();
            public List<BtTraitSO> BehaviorTraits = new List<BtTraitSO>();
            public List<BtTraitSO> DeathTraits = new List<BtTraitSO>();
        }

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
        /// Wave_Master 차트를 읽어 기본 WaveSO  생성하고 딕셔너리에 등록
        /// </summary>
        private void ParseWaveMaster(string masterChartId)
        {
            // 뒤끝 서버에서 WaveData차트 가져오기
            var bro = Backend.Chart.GetChartContents(masterChartId);
            if (!bro.IsSuccess())
            {
                Debug.LogError($"[Master] 웨이브 마스터 차트 로드 실패: {bro.GetErrorCode()}");
                return;
            }


            foreach (LitJson.JsonData row in bro.FlattenRows())
            {
                // 1. 엑셀에서 값 추출하기
                string waveId = row["WaveID"].ToString();
                float spawnInterval = float.Parse(row["SpawnInterval"].ToString());
                int totalSpawnCount = int.Parse(row["TotalSpawnCount"].ToString());
                int minPerSpawn = int.Parse(row["MinPerSpawn"].ToString());
                int maxPerSpawn = int.Parse(row["MaxPerSpawn"].ToString());

                // 뒤끝 토글(bool) 데이터는 문자열 "true"/"false"로 올 수 있으므로 변환 처리
                bool isBossWave = bool.Parse(row["IsBossWave"].ToString().ToLower());

                // 2.  WaveSO 에셋 인스턴스 동적 생성
                WaveSO newWaveSO = ScriptableObject.CreateInstance<WaveSO>();

                // 3. WaveSO에 InitializeMaster 메서드로 기본 스탯 주입
                newWaveSO.InitializeMaster(spawnInterval, totalSpawnCount, minPerSpawn, maxPerSpawn, isBossWave);

                // 4.  WaveTable 딕셔너리에 [WaveID : ] 형태로 먼저 등록
                if (!WaveTable.ContainsKey(waveId))
                {
                    WaveTable.Add(waveId, newWaveSO);
                    Debug.Log($"[웨이브 테이블 생성] ID: {waveId} (BossWave: {isBossWave})");
                }
            }

            Debug.Log($" {WaveTable.Count}개의 WaveSO  생성 성공.");
        }
        /// <summary>
        /// Wave_SpawnGroups 서브 차트를 읽어와 기존 WaveTable의 방에 몬스터 및 복수 이동 패턴을 누적합니다.
        /// </summary>
        private void ParseWaveSpawnGroups(string spawnGroupChartId)
        {
            var bro = Backend.Chart.GetChartContents(spawnGroupChartId);
            if (!bro.IsSuccess())
            {
                Debug.LogError($"[SpawnGroups] 스폰 그룹 차트 로드 실패: {bro.GetErrorCode()}");
                return;
            }

            // [WaveID : 임시 조립 뭉치 리스트]  생성
            Dictionary<string, List<TempSpawnGroup>> assembleMap = new Dictionary<string, List<TempSpawnGroup>>();

            
            foreach (LitJson.JsonData row in bro.FlattenRows())
            {
                string waveId = row["WaveID"].ToString();
                string monsterId = row["MonsterID"].ToString();

                
                if (!WaveTable.ContainsKey(waveId))
                    continue;

                //   WaveID 이 없다면 새로 생성
                if (!assembleMap.ContainsKey(waveId))
                    assembleMap.Add(waveId, new List<TempSpawnGroup>());

                List<TempSpawnGroup> tempList = assembleMap[waveId];

                //  웨이브 리스트에 '동일한 몬스터'가 누적되고 있는지 검색
                TempSpawnGroup match = tempList.Find(x => x.MonsterId == monsterId);

                // 이동 패턴과 확률 
                string movementPath = row["Movement"].ToString();
                float probability = float.Parse(row["Probability"].ToString());

                ScriptableObject moveSO = Resources.Load<ScriptableObject>(movementPath);
                OptionalMovementBehavior newMoveBehavior = new OptionalMovementBehavior(moveSO, probability);

               
                if (match == null)
                {
                    match = new TempSpawnGroup { MonsterId = monsterId };
                    match.Weight = float.Parse(row["Weight"].ToString());

                    // 특성 파싱
                    string bTraitsStr = row.Keys.Contains("_behaviorTraits") ? row["_behaviorTraits"].ToString() : "";
                    string dTraitsStr = row.Keys.Contains("_deathTraits") ? row["_deathTraits"].ToString() : "";

                    if (!string.IsNullOrEmpty(bTraitsStr))
                    {
                        foreach (var t in bTraitsStr.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                        {
                            BtTraitSO traitSO = Resources.Load<BtTraitSO>(t.Trim());
                            if (traitSO != null) match.BehaviorTraits.Add(traitSO);
                        }
                    }

                    if (!string.IsNullOrEmpty(dTraitsStr))
                    {
                        foreach (var t in dTraitsStr.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                        {
                            BtTraitSO traitSO = Resources.Load<BtTraitSO>(t.Trim());
                            if (traitSO != null) match.DeathTraits.Add(traitSO);
                        }
                    }

                    // 임시 리스트에 등록
                    tempList.Add(match);
                }

                // 이동 패턴을 리스트에  누적
                match.Movements.Add(newMoveBehavior);
            }

            // 3. 임시 클래스들을 SpawnGroup 구조체로 변환 후 WaveSO에 주입
            foreach (var pair in assembleMap)
            {
                string waveId = pair.Key;
                List<SpawnGroup> finalStructList = new List<SpawnGroup>();

                foreach (TempSpawnGroup temp in pair.Value)
                {
                    //  MonsterTable에서  MonsterDataSO 가져오기
                    if (MonsterDataParsingManager.Instance.MonsterTable.TryGetValue(temp.MonsterId, out MonsterDataSO monsterData))
                    {
                        SpawnGroup realStruct = new SpawnGroup();

                        // Initialize 함수로 데이터 주입
                        realStruct.Initialize(monsterData.MonsterPrefab, monsterData, temp.Weight, temp.Movements, temp.BehaviorTraits, temp.DeathTraits);
                        finalStructList.Add(realStruct);
                    }
                }

                // List<SpawnGroup>을 기존 WaveTable[waveId] 
                WaveTable[waveId].SetSpawnGroups(finalStructList);
                Debug.Log($"[서브 조립 완료] {waveId}번에 일반 몬스터 {finalStructList.Count}종류 누적 완료!");
            }
        }
        /// <summary>
        /// Wave_Formations 서브 차트를 읽어와 기존 WaveTable에 수량별 포메이션 규칙 누적
        /// </summary>
        private void ParseWaveFormations(string formationChartId)
        {
            // 1. 뒤끝 서버에서 차트 데이터 가져오기
            var bro = Backend.Chart.GetChartContents(formationChartId);
            if (!bro.IsSuccess())
            {
                Debug.LogError($"[Formations] 포메이션 차트 로드 실패: {bro.GetErrorCode()}");
                return;
            }

            
            foreach (LitJson.JsonData row in bro.FlattenRows())
            {
                string waveId = row["WaveID"].ToString();

                
                if (!WaveTable.ContainsKey(waveId))
                    continue;

                // 엑셀에서 추출
                int minCount = int.Parse(row["Formation_MinCount"].ToString());
                float weight = float.Parse(row["Formation_Weight"].ToString());
                string formationPath = row["Formation"].ToString();

                // Resources 폴더에서 포메이션 SO 찾기
                ScriptableObject formationSO = Resources.Load<ScriptableObject>(formationPath);
                if (formationSO == null)
                {
                    Debug.LogWarning($"[경고] {formationPath} 경로에 포메이션 에셋이 없습니다!");
                }

                //   생성자로 클래스 생성
                FormationRule newRule = new FormationRule(minCount, formationSO, weight);

                // 3. 딕셔너리에서 꺼내와 내부 리스트에  누적(Add)
                
                WaveTable[waveId].FormationRulesList.Add(newRule);
            }

            Debug.Log($"[서브 조립 완료] 모든 웨이브에 포메이션 규칙 누적 완료!");
        }

        /// <summary>
        /// [파트 D] Wave_BossGroups 서브 차트를 읽어와 기존 WaveTable의 방에 보스 데이터 및 공격 패턴을 주입
        /// </summary>
      
        private void ParseWaveBossGroups(string bossGroupChartId)
        {
            var bro = Backend.Chart.GetChartContents(bossGroupChartId);
            if (!bro.IsSuccess())
            {
                Debug.LogError($"[BossGroups] 보스 그룹 차트 로드 실패: {bro.GetErrorCode()}");
                return;
            }

            foreach (LitJson.JsonData row in bro.FlattenRows())
            {
                string waveId = row["WaveID"].ToString();

                
                if (!WaveTable.ContainsKey(waveId))
                    continue;

                string bossMonsterId = row["BossMonsterID"].ToString();

                //  MonsterTable에 보스의 데이터(스탯/프리팹) 조회
                if ((MonsterDataParsingManager.Instance.MonsterTable.TryGetValue(bossMonsterId, out MonsterDataSO bossMonsterData)))
                {
                    // 1. 공격 패턴 리스트
                    
                    List<BossAttackRule> patternList = new List<BossAttackRule>();
                    string attackStr = row["AttackPattern"].ToString();

                    foreach (var p in attackStr.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                    {
                        ScriptableObject attackSO = Resources.Load<ScriptableObject>(p.Trim());
                        if (attackSO != null)
                        {
                            //클래스 생성 후 대입
                            BossAttackRule newRule = new BossAttackRule();
                            newRule.attackPattern = attackSO;

                            patternList.Add(newRule);
                        }
                    }

                    // 2. 발사 간격 파싱
                    float fireInterval = float.Parse(row["FireInterval"].ToString());

                    // 3. 사망 특성 파싱 
                    List<BtTraitSO> deathTraits = new List<BtTraitSO>();
                    string dTraitsStr = row.Keys.Contains("_deathTraits") ? row["_deathTraits"].ToString() : "";
                    if (!string.IsNullOrEmpty(dTraitsStr))
                    {
                        foreach (var t in dTraitsStr.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                        {
                            BtTraitSO traitSO = Resources.Load<BtTraitSO>(t.Trim());
                            if (traitSO != null) deathTraits.Add(traitSO);
                        }
                    }

                    // 4. BossGroup 구조체 생성 및 Initialize 호출로 데이터 주입
                    BossGroup finalBossGroup = new BossGroup();
                    finalBossGroup.Initialize(bossMonsterData.MonsterPrefab, bossMonsterData, patternList, fireInterval, deathTraits);

                    // 5.보스 정보를 기존 WaveTable[waveId]에 대입
                    WaveTable[waveId].SetBossGroup(finalBossGroup);
                    Debug.Log($"[보스 조립 완료] {waveId}번에 보스 몬스터({bossMonsterId}, 패턴 {patternList.Count}개) 배치 완료!");
                }
            }

            Debug.Log("WaveTable 생성완료");
        }

        /// <summary>
        ///  뒤끝 서버에서 4개의 웨이브 관련 차트를 순서대로 로드, WaveSO로 조립
        /// <summary>
       
        public void LoadStageWaveTableFromServer(string masterId, string spawnGroupId, string formationId, string bossGroupId, string stageId)
        {
            // 1. 중복 방지
            WaveTable.Clear();
            if (StageTable == null) StageTable = new Dictionary<string, StageSO>();
            StageTable.Clear();

            Debug.Log("서버의 스테이지 및 웨이브 데이터 로드 시작...");

            // 2.  기본 뼈대 
            ParseWaveMaster(masterId);

            // 3. 일반 몬스터 및 복수 이동 패턴 누적
            ParseWaveSpawnGroups(spawnGroupId);

            // 4.수량별 포메이션 규칙 누적
            ParseWaveFormations(formationId);

            // 5.보스 몬스터 및 공격 패턴 주입
            ParseWaveBossGroups(bossGroupId);

            // 6. 조립된 WaveSO들을 엮어서 스테이지 SO 빌드
            ParseStageMaster(stageId);

            // 7. 디버그 리스트 세팅
            Debug.Log($" {WaveTable.Count}개의 WaveSO 및 {StageTable.Count}개의 StageSO 조립");

            //조립이 끝난 StageSO들을 리스트 형태로 변환해서 StageDatabase에 전달
            List < StageSO > stageList = new List<StageSO>(StageTable.Values);
            //StageDatabase.Instance.Initialize(stageList);

            // 인스펙터 디버그 모드 확인용 리스트 갱신
            debugWaveList = new List<WaveSO>(WaveTable.Values);
            debugStageList = new List<StageSO>(StageTable.Values);
        }

        /// <summary>
        ///  Stage_Master 차트를 읽어와 동일한 StageID 행들의 WaveSO들을 누적 
        /// </summary>
        private void ParseStageMaster(string stageChartId)
        {
            var bro = Backend.Chart.GetChartContents(stageChartId);
            if (!bro.IsSuccess())
            {
                Debug.LogError($"[Stage] 스테이지 마스터 차트 로드 실패: {bro.GetErrorCode()}");
                return;
            }

            StageTable.Clear();

            // 중간 조립을 위해 [StageID : WaveSO 리스트 주머니] 임시 생성
            Dictionary<string, List<WaveSO>> stageAssembleMap = new Dictionary<string, List<WaveSO>>();

            
            foreach (LitJson.JsonData row in bro.FlattenRows())
            {
               
                string stageId = row["StageID"].ToString();
                string waveId = row["WaveID"].ToString();

                // WaveTable에서  WaveSO 가져오기
                if (WaveTable.TryGetValue(waveId, out WaveSO waveSO))
                {
                   
                    if (!stageAssembleMap.ContainsKey(stageId))
                    {
                        stageAssembleMap.Add(stageId, new List<WaveSO>());
                    }

                    //  웨이브를 누적(Add)
                    stageAssembleMap[stageId].Add(waveSO);
                }
                else
                {
                    Debug.LogWarning($"{stageId}에 적힌 {waveId}는 WaveTable에 존재하지 않는 웨이브 ID입니다!");
                }
            }

            // 2. 리스트를  StageSO 에셋에  대입
            foreach (var pair in stageAssembleMap)
            {
                string stageId = pair.Key;
                List<WaveSO> finalWavesList = pair.Value; // 3개가 꽉 차 있는 리스트

                // 런타임 StageSO 에셋 동적 생성
                StageSO newStageSO = ScriptableObject.CreateInstance<StageSO>();

                // StageSO 내부에 웨이브 리스트 주입
                newStageSO.Initialize(finalWavesList);

                
                StageTable.Add(stageId, newStageSO);
                Debug.Log($"{stageId}번에 총 {finalWavesList.Count}개의 웨이브 누적");
            }

            // 디버그용 리스트 갱신
            debugStageList = new List<StageSO>(StageTable.Values);
            Debug.Log($" {StageTable.Count}개의 스테이지 빌드!");
        }

    }
}