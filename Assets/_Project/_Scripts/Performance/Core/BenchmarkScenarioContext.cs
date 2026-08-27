using System;
using UnityEngine;

namespace Shield_Shot.Performance
{
    public sealed class BenchmarkScenarioContext
    {
        public BenchmarkRunProfileSO Profile { get; }
        public Transform ScenarioRoot { get; }
        public int IterationIndex { get; }

        public int IterationNumber => IterationIndex + 1;

        public BenchmarkScenarioContext(
            BenchmarkRunProfileSO profile,
            Transform scenarioRoot,
            int iterationIndex)
        {
            Profile = profile != null
                ? profile
                : throw new ArgumentNullException(nameof(profile));

            ScenarioRoot = scenarioRoot != null
                ? scenarioRoot
                : throw new ArgumentNullException(nameof(scenarioRoot));

            if (iterationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(iterationIndex),
                    iterationIndex,
                    "Iteration index cannot be negative.");
            }

            IterationIndex = iterationIndex;
        }
    }
}