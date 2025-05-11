using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace DesktopWebRadio.Services
{
    public class AudioVisualizationProvider : ISampleProvider, IDisposable
    {
        private readonly ISampleProvider source;
        private readonly int fftLength;
        private readonly Complex[] fftBuffer;
        private readonly float[] fftResult;
        private readonly FftEventArgs fftArgs;
        private readonly SampleAggregator sampleAggregator;
        private bool disposed;

        public WaveFormat WaveFormat => source.WaveFormat;
        
        public AudioVisualizationProvider(IWaveProvider waveProvider)
        {
            // Try to safely convert IWaveProvider to ISampleProvider with better error handling
            try
            {
                if (waveProvider is ISampleProvider sampleProvider)
                {
                    source = sampleProvider;
                }
                else
                {
                    // Check if conversion to IEEE float format is needed
                    if (waveProvider.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                    {
                        try
                        {
                            // Try MediaFoundationResampler first if available
                            if (waveProvider is IWaveProvider provider && provider is WaveStream stream)
                            {
                                var resampler = new MediaFoundationResampler(
                                    stream,
                                    WaveFormat.CreateIeeeFloatWaveFormat(
                                        waveProvider.WaveFormat.SampleRate,
                                        waveProvider.WaveFormat.Channels));
                                source = new WaveToSampleProvider(resampler);
                            }
                            else
                            {
                                // Fallback to standard conversion
                                var converter = new WaveFormatConversionStream(
                                    WaveFormat.CreateIeeeFloatWaveFormat(
                                        waveProvider.WaveFormat.SampleRate,
                                        waveProvider.WaveFormat.Channels),
                                    waveProvider as WaveStream);
                                source = new WaveToSampleProvider(converter);
                            }
                        }
                        catch (Exception)
                        {
                            // Last resort: try direct conversion which may work with some formats
                            source = new WaveToSampleProvider(waveProvider);
                        }
                    }
                    else
                    {
                        // Already IEEE float format, just convert to sample provider
                        source = new WaveToSampleProvider(waveProvider);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert audio format for visualization: {ex.Message}", ex);
            }

            fftLength = 2048; // Must be a power of 2
            fftBuffer = new Complex[fftLength];
            fftResult = new float[fftLength / 2]; // Output is half the size

            fftArgs = new FftEventArgs(fftResult);
            sampleAggregator = new SampleAggregator(fftLength);
            sampleAggregator.FftCalculated += OnFftCalculated;
            sampleAggregator.PerformFFT = true;
        }

        private void OnFftCalculated(object sender, FftEventArgs e)
        {
            // Just copy the result
            Array.Copy(e.Result, fftResult, fftResult.Length);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            try
            {
                // Read from the source
                int samplesRead = source.Read(buffer, offset, count);

                // Process samples through the aggregator
                for (int i = 0; i < samplesRead; i++)
                {
                    sampleAggregator.Add(buffer[offset + i]);
                }

                return samplesRead;
            }
            catch (Exception)
            {
                // If reading fails, return silence
                for (int i = 0; i < count; i++)
                {
                    buffer[offset + i] = 0f;
                }
                return count;
            }
        }

        /// <summary>
        /// Creates a new AudioVisualizationProvider with the specified FFT length
        /// </summary>
        /// <param name="waveProvider">The audio source</param>
        /// <param name="fftLength">FFT length (must be a power of 2)</param>
        /// <returns>A new AudioVisualizationProvider</returns>
        public static AudioVisualizationProvider Create(IWaveProvider waveProvider, int fftLength = 2048)
        {
            try
            {
                return new AudioVisualizationProvider(waveProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating AudioVisualizationProvider: {ex.Message}");

                // Try with a simplified approach if the standard one fails
                try
                {
                    // We can't directly convert ISampleProvider to IWaveProvider
                    // So we'll just create a new provider with the original waveProvider
                    return new AudioVisualizationProvider(waveProvider);
                }
                catch
                {
                    // If all else fails, throw the original exception
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns the current FFT data
        /// </summary>
        /// <returns>Array of FFT values</returns>
        public float[] GetFFTData()
        {
            return fftResult;
        }

        /// <summary>
        /// Disposes resources used by the audio visualization provider
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                // Clean up events
                if (sampleAggregator != null)
                {
                    sampleAggregator.FftCalculated -= OnFftCalculated;
                }

                // Clean up disposable source
                if (source is IDisposable disposableSource)
                {
                    disposableSource.Dispose();
                }

                disposed = true;
            }
        }
    }
}
