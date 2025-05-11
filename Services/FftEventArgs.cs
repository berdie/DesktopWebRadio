using System;

namespace DesktopWebRadio.Services
{
    /// <summary>
    /// Event arguments for FFT calculations containing the resulting data
    /// </summary>
    public class FftEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the FFT result data
        /// </summary>
        public float[] Result { get; }

        /// <summary>
        /// Initializes a new instance of the FftEventArgs class
        /// </summary>
        /// <param name="result">The FFT result data</param>
        public FftEventArgs(float[] result)
        {
            Result = result;
        }
    }
} 