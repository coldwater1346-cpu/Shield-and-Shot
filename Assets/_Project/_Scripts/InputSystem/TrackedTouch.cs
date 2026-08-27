using UnityEngine;
using Shield_Shot.InputSystem.Data;

namespace Shield_Shot.InputSystem
{
    public class TrackedTouch
    {
        public int fingerId;
        public Vector2 startPosition;
        public Vector2 currentPosition;

        public float startTime;
        public float lastMovedTime; // 마지막으로 드래그(이동)한 시간

        public GestureState state = GestureState.Tracking;
        public bool isChargingTriggered = false; // 차징 시작 이벤트가 호출되었는지 여부

        // 시작 지점부터 현재까지의 누적 이동 거리
        public float TotalDistance => Vector2.Distance(startPosition, currentPosition);
    }
}