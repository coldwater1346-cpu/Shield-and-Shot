namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// 스테이지 클리어 보상(골드)을 제공하는 소스.
    public interface IStageReward
    {
        int RewardGold { get; }
    }
}