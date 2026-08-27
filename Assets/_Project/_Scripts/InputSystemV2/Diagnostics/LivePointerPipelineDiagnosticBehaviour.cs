using Shield_Shot.InputSystemV2.Application;
using Shield_Shot.InputSystemV2.Infrastructure;
using Shield_Shot.InputSystemV2.Combat.Application;
using Shield_Shot.InputSystemV2.Combat.Diagnostics;
using Shield_Shot.InputSystemV2.Combat.Domain;
using Shield_Shot.InputSystemV2.Gestures.Application;
using Shield_Shot.InputSystemV2.Gestures.Diagnostics;
using Shield_Shot.InputSystemV2.Integration;
using UnityEngine;

namespace Shield_Shot.InputSystemV2.Diagnostics
{
    [RequireComponent(typeof(UnityPointerInputDriver))]
    public sealed class LivePointerPipelineDiagnosticBehaviour
        : MonoBehaviour
    {
        [Header("Filter")]
        [SerializeField, Min(0f)]
        private float minimumMovementDistance = 5f;

        [Header("Pipeline Counts")]
        [SerializeField]
        private long rawInputCount;

        [SerializeField]
        private long coalescedOutputCount;

        [SerializeField]
        private long finalOutputCount;

        [Header("Suppressed Counts")]
        [SerializeField]
        private long coalescedSuppressedCount;

        [SerializeField]
        private long thresholdRejectedCount;

        [SerializeField]
        private long totalSuppressedCount;

        [Header("Final Output Phases")]
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

        [Header("UI Filter Counts")]
        [SerializeField]
        private long uiAcceptedCount;

        [SerializeField]
        private long uiRejectedCount;

        [Header("Combat Routing")]
        [SerializeField]
        private CombatSplitDirection splitDirection =
            CombatSplitDirection.LeftRight;

        [SerializeField, Range(0.01f, 0.99f)]
        private float splitRatio = 0.5f;

        [SerializeField]
        private bool isInverted;

        [SerializeField]
        private Vector2 referenceResolution =
            new Vector2(1080f, 1920f);

        [SerializeField]
        private long attackInputCount;

        [SerializeField]
        private long defenseInputCount;

        [Header("Attack Gestures")]
        [SerializeField]
        private long attackGestureCount;

        [SerializeField]
        private long attackGestureBeganCount;

        [SerializeField]
        private long attackGestureChangedCount;

        [SerializeField]
        private long attackGestureCompletedCount;

        [SerializeField]
        private long attackGestureCanceledCount;

        [Header("Defense Gestures")]
        [SerializeField]
        private long defenseGestureCount;

        [SerializeField]
        private long defenseGestureBeganCount;

        [SerializeField]
        private long defenseGestureChangedCount;

        [SerializeField]
        private long defenseGestureCompletedCount;

        [SerializeField]
        private long defenseGestureCanceledCount;

        [Header("Attack Charge")]
        [SerializeField, Range(0f, 1f)]
        private float minimumAimDistanceRatio = 0.05f;

        [SerializeField, Min(0f)]
        private double chargeStartDelay = 0.15d;

        [SerializeField, Min(0.01f)]
        private double fullChargeDuration = 1d;

        [SerializeField, Min(0.001f)]
        private double chargeSignalInterval = 1d / 30d;

        [Header("Attack Input Signals")]
        [SerializeField]
        private long attackSignalCount;

        [SerializeField]
        private long attackAimChangedCount;

        [SerializeField]
        private long attackChargeChangedCount;

        [SerializeField]
        private long attackReleasedCount;

        [SerializeField]
        private long attackCanceledCount;

        [SerializeField]
        private float maximumChargeRatio;

        [SerializeField]
        private float latestChargeRatio;

        [SerializeField]
        private float lastReleasedChargeRatio;

        [Header("Viewport Filter Counts")]
        [SerializeField]
        private long viewportAcceptedCount;

        [SerializeField]
        private long viewportRejectedCount;

        [Header("Gameplay Output")]
        [SerializeField]
        private WeaponAttackInputAdapter weaponAttackInputAdapter;

        private PointerSampleCounter viewportAcceptedCounter;
        private PointerStartBlockFilter viewportBlockFilter;

        private PointerGestureCounter attackGestureCounter;
        private PointerGestureCounter defenseGestureCounter;

        private PointerGestureTracker attackGestureTracker;
        private PointerGestureTracker defenseGestureTracker;

        private AttackInputCounter attackInputCounter;
        private AttackGestureInterpreter attackInterpreter;
        private UnityInputClock inputClock;

        private UnityPointerInputDriver driver;

        private PointerSampleCounter rawCounter;
        private PointerSampleCounter coalescedCounter;
        private PointerSampleCounter finalCounter;
        private PointerSampleCounter attackCounter;
        private PointerSampleCounter defenseCounter;

        private CombatPointerRouter combatRouter;

        private PointerMovementThresholdFilter movementFilter;
        private PointerMoveCoalescingSink movementCoalescer;

        private PointerSampleCounter uiAcceptedCounter;

        private EventSystemPointerStartBlockPolicy
            uiBlockPolicy;

        private PointerStartBlockFilter
            uiBlockFilter;

        private void Awake()
        {
            driver =
                GetComponent<UnityPointerInputDriver>();

            BuildPipeline();
            SynchronizeInspectorState();
        }

        private void LateUpdate()
        {
            attackInterpreter?.Tick();
            SynchronizeInspectorState();
        }

        [ContextMenu("Reset Live Diagnostic")]
        public void ResetDiagnostic()
        {
            if (rawCounter == null ||
                uiAcceptedCounter == null ||
                coalescedCounter == null ||
                finalCounter == null ||
                uiBlockFilter == null ||
                movementFilter == null ||
                movementCoalescer == null||
                attackCounter == null ||
                defenseCounter == null ||
                combatRouter == null||
                attackGestureCounter == null ||
                defenseGestureCounter == null ||
                attackGestureTracker == null ||
                defenseGestureTracker == null||
                attackInputCounter == null ||
                attackInterpreter == null ||
                viewportAcceptedCounter == null ||
                viewportBlockFilter == null)


            {
                return;
            }

            rawCounter.Reset();
            uiAcceptedCounter.Reset();
            coalescedCounter.Reset();
            finalCounter.Reset();
            uiBlockFilter.Reset();
            movementFilter.Reset();
            movementCoalescer.Reset();
            attackCounter.Reset();
            defenseCounter.Reset();
            combatRouter.Reset();
            attackGestureCounter.Reset();
            defenseGestureCounter.Reset();

            attackGestureTracker.Reset();
            defenseGestureTracker.Reset();

            attackInputCounter.Reset();
            attackInterpreter.Reset();
            weaponAttackInputAdapter?.ResetInput();

            viewportAcceptedCounter.Reset();
            viewportBlockFilter.Reset();
            SynchronizeInspectorState();
        }

        private void BuildPipeline()
        {
            rawCounter =
                new PointerSampleCounter();

            uiAcceptedCounter =
                new PointerSampleCounter();

            coalescedCounter =
                new PointerSampleCounter();

            finalCounter =
                new PointerSampleCounter();

            attackCounter =
                new PointerSampleCounter();

            defenseCounter =
                new PointerSampleCounter();

            viewportAcceptedCounter =
                new PointerSampleCounter();

            CombatInputLayout combatLayout =
                new CombatInputLayout(
                    splitDirection,
                    splitRatio,
                    isInverted);

            ICombatInputLayoutProvider layoutProvider =
                new FixedCombatInputLayoutProvider(
                    in combatLayout);

            Rect viewport =
                new Rect(
                    0f,
                    0f,
                    referenceResolution.x,
                    referenceResolution.y);

            IPointerViewportProvider viewportProvider =
                new FixedPointerViewportProvider(
                    in viewport);

            AttackChargeSettings chargeSettings =
                new AttackChargeSettings(
                    minimumAimDistanceRatio,
                    chargeStartDelay,
                    fullChargeDuration,
                    chargeSignalInterval);

            IAttackChargeSettingsProvider chargeSettingsProvider =
                new FixedAttackChargeSettingsProvider(
                    in chargeSettings);

            inputClock =
                new UnityInputClock();

            attackInputCounter =
                new AttackInputCounter();

            IAttackInputSink attackOutput = 
                attackInputCounter;

            if (weaponAttackInputAdapter != null)
            {
                attackOutput =
                    new ObservedAttackInputSink(
                        attackInputCounter,
                        weaponAttackInputAdapter);
            }

            attackInterpreter =
                new AttackGestureInterpreter(
                    chargeSettingsProvider,
                    viewportProvider,
                    inputClock,
                    attackOutput);

            ICombatInputChannelResolver channelResolver =
                new SplitCombatInputChannelResolver(
                    layoutProvider,
                    viewportProvider);

            IPointerStartBlockPolicy viewportBlockPolicy =
                new ViewportPointerStartBlockPolicy(
                    viewportProvider);

            viewportBlockFilter =
                new PointerStartBlockFilter(
                    viewportBlockPolicy);

            attackGestureCounter =
                new PointerGestureCounter();

            defenseGestureCounter =
                new PointerGestureCounter();

            IPointerGestureSink observedAttackGestureSink =
                new ObservedPointerGestureSink(
                    attackGestureCounter,
                    attackInterpreter);

            attackGestureTracker =
                new PointerGestureTracker(
                    observedAttackGestureSink);

            defenseGestureTracker =
                new PointerGestureTracker(
                    defenseGestureCounter);

            IPointerSampleSink observedAttackSink =
                new ObservedPointerSink(
                    attackCounter,
                    attackGestureTracker);

            IPointerSampleSink observedDefenseSink =
                new ObservedPointerSink(
                    defenseCounter,
                    defenseGestureTracker);

            combatRouter =
                new CombatPointerRouter(
                    channelResolver,
                    observedAttackSink,
                    observedDefenseSink);

            uiBlockPolicy =
                new EventSystemPointerStartBlockPolicy();

            uiBlockFilter =
                new PointerStartBlockFilter(
                    uiBlockPolicy);

            movementFilter =
                new PointerMovementThresholdFilter(
                    minimumMovementDistance);

            IPointerSampleSink observedFinalSink =
                new ObservedPointerSink(
                    finalCounter,
                    combatRouter);

            IPointerSampleSink filteredMovementSink =
                new FilteredPointerSink(
                    movementFilter,
                    observedFinalSink);

            IPointerSampleSink observedCoalescedSink =
                new ObservedPointerSink(
                    coalescedCounter,
                    filteredMovementSink);

            movementCoalescer =
                new PointerMoveCoalescingSink(
                    observedCoalescedSink);

            IPointerFrameSink observedUiAcceptedPipeline =
                new ObservedPointerFrameSink(
                    uiAcceptedCounter,
                    movementCoalescer);

            IPointerFrameSink uiFilteredPipeline =
                new FilteredPointerFrameSink(
                    uiBlockFilter,
                    observedUiAcceptedPipeline);

            IPointerFrameSink observedViewportAcceptedPipeline =
                new ObservedPointerFrameSink(
                    viewportAcceptedCounter,
                    uiFilteredPipeline);

            IPointerFrameSink viewportFilteredPipeline =
                new FilteredPointerFrameSink(
                    viewportBlockFilter,
                    observedViewportAcceptedPipeline);

            IPointerFrameSink observedRawPipeline =
                new ObservedPointerFrameSink(
                    rawCounter,
                    viewportFilteredPipeline);

            driver.SetPipeline(
                observedRawPipeline);
        }

        private void SynchronizeInspectorState()
        {
            rawInputCount =
                rawCounter != null
                    ? rawCounter.TotalCount
                    : 0;

            coalescedOutputCount =
                coalescedCounter != null
                    ? coalescedCounter.TotalCount
                    : 0;

            finalOutputCount =
                finalCounter != null
                    ? finalCounter.TotalCount
                    : 0;

            uiAcceptedCount =
                uiAcceptedCounter != null
                    ? uiAcceptedCounter.TotalCount
                    : 0;
            attackInputCount =
                   attackCounter != null
                    ? attackCounter.TotalCount
                    : 0;

            defenseInputCount =
                defenseCounter != null
                    ? defenseCounter.TotalCount
                    : 0;
            viewportAcceptedCount =
                viewportAcceptedCounter != null
                    ? viewportAcceptedCounter.TotalCount
                    : 0;

            viewportRejectedCount =
                rawInputCount -
                viewportAcceptedCount;

            uiRejectedCount =
                viewportAcceptedCount -
                uiAcceptedCount;

            coalescedSuppressedCount =
                uiAcceptedCount -
                coalescedOutputCount;

            thresholdRejectedCount =
                coalescedOutputCount -
                finalOutputCount;

            totalSuppressedCount =
                rawInputCount -
                finalOutputCount;

            if (finalCounter == null)
            {
                beganCount = 0;
                movedCount = 0;
                stationaryCount = 0;
                endedCount = 0;
                canceledCount = 0;
                return;
            }
            if (attackGestureCounter != null)
            {
                attackGestureCount =
                    attackGestureCounter.TotalCount;

                attackGestureBeganCount =
                    attackGestureCounter.BeganCount;

                attackGestureChangedCount =
                    attackGestureCounter.ChangedCount;

                attackGestureCompletedCount =
                    attackGestureCounter.CompletedCount;

                attackGestureCanceledCount =
                    attackGestureCounter.CanceledCount;
            }

            if (defenseGestureCounter != null)
            {
                defenseGestureCount =
                    defenseGestureCounter.TotalCount;

                defenseGestureBeganCount =
                    defenseGestureCounter.BeganCount;

                defenseGestureChangedCount =
                    defenseGestureCounter.ChangedCount;

                defenseGestureCompletedCount =
                    defenseGestureCounter.CompletedCount;

                defenseGestureCanceledCount =
                    defenseGestureCounter.CanceledCount;
            }

            if (attackInputCounter != null)
            {
                attackSignalCount =
                    attackInputCounter.TotalCount;

                attackAimChangedCount =
                    attackInputCounter.AimChangedCount;

                attackChargeChangedCount =
                    attackInputCounter.ChargeChangedCount;

                attackReleasedCount =
                    attackInputCounter.ReleasedCount;

                attackCanceledCount =
                    attackInputCounter.CanceledCount;

                maximumChargeRatio =
                    attackInputCounter.MaximumChargeRatio;

                latestChargeRatio =
                    attackInputCounter.LatestChargeRatio;

                lastReleasedChargeRatio =
                    attackInputCounter.LastReleasedChargeRatio;
            }

            beganCount =
                finalCounter.BeganCount;

            movedCount =
                finalCounter.MovedCount;

            stationaryCount =
                finalCounter.StationaryCount;

            endedCount =
                finalCounter.EndedCount;

            canceledCount =
                finalCounter.CanceledCount;
        }
    }
}
