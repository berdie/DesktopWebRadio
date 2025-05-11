using NAudio.Wave;
using System;
using System.IO;
using System.Net;

namespace DesktopWebRadio.Services
{
    public class AudioPlayer : IDisposable
    {
        private WaveOutEvent outputDevice;
        private IWaveProvider currentStreamProvider;
        private bool disposed = false;
        
        public event EventHandler<PlaybackStateChangedEventArgs> PlaybackStateChanged;
        public event EventHandler<PlaybackErrorEventArgs> PlaybackError;
        
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
        
        public IWaveProvider PlayStream(string streamUrl)
        {
            try
            {
                StopPlayback();
                
                IWaveProvider provider;
                
                try
                {
                    // Try MediaFoundationReader first - best codec support on Windows
                    provider = new MediaFoundationReader(streamUrl);
                }
                catch (Exception ex) when (ex.Message.Contains("AcmNotPossible") || 
                                         ex.Message.Contains("acmStreamOpen"))
                {
                    // MediaFoundationReader failed, try MP3FileReader with a buffered stream
                    try
                    {
                        var webRequest = WebRequest.Create(streamUrl);
                        var webResponse = webRequest.GetResponse();
                        var responseStream = webResponse.GetResponseStream();
                        
                        // Use a buffered stream to improve playback stability
                        var bufferedStream = new BufferedStream(responseStream, 65536); // 64KB buffer
                        provider = new Mp3FileReader(bufferedStream);
                    }
                    catch (Exception fallbackEx)
                    {
                        // If both approaches failed, wrap the exception with more context
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
                
                // Initialize the output device with the provider
                try
                {
                    outputDevice.Init(provider);
                    outputDevice.Play();
                    currentStreamProvider = provider;
                    
                    OnPlaybackStateChanged(new PlaybackStateChangedEventArgs(PlaybackState.Playing));
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
        
        private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
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
                    outputDevice = null;
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
