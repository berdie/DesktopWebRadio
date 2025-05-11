using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopWebRadio
{
    public partial class ThemeSettingsForm : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;

        public ThemeSettingsForm(MaterialSkinManager manager)
        {
            InitializeComponent();

            materialSkinManager = manager;
            InitializeThemeOptions();
        }

        private void InitializeThemeOptions()
        {
            // Initialize theme options
            cmbTheme.Items.Clear();
            cmbTheme.Items.Add("Light");
            cmbTheme.Items.Add("Dark");
            cmbTheme.SelectedIndex = materialSkinManager.Theme == MaterialSkinManager.Themes.LIGHT ? 0 : 1;

            // Initialize primary color options
            cmbPrimaryColor.Items.Clear();
            cmbPrimaryColor.Items.AddRange(new string[] { 
                "DeepOrange", "Blue", "Red", "Green", 
                "Purple", "Indigo", "Teal" 
            });

            // Find the current primary color index based on color comparison
            Color currentPrimaryColor = materialSkinManager.ColorScheme.PrimaryColor;
            cmbPrimaryColor.SelectedIndex = GetSelectedPrimaryColorIndex(currentPrimaryColor);

            // Initialize accent color options
            cmbAccentColor.Items.Clear();
            cmbAccentColor.Items.AddRange(new string[] { 
                "Yellow", "Blue", "Red", "Green", 
                "Purple", "Orange", "Lime" 
            });

            // Find the current accent color index based on color comparison
            Color currentAccentColor = materialSkinManager.ColorScheme.AccentColor;
            cmbAccentColor.SelectedIndex = GetSelectedAccentColorIndex(currentAccentColor);
        }

        private int GetSelectedPrimaryColorIndex(Color currentColor)
        {
            // Default to first option (DeepOrange)
            int selectedIndex = 0;
            
            // Create a temporary ColorScheme to compare colors
            ColorScheme tempScheme;
            
            // Compare with Blue
            tempScheme = new ColorScheme(Primary.Blue500, Primary.Blue700, Primary.Blue800, Accent.Yellow700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.PrimaryColor)) return 1;
            
            // Compare with Red
            tempScheme = new ColorScheme(Primary.Red500, Primary.Red700, Primary.Red800, Accent.Yellow700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.PrimaryColor)) return 2;
            
            // Compare with Green
            tempScheme = new ColorScheme(Primary.Green500, Primary.Green700, Primary.Green800, Accent.Yellow700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.PrimaryColor)) return 3;
            
            // Compare with Purple
            tempScheme = new ColorScheme(Primary.Purple500, Primary.Purple700, Primary.Purple800, Accent.Yellow700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.PrimaryColor)) return 4;
            
            // Compare with Indigo
            tempScheme = new ColorScheme(Primary.Indigo500, Primary.Indigo700, Primary.Indigo800, Accent.Yellow700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.PrimaryColor)) return 5;
            
            // Compare with Teal
            tempScheme = new ColorScheme(Primary.Teal500, Primary.Teal700, Primary.Teal800, Accent.Yellow700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.PrimaryColor)) return 6;
            
            return selectedIndex; // DeepOrange
        }

        private int GetSelectedAccentColorIndex(Color currentColor)
        {
            // Default to first option (Yellow)
            int selectedIndex = 0;
            
            // Create a temporary ColorScheme to compare colors
            ColorScheme tempScheme;
            
            // Compare with Blue
            tempScheme = new ColorScheme(Primary.DeepOrange500, Primary.DeepOrange700, Primary.DeepOrange800, Accent.Blue700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.AccentColor)) return 1;
            
            // Compare with Red
            tempScheme = new ColorScheme(Primary.DeepOrange500, Primary.DeepOrange700, Primary.DeepOrange800, Accent.Red700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.AccentColor)) return 2;
            
            // Compare with Green
            tempScheme = new ColorScheme(Primary.DeepOrange500, Primary.DeepOrange700, Primary.DeepOrange800, Accent.Green700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.AccentColor)) return 3;
            
            // Compare with Purple
            tempScheme = new ColorScheme(Primary.DeepOrange500, Primary.DeepOrange700, Primary.DeepOrange800, Accent.Purple700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.AccentColor)) return 4;
            
            // Compare with Orange
            tempScheme = new ColorScheme(Primary.DeepOrange500, Primary.DeepOrange700, Primary.DeepOrange800, Accent.Orange700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.AccentColor)) return 5;
            
            // Compare with Lime
            tempScheme = new ColorScheme(Primary.DeepOrange500, Primary.DeepOrange700, Primary.DeepOrange800, Accent.Lime700, TextShade.WHITE);
            if (ColorEquals(currentColor, tempScheme.AccentColor)) return 6;
            
            return selectedIndex; // Yellow
        }
        
        // Helper method to compare colors
        private bool ColorEquals(Color color1, Color color2)
        {
            return color1.ToArgb() == color2.ToArgb();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            // Apply theme settings
            materialSkinManager.Theme = cmbTheme.SelectedIndex == 0 
                ? MaterialSkinManager.Themes.LIGHT 
                : MaterialSkinManager.Themes.DARK;

            // Get selected primary color
            Primary primaryColor;
            Primary darkPrimaryColor;
            Primary lightPrimaryColor;
            
            switch (cmbPrimaryColor.SelectedIndex)
            {
                case 1: // Blue
                    primaryColor = Primary.Blue500;
                    darkPrimaryColor = Primary.Blue700;
                    lightPrimaryColor = Primary.Blue800;
                    break;
                case 2: // Red
                    primaryColor = Primary.Red500;
                    darkPrimaryColor = Primary.Red700;
                    lightPrimaryColor = Primary.Red800;
                    break;
                case 3: // Green
                    primaryColor = Primary.Green500;
                    darkPrimaryColor = Primary.Green700;
                    lightPrimaryColor = Primary.Green800;
                    break;
                case 4: // Purple
                    primaryColor = Primary.Purple500;
                    darkPrimaryColor = Primary.Purple700;
                    lightPrimaryColor = Primary.Purple800;
                    break;
                case 5: // Indigo
                    primaryColor = Primary.Indigo500;
                    darkPrimaryColor = Primary.Indigo700;
                    lightPrimaryColor = Primary.Indigo800;
                    break;
                case 6: // Teal
                    primaryColor = Primary.Teal500;
                    darkPrimaryColor = Primary.Teal700;
                    lightPrimaryColor = Primary.Teal800;
                    break;
                default: // DeepOrange (default)
                    primaryColor = Primary.DeepOrange500;
                    darkPrimaryColor = Primary.DeepOrange700;
                    lightPrimaryColor = Primary.DeepOrange800;
                    break;
            }

            // Get selected accent color
            Accent accentColor;
            switch (cmbAccentColor.SelectedIndex)
            {
                case 1: // Blue
                    accentColor = Accent.Blue700;
                    break;
                case 2: // Red
                    accentColor = Accent.Red700;
                    break;
                case 3: // Green
                    accentColor = Accent.Green700;
                    break;
                case 4: // Purple
                    accentColor = Accent.Purple700;
                    break;
                case 5: // Orange
                    accentColor = Accent.Orange700;
                    break;
                case 6: // Lime
                    accentColor = Accent.Lime700;
                    break;
                default: // Yellow (default)
                    accentColor = Accent.Yellow700;
                    break;
            }

            // Apply color scheme
            var textShade = materialSkinManager.Theme == MaterialSkinManager.Themes.LIGHT 
                ? TextShade.BLACK 
                : TextShade.WHITE;
            
            materialSkinManager.ColorScheme = new ColorScheme(
                primaryColor, 
                darkPrimaryColor, 
                lightPrimaryColor, 
                accentColor, 
                textShade
            );

            // Close the form
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
