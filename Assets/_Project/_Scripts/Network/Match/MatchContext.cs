using System.Collections.Generic;
using System.Linq;

namespace Shield_Shot.GameplayCore.Network.Match
{
    public sealed class MatchContext
    {
        private readonly List<MatchPlayerInfo> _players = new();

        public string MatchId { get; }
        public string SessionName { get; }
        public string SceneName { get; }
        public MatchMode MatchMode { get; }

        public IReadOnlyList<MatchPlayerInfo> Players => _players;

        public MatchContext(
            string matchId,
            string sessionName,
            string sceneName,
            MatchMode matchMode,
            IEnumerable<MatchPlayerInfo> players)
        {
            MatchId = matchId;
            SessionName = sessionName;
            SceneName = sceneName;
            MatchMode = matchMode;

            if (players != null)
            {
                _players.AddRange(players);
            }
        }

        public bool TryGetPlayerBySide(PlayerSide side, out MatchPlayerInfo playerInfo)
        {
            playerInfo = _players.FirstOrDefault(player => player.Side == side);
            return playerInfo != null;
        }

        public bool TryGetOpponent(PlayerSide localSide, out MatchPlayerInfo opponentInfo)
        {
            PlayerSide opponentSide = localSide == PlayerSide.Bottom
                ? PlayerSide.Top
                : PlayerSide.Bottom;

            return TryGetPlayerBySide(opponentSide, out opponentInfo);
        }
    }
}