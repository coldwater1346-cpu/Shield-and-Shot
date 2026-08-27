namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public enum PvpMatchState
    {
        None = 0,

        // 양쪽 플레이어/무기 스폰이 끝나기 전 대기 상태
        WaitingForPlayers = 1,

        // 전투 시작 전 3, 2, 1 같은 카운트다운 상태
        Countdown = 2,

        // 실제 입력과 공격이 허용되는 전투 상태
        Fighting = 3,

        // 한 명의 HP가 0이 되어 라운드 승자가 결정된 상태
        RoundEnded = 4,

        // 라운드 종료 후 증강 선택 UI가 떠 있는 상태
        AugmentSelection = 5,

        // 누군가 2점을 먼저 획득해서 최종 승패가 결정된 상태
        MatchEnded = 6,

        // 상대 이탈, 접속 종료 등으로 로비 복귀 처리 중인 상태
        ReturningToLobby = 7
    }
}