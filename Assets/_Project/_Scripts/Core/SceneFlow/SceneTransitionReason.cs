namespace Shield_Shot.Core.SceneFlow
{
    public enum SceneTransitionReason
    {
        None,
        IntroToTitle,
        TitleToLobby,
        QuickMatchFound,
        LobbyToInGame,
        BattleFinished,
        ReturnToLobby,
        Retry
    }
}
