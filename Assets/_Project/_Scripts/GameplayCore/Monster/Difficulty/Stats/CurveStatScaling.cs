using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 커브 기반 배율 스케일링:  최종값 = base × curve(difficulty)
    ///
    /// 가장 직관적인 형태. 디자이너가 그래프로 난이도 곡선을 직접 그린다.
    /// 예) 0에서 1배, 100에서 3배로 오르는 완만한 S커브.
    /// </summary>
    [CreateAssetMenu(fileName = "CurveScaling", menuName = "Monster/Difficulty/Scaling/Curve")]
    public class CurveStatScaling : StatScalingSO
    {
        [Header("난이도(x) → 배율(y)")]
        [SerializeField] private AnimationCurve _multiplier = AnimationCurve.Linear(0f, 1f, 100f, 3f);

        [Tooltip("배율이 이 값 밑으로 내려가지 않도록 클램프")]
        [SerializeField] private float _minMultiplier = 0.01f;

        public override float Evaluate(float baseValue, int difficulty)
            => baseValue * Mathf.Max(_minMultiplier, _multiplier.Evaluate(difficulty));
    }
}
