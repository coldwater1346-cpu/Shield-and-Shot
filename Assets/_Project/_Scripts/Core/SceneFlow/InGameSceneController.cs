using Shield_Shot.Core.SceneFlow;
using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.GameplayCore.Monster.Pool;
using Shield_Shot.GameplayCore.Monster.Stage;
using UnityEngine;

namespace Shield_Shot.Core.SceneFlow
{
    public class InGameSceneController : BaseSceneController
    {
        protected override void OnEnterScene(SceneTransitionData transitionData)
        {
            int biomInt = 0;
            int biomeStageIndex = 0;

            // 데이터 추출
            transitionData.TryGet("SelectedBiom", out biomInt);
            transitionData.TryGet("BiomeStageIndex", out biomeStageIndex);

            ChapterBiom biom = (ChapterBiom)biomInt;

            // 위치 정보 계산 및 게임 로직 실행
            if (StageDatabase.Instance.TryGetLocation(biom, biomeStageIndex, out int chapterIndex, out int stageInChapter, out int globalIndex))
            {
                Debug.Log($"인게임 시작: {biom} 챕터, {stageInChapter} 스테이지");
                // 게임 초기화 로직...
            }

            if (StageDatabase.Instance.SelectStory(biom, biomeStageIndex))
            {
                Debug.Log($"[InGame] 스토리 선택 성공: {biom}, {biomeStageIndex}");
            }
        }
    }
}