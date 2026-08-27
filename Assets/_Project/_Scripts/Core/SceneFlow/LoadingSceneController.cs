using ExitGames.Client.Photon.StructWrapping;
using Shield_Shot.Core.SceneFlow;
using Shield_Shot.DataManagement;
using Shield_Shot.NetworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Shield_Shot.Core.SceneFlow
{
    public sealed class LoadingSceneController : BaseSceneController
    {
        protected override void OnEnterScene(SceneTransitionData transitionData)
        {
            int biomInt = 0;
            int biomeStageIndex = 0;

            transitionData.TryGet("SelectedBiom", out biomInt);
            transitionData.TryGet("BiomeStageIndex", out biomeStageIndex);

            StartCoroutine(DelayedLoadInGame(biomInt, biomeStageIndex));
        }


        private IEnumerator DelayedLoadInGame(int biomInt, int biomeStageIndex)
        {
            yield return new WaitForSeconds(1f);

            int maxClearStageId = PlayerDataManager.Instance.clearStageStep;
            int targetStageId = ((biomInt - 1) * 30) + biomeStageIndex;

            Debug.Log($"[해금 검증] 유저 최고 스테이지 ID: {maxClearStageId}, 입장 시도 ID: {targetStageId}");

            if(targetStageId <= maxClearStageId + 1)
            {
                var nextData = new SceneTransitionData(SceneType.Loading, SceneType.InGame, SceneTransitionReason.LobbyToInGame);

                // 데이터를 인게임 씬으로 다시 전달
                nextData.Set("SelectedBiom", biomInt);
                nextData.Set("BiomeStageIndex", biomeStageIndex);

                SceneFlowManager.Instance.LoadScene("04_InGame", nextData);
            }
            else
            {
                Debug.LogError("아직 잠긴 스테이지입니다!");
                SceneFlowManager.Instance.LoadScene("03_Lobby", null);
            }
        }
    }
}

