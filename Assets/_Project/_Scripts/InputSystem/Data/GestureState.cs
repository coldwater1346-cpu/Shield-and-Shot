using UnityEngine;

namespace Shield_Shot.InputSystem.Data
{
    public enum GestureState
    {
        None,
        Tracking,       // 입력 추적 중 (일반 발사 대기)
        Charging,       // 차징 조건 충족 후 차징 중
        ChargedComplete, // 풀차징 완료 상태 (필요 시 활용)
        Released, // 손가락을 뗀 상태
        Canceled    // 시스템적으로 취소된 상태
    }
}