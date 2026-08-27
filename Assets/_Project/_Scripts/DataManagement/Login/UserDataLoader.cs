using UnityEngine;
using System;
using Shield_Shot.DataManagement;
using Shield_Shot.NetworkCore;

namespace Shield_Shot.DataManagement.Login
{
    public class UserDataLoader
    {
        public void Load()
        {
            BackendGameData.Instance.GameDataGet();

            UserData serverData =
                BackendGameData.userData;

            if (serverData == null)
            {
                throw new InvalidOperationException(
                    "서버 유저 데이터를 불러오지 못했습니다.");
            }

            PlayerDataManager player =
                PlayerDataManager.Instance;

            if (player == null)
            {
                throw new InvalidOperationException(
                    "PlayerDataManager.Instance가 없습니다.");
            }

            player.gold = serverData.gold;
            player.diamond = serverData.diamond;
            player.clearStageStep =
                serverData.clearStageStep;
            player.profileId = serverData.profileId;
            player.frameId = serverData.frameId;
            player.highestWave =  serverData.highestWave;
        }
    }
}
