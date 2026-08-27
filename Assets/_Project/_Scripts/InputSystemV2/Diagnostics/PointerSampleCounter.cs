using Shield_Shot.InputSystemV2.Application;
using Shield_Shot.InputSystemV2.Domain;

namespace Shield_Shot.InputSystemV2.Diagnostics
{
    public sealed class PointerSampleCounter : IPointerSampleSink
    {
        public long TotalCount { get; private set; }
        public long BeganCount { get; private set; }
        public long MovedCount { get; private set; }
        public long StationaryCount { get; private set; }
        public long EndedCount { get; private set; }
        public long CanceledCount { get; private set; }

        public bool HasLastSample { get; private set; }
        public PointerSample LastSample { get; private set; }

        public void Receive(in PointerSample sample)
        {
            TotalCount++;

            switch (sample.Phase)
            {
                case PointerPhase.Began:
                    BeganCount++;
                    break;

                case PointerPhase.Moved:
                    MovedCount++;
                    break;

                case PointerPhase.Stationary:
                    StationaryCount++;
                    break;

                case PointerPhase.Ended:
                    EndedCount++;
                    break;

                case PointerPhase.Canceled:
                    CanceledCount++;
                    break;
            }

            LastSample = sample;
            HasLastSample = true;
        }

        public void Reset()
        {
            TotalCount = 0;
            BeganCount = 0;
            MovedCount = 0;
            StationaryCount = 0;
            EndedCount = 0;
            CanceledCount = 0;

            LastSample = default;
            HasLastSample = false;
        }
    }
}