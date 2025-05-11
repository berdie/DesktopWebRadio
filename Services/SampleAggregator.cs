using NAudio.Dsp;
using System;

namespace DesktopWebRadio.Services
{
    /// <summary>
    /// Provides sample aggregation and FFT calculation functionality
    /// </summary>
    public class SampleAggregator
    {
        private readonly float[] fftBuffer;
        private readonly NAudio.Dsp.Complex[] fftComplex;
        private int fftPos;
        private readonly int fftLength;
        private readonly FftEventArgs fftArgs;
        private bool performFFT;

        /// <summary>
        /// Occurs when the FFT calculation is complete
        /// </summary>
        public event EventHandler<FftEventArgs> FftCalculated;

        /// <summary>
        /// Gets or sets a value indicating whether FFT calculations should be performed
        /// </summary>
        public bool PerformFFT
        {
            get { return performFFT; }
            set { performFFT = value; }
        }

        /// <summary>
        /// Initializes a new instance of the SampleAggregator class
        /// </summary>
        /// <param name="fftLength">Length of the FFT to calculate (must be a power of 2)</param>
        public SampleAggregator(int fftLength)
        {
            this.fftLength = fftLength;
            fftBuffer = new float[fftLength];
            fftComplex = new NAudio.Dsp.Complex[fftLength];
            fftArgs = new FftEventArgs(new float[fftLength / 2]);
        }

        /// <summary>
        /// Adds a sample value to the aggregator
        /// </summary>
        /// <param name="value">The sample value to add</param>
        public void Add(float value)
        {
            if (performFFT)
            {
                fftBuffer[fftPos] = value;
                fftPos++;
                if (fftPos >= fftLength)
                {
                    fftPos = 0;
                    CalculateFFT();
                }
            }
        }

        /// <summary>
        /// Calculates the FFT for the current sample buffer
        /// </summary>
        private void CalculateFFT()
        {
            // Convert samples to complex numbers
            for (int i = 0; i < fftLength; i++)
            {
                fftComplex[i].X = (float)(fftBuffer[i] * FastFourierTransform.HammingWindow(i, fftLength));
                fftComplex[i].Y = 0;
            }

            // Perform FFT
            FastFourierTransform.FFT(true, (int)Math.Log(fftLength, 2.0), fftComplex);

            // Calculate magnitudes
            for (int i = 0; i < fftLength / 2; i++)
            {
                // Calculate magnitude
                float magnitude = (float)Math.Sqrt(fftComplex[i].X * fftComplex[i].X + fftComplex[i].Y * fftComplex[i].Y);
                
                // Apply scaling to get the amplitude
                fftArgs.Result[i] = magnitude / fftLength;
            }

            // Raise the event
            FftCalculated?.Invoke(this, fftArgs);
        }
    }
} 