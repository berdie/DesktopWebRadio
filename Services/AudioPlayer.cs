using NAudio.Wave;
using System;
using System.IO;
using System.Net;
using System.Net.Http;  // Aggiunta per HttpClient
using System.Threading.Tasks;  // Aggiunta per supporto asincrono

namespace DesktopWebRadio.Services
{
    public class AudioPlayer : IDisposable
    {
        private WaveOutEvent outputDevice;
        private IWaveProvider? currentStreamProvider;
        private bool disposed = false;
        private static readonly HttpClient httpClient = new HttpClient();

        static AudioPlayer()
        {
            // Configura il client HTTP globale
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DesktopWebRadio/1.0");
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
        public event EventHandler<PlaybackErrorEventArgs>? PlaybackError;

        public float Volume
        {
            get => outputDevice?.Volume ?? 0f;
            set { if (outputDevice != null) outputDevice.Volume = value; }
        }

        public PlaybackState PlaybackState => outputDevice?.PlaybackState ?? PlaybackState.Stopped;

        public AudioPlayer()
        {
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
        }

        // Metodo moderno che utilizza HttpClient invece di HttpWebRequest
        public static async Task<Stream> GetStreamFromUrlAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {
                throw new IOException($"Error retrieving stream from URL: {ex.Message}", ex);
            }
        }

        public async Task<IWaveProvider?> PlayStreamAsync(string streamUrl) // Updated return type to allow nullability
        {
            try
            {
                StopPlayback();

                IWaveProvider? provider = null; // Initialize as nullable

                try
                {
                    provider = new MediaFoundationReader(streamUrl);
                }
                catch (Exception ex) when (ex.Message.Contains("AcmNotPossible") ||
                                          ex.Message.Contains("acmStreamOpen"))
                {
                    try
                    {
                        var responseStream = await GetStreamFromUrlAsync(streamUrl);
                        var bufferedStream = new BufferedStream(responseStream, 65536);
                        provider = new Mp3FileReader(bufferedStream);
                    }
                    catch (HttpRequestException httpEx)
                    {
                        OnPlaybackError(new PlaybackErrorEventArgs(
                            $"Network error: {httpEx.Message} (Status: {httpEx.StatusCode})",
                            httpEx));
                        return null;
                    }
                    catch (Exception fallbackEx)
                    {
                        OnPlaybackError(new PlaybackErrorEventArgs(
                            $"Failed to open stream with fallback method: {fallbackEx.Message}",
                            fallbackEx));
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    OnPlaybackError(new PlaybackErrorEventArgs(
                        $"Error opening audio stream: {ex.Message}", ex));
                    return null;
                }

                try
                {
                    if (provider != null) // Ensure provider is not null before initializing
                    {
                        outputDevice.Init(provider);
                        outputDevice.Play();
                        currentStreamProvider = provider;

                        OnPlaybackStateChanged(new PlaybackStateChangedEventArgs(PlaybackState.Playing));
                    }
                    return provider;
                }
                catch (Exception ex)
                {
                    OnPlaybackError(new PlaybackErrorEventArgs(
                        $"Error initializing playback: {ex.Message}", ex));
                    return null;
                }
            }
            catch (Exception ex)
            {
                OnPlaybackError(new PlaybackErrorEventArgs(
                    $"Unexpected error in PlayStream: {ex.Message}", ex));
                return null;
            }
        }


        // Manteniamo anche una versione sincrona per compatibilità con il codice esistente
        public IWaveProvider? PlayStream(string streamUrl)
        {
            return PlayStreamAsync(streamUrl).GetAwaiter().GetResult();
        }

        // Resto della classe invariato
        public void StopPlayback()
        {
            if (outputDevice != null && outputDevice.PlaybackState != PlaybackState.Stopped)
            {
                outputDevice.Stop();
            }

            if (currentStreamProvider != null && currentStreamProvider is IDisposable disposable)
            {
                disposable.Dispose();
                currentStreamProvider = null;
            }

            OnPlaybackStateChanged(new PlaybackStateChangedEventArgs(PlaybackState.Stopped));
        }

        private void OutputDevice_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                OnPlaybackError(new PlaybackErrorEventArgs(
                    $"Playback stopped due to error: {e.Exception.Message}", e.Exception));
            }

            OnPlaybackStateChanged(new PlaybackStateChangedEventArgs(PlaybackState.Stopped));
        }


        protected void OnPlaybackStateChanged(PlaybackStateChangedEventArgs e)
        {
            PlaybackStateChanged?.Invoke(this, e);
        }

        protected void OnPlaybackError(PlaybackErrorEventArgs e)
        {
            PlaybackError?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                StopPlayback();

                if (outputDevice != null)
                {
                    outputDevice.Dispose();
                    outputDevice = null!; // Use null-forgiving operator to suppress nullable warning
                }

                disposed = true;
            }
        }

        public class PlaybackStateChangedEventArgs : EventArgs
        {
            public PlaybackState State { get; }

            public PlaybackStateChangedEventArgs(PlaybackState state)
            {
                State = state;
            }
        }

        public class PlaybackErrorEventArgs : EventArgs
        {
            public string Message { get; }
            public Exception Exception { get; }

            public PlaybackErrorEventArgs(string message, Exception exception)
            {
                Message = message;
                Exception = exception;
            }
        }
    }
}
