using System;
using System.Drawing;
using System.Windows.Forms;
using DesktopWebRadio.Helpers;

namespace DesktopWebRadio.Services
{
    public enum VisualizerMode
    {
        Bar,
        Line,
        Spectrum,
        None
    }

    public class MusicVisualizer
    {
        private readonly Panel visualizerPanel;
        private float[] fftData;
        private VisualizerMode mode;
        private readonly Brush[] barBrushes;
        private readonly Color[] spectrumColors;
        private Color savedBackgroundColor; // Campo per salvare il colore di sfondo


        public VisualizerMode VisualizerMode
        {
            get => mode;
            set => mode = value;
        }

        public MusicVisualizer(Panel panel)
        {
            visualizerPanel = panel;
            fftData = new float[2048];
            mode = VisualizerMode.Bar;
            savedBackgroundColor = panel.BackColor;

            // Initialize brushes for the bars
            barBrushes = new Brush[]
            {
                new SolidBrush(Color.FromArgb(255, 0, 217, 255)),   // Cyan
                new SolidBrush(Color.FromArgb(255, 255, 0, 128)),   // Magenta
                new SolidBrush(Color.FromArgb(255, 50, 255, 50)),   // Bright Green
                new SolidBrush(Color.FromArgb(255, 255, 255, 0)),   // Yellow
                new SolidBrush(Color.FromArgb(255, 220, 130, 255)), // Light Purple
                new SolidBrush(Color.FromArgb(255, 255, 100, 0)),   // Bright Orange
            };

            // Initialize colors for spectrum
            spectrumColors = new Color[]
            {
                Color.FromArgb(255, 0, 0, 255),      // Blue
                Color.FromArgb(255, 0, 128, 255),    // Light Blue
                Color.FromArgb(255, 0, 255, 255),    // Cyan
                Color.FromArgb(255, 0, 255, 128),    // Cyan-Green
                Color.FromArgb(255, 0, 255, 0),      // Green
                Color.FromArgb(255, 128, 255, 0),    // Yellow-Green
                Color.FromArgb(255, 255, 255, 0),    // Yellow
                Color.FromArgb(255, 255, 128, 0),    // Orange
                Color.FromArgb(255, 255, 0, 0)       // Red
            };

            // Add paint handler to panel
            visualizerPanel.Paint += VisualizerPanel_Paint;
            
            // Enable double buffering using the extension method
            visualizerPanel.SetDoubleBuffered(true);
        }

        public void PreserveBackgroundColor()
        {
            // Ripristina il colore di sfondo salvato
            if (visualizerPanel.BackColor != savedBackgroundColor)
            {
                visualizerPanel.BackColor = savedBackgroundColor;
            }
        }

        // Metodo per aggiornare il colore di sfondo salvato
        public void UpdateSavedBackgroundColor(Color color)
        {
            savedBackgroundColor = color;
            visualizerPanel.BackColor = color;
        }

        public void UpdateFFTData(float[] data)
        {
            if (data != null && data.Length > 0)
            {
                fftData = data;
                visualizerPanel.Invalidate();
            }
        }

        private void VisualizerPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int width = visualizerPanel.Width;
            int height = visualizerPanel.Height;
            
            // Clear the panel
            g.Clear(savedBackgroundColor);

            if (fftData == null || fftData.Length == 0 || mode == VisualizerMode.None)
                return;

            switch (mode)
            {
                case VisualizerMode.Bar:
                    DrawBarVisualizer(g, width, height);
                    break;
                case VisualizerMode.Line:
                    DrawLineVisualizer(g, width, height);
                    break;
                case VisualizerMode.Spectrum:
                    DrawSpectrumVisualizer(g, width, height);
                    break;
            }
        }

        private void DrawBarVisualizer(Graphics g, int width, int height)
        {
            int barCount = Math.Min(64, width / 5);  // Max 64 bars or limit by width
            int barWidth = width / barCount;
            int skipFactor = fftData.Length / barCount;
            
            for (int i = 0; i < barCount; i++)
            {
                // Skip some samples for more visible bars
                int sampleIndex = i * skipFactor;
                if (sampleIndex >= fftData.Length) break;
                
                // Scale the FFT value (0.0 to 1.0) to the panel height
                float value = Math.Min(1.0f, fftData[sampleIndex]) * 2;  // Amplify by 2 for better visibility
                int barHeight = (int)(height * value);
                
                // Ensure minimum height for better visual effect
                barHeight = Math.Max(2, barHeight);
                
                // Calculate the position of the bar
                int x = i * barWidth;
                int y = height - barHeight;
                
                // Select a brush based on the bar's index
                Brush brush = barBrushes[i % barBrushes.Length];
                
                // Draw the bar
                g.FillRectangle(brush, x, y, barWidth - 1, barHeight);
            }
        }

        private void DrawLineVisualizer(Graphics g, int width, int height)
        {
            int pointCount = Math.Min(256, width);  // Use up to 256 points or width
            int skipFactor = fftData.Length / pointCount;
            
            // Create points for the line
            Point[] points = new Point[pointCount + 2];
            
            // Start point at the bottom left
            points[0] = new Point(0, height);
            
            // Add points for each FFT value
            for (int i = 0; i < pointCount; i++)
            {
                int sampleIndex = i * skipFactor;
                if (sampleIndex >= fftData.Length) break;
                
                float value = Math.Min(1.0f, fftData[sampleIndex]) * 2;  // Amplify by 2
                int y = height - (int)(height * value);
                
                points[i + 1] = new Point(i * (width / pointCount), y);
            }
            
            // End point at the bottom right
            points[pointCount + 1] = new Point(width, height);
            
            // Create a path and fill it
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddLines(points);
                
                // Create a gradient brush for filling
                using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0, 0),
                    new Point(0, height),
                    Color.FromArgb(200, 0, 120, 255),
                    Color.FromArgb(50, 0, 120, 255)))
                {
                    g.FillPath(brush, path);
                }
                
                // Draw the line on top
                using (Pen pen = new Pen(Color.FromArgb(255, 0, 120, 255), 2))
                {
                    g.DrawLines(pen, points.AsSpan(1, pointCount).ToArray());
                }
            }
        }

        private void DrawSpectrumVisualizer(Graphics g, int width, int height)
        {
            int barCount = Math.Min(128, width / 2);
            int barWidth = width / barCount;
            int skipFactor = fftData.Length / barCount;
            
            // Create a gradient brush for the spectrum effect
            using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new RectangleF(0, 0, width, height),
                Color.Blue,
                Color.Red,
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                // Create color blend for smooth transitions
                System.Drawing.Drawing2D.ColorBlend blend = new System.Drawing.Drawing2D.ColorBlend();
                blend.Colors = spectrumColors;
                blend.Positions = new float[spectrumColors.Length];
                
                // Set positions for the colors
                for (int i = 0; i < spectrumColors.Length; i++)
                {
                    blend.Positions[i] = (float)i / (spectrumColors.Length - 1);
                }
                
                brush.InterpolationColors = blend;
                
                for (int i = 0; i < barCount; i++)
                {
                    int sampleIndex = i * skipFactor;
                    if (sampleIndex >= fftData.Length) break;
                    
                    float value = Math.Min(1.0f, fftData[sampleIndex]) * 1.8f;
                    int barHeight = (int)(height * value);
                    barHeight = Math.Max(2, barHeight);
                    
                    int x = i * barWidth;
                    int y = height - barHeight;
                    
                    g.FillRectangle(brush, x, y, barWidth - 1, barHeight);
                }
            }
        }
    }
}
