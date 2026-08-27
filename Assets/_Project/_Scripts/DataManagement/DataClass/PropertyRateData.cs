using System;
using UnityEngine;

[Serializable]
public class PropertyRateData 
{
    public ItemGradeType Grade;      // 키값으로 사용할 아이템 등급
    public float NoneRate;           // 속성 부여 실패 확률 (%)
    public float FireRate;           // 화염 속성 확률 (%)
    public float IceRate;            // 얼음 속성 확률 (%)
    public float LightningRate;      // 번개 속성 확률 (%)
    public float WindRate;           // 바람 속성 확률 (%)

    //  모든 속성의 확률을  배열로
    // 인덱스 순서 규칙: 0:None, 1:Fire, 2:Ice, 3:Lightning, 4:Wind
    public float[] Rates;

    /// <summary>
    ///  클래스가 메모리에 생성될 때배열 크기를 5로 명시 생성자
    /// </summary>
    public PropertyRateData()
    {
        Rates = new float[5];
    }


}
