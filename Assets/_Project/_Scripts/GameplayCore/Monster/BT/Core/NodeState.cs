namespace Shield_Shot.GameplayCore.Monster.BT.Core
{
    public enum NodeState
    {
        Success,  // 노드 성공적으로 완료
        Failure,  // 노드 실패
        Running   // 아직 실행 중 (다음 프레임도 계속)
    }
}