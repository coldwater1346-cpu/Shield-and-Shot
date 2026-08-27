using Shield_Shot.InputSystemV2.Application;
using Shield_Shot.InputSystemV2.Domain;
using UnityEngine;

namespace Shield_Shot.InputSystemV2.Diagnostics
{
    public sealed class PointerPipelineDiagnosticBehaviour
        : MonoBehaviour
    {
        [Header("Execution")]
        [SerializeField]
        private bool runAutomatically = true;

        [SerializeField, Min(1)]
        private int maxSamplesPerFrame = 4;

        [Header("Runtime State")]
        [SerializeField]
        private bool isCompleted;

        [SerializeField]
        private int remainingCount;

        [SerializeField]
        private long totalCount;

        [SerializeField]
        private long beganCount;

        [SerializeField]
        private long movedCount;

        [SerializeField]
        private long stationaryCount;

        [SerializeField]
        private long endedCount;

        [SerializeField]
        private long canceledCount;
        [SerializeField]
        private int deliveredCount;

        [SerializeField]
        private long coalescedOutputCount;

        [SerializeField]
        private long coalescedSuppressedCount;

        [SerializeField]
        private long thresholdRejectedCount;

        [SerializeField]
        private long totalSuppressedCount;

        private SyntheticPointerSource source;
        private PointerSampleCounter counter;
        private PointerMovementThresholdFilter movementFilter;
        private PointerMoveCoalescingSink movementCoalescer;
        private PointerSampleCounter coalescedCounter;
        private IPointerFrameSink pipeline;

        private void Awake()
        {
            BuildPipeline();
            SynchronizeInspectorState();
        }

        private void Update()
        {
            if (!runAutomatically ||
                source == null ||
                source.IsCompleted)
            {
                return;
            }

            source.Collect(pipeline);
            pipeline.CompleteFrame();

            SynchronizeInspectorState();
        }

        [ContextMenu("Reset Diagnostic")]
        public void ResetDiagnostic()
        {
            if (source == null ||
    coalescedCounter == null ||
    counter == null)
            {
                return;
            }

            source.Reset();
            coalescedCounter.Reset();
            counter.Reset();
            movementFilter.Reset();
            movementCoalescer.Reset();

            SynchronizeInspectorState();
        }

        private void BuildPipeline()
        {
            PointerSample[] samples =
{
    new PointerSample(
        pointerId: 0,
        deviceKind: PointerDeviceKind.Touch,
        phase: PointerPhase.Began,
        screenPosition: new Vector2(100f, 200f),
        timestamp: 0d),

    // 5px 미만: 제거 대상
    new PointerSample(
        pointerId: 0,
        deviceKind: PointerDeviceKind.Touch,
        phase: PointerPhase.Moved,
        screenPosition: new Vector2(101f, 201f),
        timestamp: 0.1d),

    // 여전히 마지막 통과 위치에서 5px 미만: 제거 대상
    new PointerSample(
        pointerId: 0,
        deviceKind: PointerDeviceKind.Touch,
        phase: PointerPhase.Moved,
        screenPosition: new Vector2(103f, 202f),
        timestamp: 0.2d),

    // 마지막 통과 위치에서 5px 이상: 통과 대상
    new PointerSample(
        pointerId: 0,
        deviceKind: PointerDeviceKind.Touch,
        phase: PointerPhase.Moved,
        screenPosition: new Vector2(110f, 205f),
        timestamp: 0.3d),

    new PointerSample(
        pointerId: 0,
        deviceKind: PointerDeviceKind.Touch,
        phase: PointerPhase.Ended,
        screenPosition: new Vector2(111f, 206f),
        timestamp: 0.4d)
};

            source = new SyntheticPointerSource(
    samples,
    maxSamplesPerFrame);

            coalescedCounter =
                new PointerSampleCounter();

            counter =
                new PointerSampleCounter();

            movementFilter =
                new PointerMovementThresholdFilter(
                    minimumDistance: 5f);

            IPointerSampleSink filteredSink =
                new FilteredPointerSink(
                    movementFilter,
                    counter);

            IPointerSampleSink observedSink =
                new ObservedPointerSink(
                    coalescedCounter,
                    filteredSink);

            movementCoalescer =
                new PointerMoveCoalescingSink(
                    observedSink);

            pipeline = movementCoalescer;
        }

        private void SynchronizeInspectorState()
        {
            isCompleted =
                source != null &&
                source.IsCompleted;

            remainingCount =
                source != null 
                ? source.RemainingCount 
                : 0;

            deliveredCount =
                source != null 
                ? source.DeliveredCount 
                : 0;

            if (counter == null)
            {
                totalCount = 0;
                beganCount = 0;
                movedCount = 0;
                stationaryCount = 0;
                endedCount = 0;
                canceledCount = 0;
                return;
            }

            totalCount = counter.TotalCount;
            beganCount = counter.BeganCount;
            movedCount = counter.MovedCount;
            stationaryCount = counter.StationaryCount;
            endedCount = counter.EndedCount;
            canceledCount = counter.CanceledCount;

            coalescedOutputCount =
                coalescedCounter != null 
                ? coalescedCounter.TotalCount 
                : 0;

            coalescedSuppressedCount =
                deliveredCount - coalescedOutputCount;

            thresholdRejectedCount =
                coalescedOutputCount - totalCount;

            totalSuppressedCount =
                deliveredCount - totalCount;
        }
    }
}
