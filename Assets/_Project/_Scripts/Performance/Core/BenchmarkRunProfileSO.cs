using UnityEngine;

namespace Shield_Shot.Performance
{
    [CreateAssetMenu(
        fileName = "BenchmarkRunProfile",
        menuName = "Shield Shot/Performance/Benchmark Run Profile")]
    public sealed class BenchmarkRunProfileSO : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private string profileId;
        [SerializeField] private string scenarioId;
        [SerializeField] private string implementationId;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float warmupSeconds = 3f;
        [SerializeField, Min(1f)] private float measurementSeconds = 10f;
        [SerializeField, Min(1)] private int repeatCount = 3;

        [Header("Environment")]
        [SerializeField] private int randomSeed = 1004;
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool disableVSync = true;

        public string ProfileId => profileId;
        public string ScenarioId => scenarioId;
        public string ImplementationId => implementationId;

        public float WarmupSeconds => warmupSeconds;
        public float MeasurementSeconds => measurementSeconds;
        public int RepeatCount => repeatCount;

        public int RandomSeed => randomSeed;
        public int TargetFrameRate => targetFrameRate;
        public bool DisableVSync => disableVSync;
    }
}