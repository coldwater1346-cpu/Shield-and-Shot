using UnityEngine;
using System.Collections.Generic;
using BackEnd.BackndLitJson;

public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        // 1. 데이터가 아예 없거나 공백일 경우 처리
        if (string.IsNullOrEmpty(json) || json == "[]")
        {
            Debug.LogWarning("[JsonHelper] JSON 데이터가 비어 있습니다.");
            return new List<T>();
        }

        try
        {
            string newJson = "{ \"items\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);

            // 2. 파싱 결과가 null일 경우 대비
            return wrapper?.items ?? new List<T>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[JsonHelper] 파싱 실패: {e.Message}");
            return new List<T>();
        }
    }

    [System.Serializable]
    private class Wrapper<T> { public List<T> items; }
}
