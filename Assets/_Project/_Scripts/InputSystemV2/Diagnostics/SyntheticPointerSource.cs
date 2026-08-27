using System;
using Shield_Shot.InputSystemV2.Application;
using Shield_Shot.InputSystemV2.Domain;

namespace Shield_Shot.InputSystemV2.Diagnostics
{
    public sealed class SyntheticPointerSource : IPointerSource
    {
        private readonly PointerSample[] samples;
        private readonly int maxSamplesPerCollect;

        private int nextIndex;

        public int TotalCount => samples.Length;
        public int DeliveredCount => nextIndex;
        public int RemainingCount => samples.Length - nextIndex;
        public bool IsCompleted => nextIndex >= samples.Length;

        public SyntheticPointerSource(
            PointerSample[] samples,
            int maxSamplesPerCollect = 64)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            if (maxSamplesPerCollect <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxSamplesPerCollect),
                    maxSamplesPerCollect,
                    "Maximum samples per collection must be greater than zero.");
            }

            this.samples = new PointerSample[samples.Length];
            Array.Copy(
                samples,
                this.samples,
                samples.Length);

            this.maxSamplesPerCollect =
                maxSamplesPerCollect;
        }

        public void Collect(IPointerSampleSink sink)
        {
            if (sink == null)
            {
                throw new ArgumentNullException(nameof(sink));
            }

            int deliveredThisCall = 0;

            while (nextIndex < samples.Length &&
                   deliveredThisCall < maxSamplesPerCollect)
            {
                PointerSample sample =
                    samples[nextIndex];

                nextIndex++;
                deliveredThisCall++;

                sink.Receive(in sample);
            }
        }

        public void Reset()
        {
            nextIndex = 0;
        }
    }
}