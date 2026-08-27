using Shield_Shot.InputSystemV2.Gestures.Application;
using Shield_Shot.InputSystemV2.Gestures.Domain;

namespace Shield_Shot.InputSystemV2.Gestures.Diagnostics
{
    public sealed class PointerGestureCounter
        : IPointerGestureSink
    {
        public long TotalCount { get; private set; }
        public long BeganCount { get; private set; }
        public long ChangedCount { get; private set; }
        public long CompletedCount { get; private set; }
        public long CanceledCount { get; private set; }

        public bool HasLastGesture { get; private set; }

        public PointerGestureSample LastGesture
        {
            get;
            private set;
        }

        public void Receive(
            in PointerGestureSample gesture)
        {
            TotalCount++;

            switch (gesture.Phase)
            {
                case PointerGesturePhase.Began:
                    BeganCount++;
                    break;

                case PointerGesturePhase.Changed:
                    ChangedCount++;
                    break;

                case PointerGesturePhase.Completed:
                    CompletedCount++;
                    break;

                case PointerGesturePhase.Canceled:
                    CanceledCount++;
                    break;
            }

            LastGesture = gesture;
            HasLastGesture = true;
        }

        public void Reset()
        {
            TotalCount = 0;
            BeganCount = 0;
            ChangedCount = 0;
            CompletedCount = 0;
            CanceledCount = 0;

            LastGesture = default;
            HasLastGesture = false;
        }
    }
}