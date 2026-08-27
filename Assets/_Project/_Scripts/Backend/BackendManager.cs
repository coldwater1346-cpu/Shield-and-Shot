// 뒤끝 SDK namespace 추가
using BackEnd;
using BackEnd.BackndNewtonsoft.Json.Bson;
using LitJson;
using Shield_Shot.DataManagement;
using Shield_Shot.DataManagement.InventorySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static BackEnd.Backend;

namespace Shield_Shot.NetworkCore
{
    public class BackendManager : PersistentSingleton<BackendManager>
    {      
        void Start()
        {
            
            var bro = Backend.Initialize(); // 뒤끝 초기화


            // 뒤끝 초기화에 대한 응답값
            if (bro.IsSuccess())
            {
                Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success
            }
            else
            {
                Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
            }
           
        }
        
        public void RequestStageData(string chartId, string rowInDate, System.Action<bool, string> callback)
        {
            Backend.Chart.GetChartContents(chartId, (callbackResult) =>
            {
                if (callbackResult.IsSuccess())
                {
                    string jsonResult = callbackResult.GetFlattenJSON().ToString();
                    callback(true, jsonResult);
                }
                else
                {
                    Debug.LogError($"[BackendManager] ChartData respon failed: {callbackResult.GetMessage()}");
                    callback(false, null);
                }
            });
        }
        
        public void SelectStage(int stageIndex)
        {
            Param param = new Param();
            param.Add("selectedStageIndex", stageIndex);

            Backend.GameData.Update("StageData", new Where(), param, (callback) =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("스테이지 선택 저장 완료");
                }
            });
        }


        public void GetHighestClearStage(Action<int> onComplete)
        {
            Backend.GameData.GetMyData(
                "USER_DATA",
                new Where(),
                bro =>
                {
                    if (!bro.IsSuccess())
                    {
                        Debug.LogError(bro);
                        onComplete?.Invoke(1);
                        return;
                    }

                    var rows = bro.FlattenRows();

                    int highest = 1;

                    if (rows.Count > 0)
                    {
                        if (rows[0].ContainsKey("clearStageStep"))
                        {
                            highest = int.Parse(rows[0]["clearStageStep"].ToString());
                        }
                    }
                    onComplete?.Invoke(highest);
                });
        }
    }
}
