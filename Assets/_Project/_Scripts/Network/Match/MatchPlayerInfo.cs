using Fusion;

namespace Shield_Shot.GameplayCore.Network.Match
{
    public sealed class MatchPlayerInfo
    {
        public string UserId { get; }
        public string Nickname { get; }
        public PlayerRef PlayerRef { get; }
        public PlayerSide Side { get; }

        public MatchPlayerInfo(
            string userId,
            string nickname,
            PlayerRef playerRef,
            PlayerSide side)
        {
            UserId = userId;
            Nickname = nickname;
            PlayerRef = playerRef;
            Side = side;
        }
    }
}