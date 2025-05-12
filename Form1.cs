using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using MaterialSkin.Controls;
using MaterialSkin;
using NAudio.Wave;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DesktopWebRadio.Models;
using DesktopWebRadio.Services;
using Newtonsoft.Json.Linq;

namespace DesktopWebRadio
{
    public partial class Form1 : MaterialForm
    {
        private MaterialSkinManager skinManager;
        private WaveOutEvent? waveOut;
        private IWaveProvider? mediaReader;
        private System.Timers.Timer? metadataTimer;
        private string currentStreamUrl = "";
        private string currentStation = "";
        private string currentArtist = "";
        private string currentTitle = "";
        private Thread? metadataThread;
        private bool stopMetadataThread = false;
        private readonly string discogsApiKey = "YOUR_DISCOGS_APIKEY";
        private readonly string discogsApiSecret = "YOUR_DISCOGS_SECRET";
        private readonly string discogsUserAgent = "DesktopWebRadio/1.0";
        private bool useDiscogs = true;
        private readonly string lastfmApiKey = "YOUR_LASTFM_APIKEY";
        private string currentAlbumArtUrl = "";
        private readonly HttpClient httpClient;
        private List<RadioStation>? radioStations;
        private MusicVisualizer musicVisualizer;
        private AudioVisualizationProvider? visualizationProvider;
        private System.Windows.Forms.Timer? visualizerUpdateTimer;
        private readonly System.Windows.Forms.Timer scrollTimer;
        private AlbumArtProvider albumArtProvider;
        private int scrollPosition = 0;
        private string fullMetadataText = "";

        public Form1()
        {
            InitializeComponent();

            // Initialize MaterialSkinManager
            skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            skinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800,
                Primary.BlueGrey900,
                Primary.BlueGrey500,
                Accent.LightBlue200,
                TextShade.WHITE
            );

            // Initialize the HttpClient for API requests
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DesktopWebRadio/1.0");

            // Initialize album art provider
            albumArtProvider = new AlbumArtProvider(discogsApiKey, discogsApiSecret, discogsUserAgent, lastfmApiKey);

            // Set up UI buttons
            materialStopBtn.Text = "STOP";
            materialStopBtn.Enabled = false;

            // Set up volume control
            volumeTrackBar.Minimum = 0;
            volumeTrackBar.Maximum = 100;
            volumeTrackBar.Value = 80;

            // Setup event handlers
            materialPlayBtn.Click += MaterialPlayBtn_Click;
            materialStopBtn.Click += MaterialStopBtn_Click;
            materialThemeBtn.Click += MaterialThemeBtn_Click;
            volumeTrackBar.Scroll += VolumeTrackBar_Scroll;
            stationComboBox.SelectedIndexChanged += StationComboBox_SelectedIndexChanged;
            visualizerModeComboBox.SelectedIndexChanged += VisualizerModeComboBox_SelectedIndexChanged;
            pictureAlbumBox.Click += PictureAlbumBox_Click;

            // Set up visualizer panel
            visualizerPanel.BackColor = Color.FromArgb(255, 14, 22, 33); // Dark navy blue background
            visualizerPanel.BorderStyle = BorderStyle.FixedSingle; // Add border to make it more visible

            // Initialize audio visualizer
            musicVisualizer = new MusicVisualizer(visualizerPanel);
            InitializeVisualizer();

            // Initialize metadata scrolling timer
            scrollTimer = new System.Windows.Forms.Timer();
            scrollTimer.Interval = 150;
            scrollTimer.Tick += ScrollTimer_Tick;

            // Make sure the visualizer panel is visible and properly sized
            visualizerPanel.Visible = true;
            visualizerPanel.BringToFront();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Load radio stations from CSV file
            LoadRadioStations();

            // Set the form title
            this.Text = "Web Radio Player";
        }

        #region Radio Stations

        private void LoadRadioStations()
        {
            try
            {
                radioStations = new List<RadioStation>();
                string csvPath = Path.Combine(Application.StartupPath, "InternetRadio.csv");

                if (File.Exists(csvPath))
                {
                    string[] lines = File.ReadAllLines(csvPath);
                    
                    // Skip header row if it exists
                    bool firstLine = true;
                    foreach (string line in lines)
                    {
                        // Skip header or empty lines
                        if (string.IsNullOrWhiteSpace(line) || (firstLine && line.StartsWith("Station")))
                        {
                            firstLine = false;
                            continue;
                        }

                        string[] parts = line.Split(',');
                        if (parts.Length >= 2)
                        {
                            radioStations.Add(new RadioStation
                            {
                                StationName = parts[0].Trim(),
                                StreamUrl = parts[1].Trim()
                            });
                        }
                        firstLine = false;
                    }

                    // Populate the combo box with station names
                    stationComboBox.Items.Clear();
                    foreach (var station in radioStations)
                    {
                        stationComboBox.Items.Add(station.StationName);
                    }

                    if (stationComboBox.Items.Count > 0)
                    {
                        stationComboBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    MessageBox.Show("Radio stations file not found: " + csvPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading radio stations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StationComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (stationComboBox.SelectedIndex >= 0)
            {
                currentStation = stationComboBox.Text;
                
                // If playback is active, switch to the new station
                if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
                {
                    StopPlayback();
                    _ = StartPlaybackAsync(GetVisualizerUpdateTimer());
                }
            }
        }

        #endregion

        #region Playback Controls

        private void MaterialPlayBtn_Click(object? sender, EventArgs e)
        {
            _ = StartPlaybackAsync(GetVisualizerUpdateTimer());
        }

        private void MaterialStopBtn_Click(object? sender, EventArgs e)
        {
            StopPlayback();
        }

        private System.Windows.Forms.Timer? GetVisualizerUpdateTimer()
        {
            return visualizerUpdateTimer;
        }

        private async Task StartPlaybackAsync(System.Windows.Forms.Timer? visualizerUpdateTimer)
        {
            try
            {
                if (stationComboBox.SelectedIndex < 0 || radioStations == null)
                {
                    MessageBox.Show("Please select a station first.");
                    return;
                }

                // Get selected station
                var selectedStation = radioStations[stationComboBox.SelectedIndex];
                currentStreamUrl = selectedStation.StreamUrl;

                // Stop existing playback if any
                StopPlayback(false);

                // Initialize WaveOut device
                waveOut = new WaveOutEvent();
                waveOut.Volume = volumeTrackBar.Value / 100f;

                try
                {
                    // Try MediaFoundationReader first (newer API with better codec support)
                    mediaReader = new MediaFoundationReader(currentStreamUrl);
                }
                catch (Exception ex)
                {
                    // If MediaFoundationReader fails, try MP3FileReader with a buffered web stream
                    if (ex.Message.Contains("AcmNotPossible") || ex.Message.Contains("acmStreamOpen"))
                    {
                        try
                        {
                            // Create a buffered stream from the URL
                            using var response = await httpClient.GetAsync(currentStreamUrl, HttpCompletionOption.ResponseHeadersRead);
                            response.EnsureSuccessStatusCode();
                            var responseStream = await response.Content.ReadAsStreamAsync();

                            // Try using MP3FileReader with stream buffering
                            var bufferedStream = new BufferedStream(responseStream, 65536); // 64KB buffer
                            mediaReader = new Mp3FileReader(bufferedStream);
                        }
                        catch (Exception fallbackEx)
                        {
                            throw new Exception($"Failed to open audio stream after fallback attempt: {fallbackEx.Message}", fallbackEx);
                        }
                    }
                    else
                    {
                        throw; // Re-throw if it's not an ACM error
                    }
                }

                // Initialize visualization provider with error handling
                try
                {
                    visualizationProvider = new AudioVisualizationProvider(mediaReader);

                    // Connect the audio stream to the WaveOut device
                    waveOut.Init(visualizationProvider);
                    waveOut.Play();

                    // Start metadata monitoring
                    StartMetadataMonitoring();

                    // Update UI state
                    if (materialPlayBtn != null && materialStopBtn != null && visualizerUpdateTimer != null)
                    {
                        materialPlayBtn.Enabled = false;
                        materialStopBtn.Enabled = true;
                        visualizerUpdateTimer.Start();
                    }

                    // Update the label with the station name
                    UpdateMetadataDisplay(currentStation, "", "");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error setting up audio visualization: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting playback: {ex.Message}", "Playback Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                StopPlayback();
            }
        }

        private void StopPlayback(bool updateUI = true)
        {
            stopMetadataThread = true;
            
            if (metadataThread != null && metadataThread.IsAlive)
            {
                metadataThread.Join(1000); // Wait for thread to finish
                metadataThread = null;
            }

            if (metadataTimer != null)
            {
                metadataTimer.Stop();
                metadataTimer.Dispose();
                metadataTimer = null;
            }

            if (visualizerUpdateTimer != null)
            {
                visualizerUpdateTimer.Stop();
            }

            if (scrollTimer != null && scrollTimer.Enabled)
            {
                scrollTimer.Stop();
            }

            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
                waveOut.Dispose();
                waveOut = null;
            }

            if (mediaReader != null)
            {
                if (mediaReader is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                mediaReader = null;
            }

            if (visualizationProvider != null)
            {
                if (visualizationProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                visualizationProvider = null;
            }

            if (updateUI)
            {
                materialPlayBtn.Enabled = true;
                materialStopBtn.Enabled = false;
                labelMetadata.Text = "Ready";
                pictureAlbumBox.Image = null;
                scrollPosition = 0;
                fullMetadataText = "";
            }
        }

        private void VolumeTrackBar_Scroll(object? sender, EventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.Volume = volumeTrackBar.Value / 100f;
            }
        }

        #endregion

        #region Metadata Handling

        private void StartMetadataMonitoring()
        {
            stopMetadataThread = false;
            metadataThread = new Thread(MetadataMonitoringTask);
            metadataThread.IsBackground = true;
            metadataThread.Start();
        }

        private void MetadataMonitoringTask()
        {
            try
            {
                while (!stopMetadataThread)
                {
                    try
                    {
                        // Usa HttpClient invece di HttpWebRequest
                        using (var request = new HttpRequestMessage(HttpMethod.Get, currentStreamUrl))
                        {
                            // Aggiungi l'header per i metadati Icy
                            request.Headers.Add("Icy-MetaData", "1");
                            request.Headers.Add("User-Agent", "DesktopWebRadio/1.0");

                            // Imposta un timeout per la richiesta
                            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                            using (var response = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).GetAwaiter().GetResult())
                            {
                                response.EnsureSuccessStatusCode();

                                // Ottieni gli header Icy dalla risposta
                                // Ottieni gli header Icy dalla risposta
                                // In the MetadataMonitoringTask method
                                // Ottieni gli header Icy dalla risposta
                                if (response.Headers.TryGetValues("icy-metaint", out IEnumerable<string>? metaIntValues) &&
                                    metaIntValues != null &&
                                    int.TryParse(metaIntValues.FirstOrDefault(), out int metaInt))

                                {
                                    byte[] buffer = new byte[metaInt];
                                    int metaLength;

                                    using (Stream stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                                    {
                                        if (stream == null) throw new InvalidOperationException("Response stream is null");

                                        try
                                        {
                                            // Skip initial audio data
                                            int bytesRead = 0;
                                            int bytesToRead = buffer.Length;

                                            // Read in chunks to prevent hanging on slow connections
                                            while (bytesRead < bytesToRead && !stopMetadataThread)
                                            {
                                                int chunkSize = Math.Min(4096, bytesToRead - bytesRead);
                                                int read = stream.Read(buffer, bytesRead, chunkSize);

                                                if (read == 0) break; // End of stream
                                                bytesRead += read;
                                            }

                                            if (bytesRead < bytesToRead)
                                            {
                                                // Incomplete read - stream likely ended
                                                throw new EndOfStreamException("Stream ended before reading complete buffer");
                                            }

                                            // Read metadata length byte
                                            metaLength = stream.ReadByte() * 16;

                                            if (metaLength > 0)
                                            {
                                                // Read metadata
                                                byte[] metadataBuffer = new byte[metaLength];
                                                bytesRead = 0;
                                                bytesToRead = metadataBuffer.Length;

                                                // Read metadata in chunks
                                                while (bytesRead < bytesToRead && !stopMetadataThread)
                                                {
                                                    int chunkSize = Math.Min(1024, bytesToRead - bytesRead);
                                                    int read = stream.Read(metadataBuffer, bytesRead, chunkSize);

                                                    if (read == 0) break; // End of stream
                                                    bytesRead += read;
                                                }

                                                if (bytesRead < bytesToRead)
                                                {
                                                    // Incomplete metadata read
                                                    throw new EndOfStreamException("Stream ended before reading complete metadata");
                                                }

                                                // Convert metadata to string
                                                string metadata = Encoding.ASCII.GetString(metadataBuffer);

                                                // Parse StreamTitle
                                                Match match = Regex.Match(metadata, @"StreamTitle='(.*?)(';|')");
                                                if (match.Success)
                                                {
                                                    string streamTitle = match.Groups[1].Value;
                                                    string artist = "", title = "";

                                                    // Try to split into artist and title
                                                    int separatorIndex = streamTitle.IndexOf(" - ");
                                                    if (separatorIndex > 0)
                                                    {
                                                        artist = streamTitle.Substring(0, separatorIndex).Trim();
                                                        title = streamTitle.Substring(separatorIndex + 3).Trim();
                                                    }
                                                    else
                                                    {
                                                        title = streamTitle.Trim();
                                                    }

                                                    // Update metadata display on UI thread
                                                    if (artist != currentArtist || title != currentTitle)
                                                    {
                                                        currentArtist = artist;
                                                        currentTitle = title;

                                                        this.Invoke(new Action(() =>
                                                        {
                                                            // Update display
                                                            UpdateMetadataDisplay(currentStation, currentArtist, currentTitle);

                                                            // Fetch album art
                                                            FetchAlbumArt(currentArtist, currentTitle);
                                                        }));
                                                    }
                                                }
                                            }
                                        }
                                        catch (ThreadAbortException)
                                        {
                                            // Thread was aborted, exit the loop
                                            break;
                                        }
                                        catch (EndOfStreamException eosEx)
                                        {
                                            // Log the error but continue - stream ended unexpectedly
                                            Console.WriteLine($"Stream ended unexpectedly: {eosEx.Message}");
                                        }
                                        catch (IOException ioEx)
                                        {
                                            // Log the error but continue - likely a network issue
                                            Console.WriteLine($"IO Exception in metadata thread: {ioEx.Message}");
                                        }
                                        catch (HttpRequestException httpEx)
                                        {
                                            // Handle HTTP-specific exceptions
                                            Console.WriteLine($"HTTP error in metadata thread: {httpEx.Message}");
                                        }
                                        catch (Exception ex)
                                        {
                                            // Log other exceptions but continue the loop
                                            Console.WriteLine($"Error processing stream: {ex.Message}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        // Thread was aborted, exit the loop
                        break;
                    }
                    catch (HttpRequestException httpEx)
                    {
                        // Handle HTTP-specific exceptions
                        Console.WriteLine($"HTTP error in metadata thread: {httpEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Log other exceptions but continue monitoring
                        Console.WriteLine($"Error in metadata thread: {ex.Message}");
                    }

                    // Wait before the next check with periodic cancellation checks
                    for (int i = 0; i < 50 && !stopMetadataThread; i++)
                    {
                        Thread.Sleep(100); // Check for stop every 100ms
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Thread was aborted, just exit
            }
            catch (Exception ex)
            {
                // Log fatal errors
                Console.WriteLine($"Fatal error in metadata thread: {ex.Message}");
            }
        }

        private void UpdateMetadataDisplay(string station, string artist, string title)
        {
            string displayText = station;

            if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
            {
                displayText = $"{station} | {artist} - {title}";
            }
            else if (!string.IsNullOrEmpty(title))
            {
                displayText = $"{station} | {title}";
            }

            fullMetadataText = displayText;
            scrollPosition = 0;

            if (displayText.Length > 40)
            {
                // Start scrolling timer if text is too long
                if (!scrollTimer.Enabled)
                {
                    scrollTimer.Start();
                }
            }
            else
            {
                // Stop scrolling timer if text is short enough
                if (scrollTimer.Enabled)
                {
                    scrollTimer.Stop();
                }
                labelMetadata.Text = displayText;
            }
        }

        private void ScrollTimer_Tick(object? sender, EventArgs e)
        {
            if (fullMetadataText.Length <= 40)
            {
                scrollTimer.Stop();
                labelMetadata.Text = fullMetadataText;
                return;
            }

            // Create scrolling text effect
            int textLength = fullMetadataText.Length;
            string displayText;
            
            if (scrollPosition + 40 > textLength)
            {
                // Wrap around
                displayText = fullMetadataText.Substring(scrollPosition) + " | " + 
                              fullMetadataText.Substring(0, 40 - (textLength - scrollPosition));
            }
            else
            {
                displayText = fullMetadataText.Substring(scrollPosition, 40);
            }

            labelMetadata.Text = displayText;
            scrollPosition = (scrollPosition + 1) % textLength;
        }

        #endregion

        #region Album Art Handling

        private async void FetchAlbumArt(string artist, string title)
        {
            if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
            {
                pictureAlbumBox.Image = null;
                return;
            }

            try
            {
                // First try with current API setting
                string? imageUrl = await albumArtProvider.GetAlbumArtUrl(artist, title, useDiscogs);

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await DownloadAndDisplayImage(imageUrl);
                }
                else
                {
                    // If first API failed, try with the alternate one
                    useDiscogs = !useDiscogs;
                    imageUrl = await albumArtProvider.GetAlbumArtUrl(artist, title, useDiscogs);
                    pictureAlbumBox.Image = null;
                    currentAlbumArtUrl = string.Empty;

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        await DownloadAndDisplayImage(imageUrl);
                    }
                    else
                    {
                        pictureAlbumBox.Image = null;
                        currentAlbumArtUrl = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching album art: {ex.Message}");
                pictureAlbumBox.Image = null;
                currentAlbumArtUrl = string.Empty;
            }
        }

        private async Task DownloadAndDisplayImage(string imageUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set a timeout to prevent hanging
                    client.Timeout = TimeSpan.FromSeconds(10);

                    byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        Image albumArt = Image.FromStream(ms);
                        pictureAlbumBox.Image = albumArt;
                        currentAlbumArtUrl = imageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image: {ex.Message}");
                throw; // Re-throw to be caught by the calling method
            }
        }


        private void PictureAlbumBox_Click(object? sender, EventArgs e)
        {
            if (pictureAlbumBox.Image != null)
            {
                // Create a new form to display the enlarged album art
                using (Form albumArtForm = new Form())
                {
                    albumArtForm.Text = $"{currentArtist} - {currentTitle}";
                    albumArtForm.Size = new Size(500, 500);
                    albumArtForm.StartPosition = FormStartPosition.CenterParent;
                    albumArtForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    albumArtForm.MaximizeBox = false;

                    PictureBox enlargedPictureBox = new PictureBox
                    {
                        Image = pictureAlbumBox.Image,
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom
                    };

                    albumArtForm.Controls.Add(enlargedPictureBox);
                    albumArtForm.ShowDialog(this);
                }
            }
        }

        #endregion

        #region Theme Handling

        private void MaterialThemeBtn_Click(object? sender, EventArgs e)
        {
            using (ThemeSettingsForm themeForm = new ThemeSettingsForm(skinManager))
            {
                themeForm.ShowDialog();
                // Dopo aver chiuso il form delle impostazioni del tema
                // preserva il colore di sfondo del visualizzatore
                if (musicVisualizer != null)
                {
                    musicVisualizer.PreserveBackgroundColor();
                }
            }
        }

        #endregion

        #region Visualizer

        private void InitializeVisualizer()
        {
            // Initialize visualizer modes
            visualizerModeComboBox.Items.Clear();
            visualizerModeComboBox.Items.AddRange(new object[] { "Bar", "Line", "Spectrum", "None" });
            visualizerModeComboBox.SelectedIndex = 0;

            // Initialize visualizer update timer
            visualizerUpdateTimer = new System.Windows.Forms.Timer();
            visualizerUpdateTimer.Interval = 30; // Update more frequently (33 times per second)
            visualizerUpdateTimer.Tick += VisualizerUpdateTimer_Tick;
            
            // Make sure panel properties are set for best visualization
            visualizerPanel.BackColor = Color.FromArgb(255, 14, 22, 33); // Dark navy blue background
            visualizerPanel.BorderStyle = BorderStyle.FixedSingle;
            
            // Initialize with a clear panel
            visualizerPanel.Invalidate();
        }

        // Update the VisualizerModeComboBox_SelectedIndexChanged method to check for null
        private void VisualizerModeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (musicVisualizer != null && visualizerModeComboBox.SelectedItem != null)
            {
                switch (visualizerModeComboBox.SelectedItem.ToString())
                {
                    case "Bar":
                        musicVisualizer.VisualizerMode = VisualizerMode.Bar;
                        break;
                    case "Line":
                        musicVisualizer.VisualizerMode = VisualizerMode.Line;
                        break;
                    case "Spectrum":
                        musicVisualizer.VisualizerMode = VisualizerMode.Spectrum;
                        break;
                    case "None":
                        musicVisualizer.VisualizerMode = VisualizerMode.None;
                        break;
                }
            }
        }


        // Replace the VisualizerUpdateTimer_Tick method
        private void VisualizerUpdateTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (visualizationProvider != null && musicVisualizer != null)
                {
                    float[]? fftData = visualizationProvider.GetFFTData();
                    if (fftData != null && fftData.Length > 0)
                    {
                        // Apply amplification to make visualization more visible  
                        for (int i = 0; i < fftData.Length; i++)
                        {
                            // Scale low values up significantly for better visibility  
                            // Apply an aggressive amplification with a log curve  
                            fftData[i] = (float)Math.Min(1.0f, Math.Log10(1 + fftData[i] * 50) * 0.8f);
                        }

                        musicVisualizer.UpdateFFTData(fftData);

                        // Force redraw of the panel  
                        visualizerPanel.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                // Just log errors but don't crash on visualization issues  
                Console.WriteLine($"Error updating visualizer: {ex.Message}");
            }
        }


        #endregion

        private void labelMetadata_Click(object sender, EventArgs e)
        {

        }
    }
}
