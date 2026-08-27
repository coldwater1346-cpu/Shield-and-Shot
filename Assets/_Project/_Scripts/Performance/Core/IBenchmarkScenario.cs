using System.Collections;

namespace Shield_Shot.Performance
{
    public interface IBenchmarkScenario
    {
        string ScenarioId { get; }

        IEnumerator Prepare(BenchmarkScenarioContext context);

        void StartWorkload();

        void Tick(float deltaTime);

        void StopWorkload();

        IEnumerator Cleanup();
    }
}