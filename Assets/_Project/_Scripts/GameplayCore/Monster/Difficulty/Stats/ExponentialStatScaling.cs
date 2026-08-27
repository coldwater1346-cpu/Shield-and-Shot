using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 지수 스케일링:  최종값 = base × growth^(difficulty / step)
    ///
    /// 무한 모드처럼 난이도가 끝없이 오르는 구간에서 체력을 복리로 불릴 때 적합.
    /// growth=1.03, step=1 이면 난이도 1당 3% 복리 증가.
    /// </summary>
    [CreateAssetMenu(fileName = "ExponentialScaling", menuName = "Monster/Difficulty/Scaling/Exponential")]
    public class ExponentialStatScaling : StatScalingSO
    {
        [Tooltip("복리 성장률. 1.03 = step마다 +3%")]
        [SerializeField] private float _growth = 1.03f;

        [Tooltip("몇 난이도마다 growth를 1번 적용할지")]
        [SerializeField] private float _step = 1f;

        [Tooltip("배율 상한(폭주 방지). 0 이하면 무제한")]
        [SerializeField] private float _maxMultiplier = 0f;

        public override float Evaluate(float baseValue, int difficulty)
        {
            float safeStep = Mathf.Max(0.0001f, _step);
            float mult = Mathf.Pow(Mathf.Max(0.0001f, _growth), difficulty / safeStep);
            if (_maxMultiplier > 0f) mult = Mathf.Min(mult, _maxMultiplier);
            return baseValue * mult;
        }
    }
}
