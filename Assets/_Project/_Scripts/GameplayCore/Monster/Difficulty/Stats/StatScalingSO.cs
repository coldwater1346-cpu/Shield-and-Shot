using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// "난이도 → 스탯 값" 하나의 수식을 캡슐화하는 전략(Strategy) SO.
    ///
    /// 핵심 계약:  <c>Evaluate(baseValue, difficulty)</c> 는
    ///            난이도가 적용된 "최종 스탯 값"을 돌려준다. (배율이 아니라 값)
    ///
    /// 이 추상 타입을 상속해 원하는 수식을 얼마든지 추가할 수 있다.
    /// (Curve / Linear / Exponential / Step / Composite ...)
    /// 새 수식 = 새 SO 클래스 하나. 리졸버·프로파일은 손대지 않는다.
    /// </summary>
    public abstract class StatScalingSO : ScriptableObject
    {
        /// <param name="baseValue">몬스터의 기본 스탯값(난이도 무관 원본).</param>
        /// <param name="difficulty">이번 웨이브의 난이도 수치.</param>
        /// <returns>난이도가 적용된 최종 스탯값.</returns>
        public abstract float Evaluate(float baseValue, int difficulty);
    }
}
