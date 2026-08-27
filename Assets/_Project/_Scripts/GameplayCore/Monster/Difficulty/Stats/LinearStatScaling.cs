using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 선형 스케일링:  최종값 = base × (1 + perDifficulty × difficulty) + flatPerDifficulty × difficulty
    ///
    /// - perDifficulty: 난이도 1당 몇 % 증가 (0.05 = 난이도당 +5%)
    /// - flatPerDifficulty: 난이도 1당 더해지는 고정량 (곱연산 없이 덧셈만 하고 싶을 때)
    ///
    /// 곱/합을 한 수식에서 함께 표현할 수 있어 밸런싱 손이 빠르다.
    /// </summary>
    [CreateAssetMenu(fileName = "LinearScaling", menuName = "Monster/Difficulty/Scaling/Linear")]
    public class LinearStatScaling : StatScalingSO
    {
        [Tooltip("난이도 1당 증가율 (0.05 = 난이도당 +5%)")]
        [SerializeField] private float _perDifficulty = 0.05f;

        [Tooltip("난이도 1당 더해지는 고정량")]
        [SerializeField] private float _flatPerDifficulty = 0f;

        [Tooltip("결과 하한")]
        [SerializeField] private float _min = 0f;

        public override float Evaluate(float baseValue, int difficulty)
        {
            float v = baseValue * (1f + _perDifficulty * difficulty) + _flatPerDifficulty * difficulty;
            return Mathf.Max(_min, v);
        }
    }
}
