using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Shield_Shot.InputSystem.Data
{
    public struct InputContext
    {
        public int fingerId;          // 멀티터치 구분을 위한 ID
        public GestureState state;       // 현재 입력 상태
        public float holdTime;         // 누르고 있던 시간
        public Vector2 dragVector;     // 드래그 방향 및 거리
        public float totalDistance;    // 시작점으로부터의 총 누적 거리
        public Vector2 startPosition;  // 터치가 시작된 스크린 좌표 (조이스틱 배경 위치 등 UI 앵커링용)
    }
}
