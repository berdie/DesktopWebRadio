using System;
using System.Windows.Forms;
using System.Reflection;

namespace DesktopWebRadio.Helpers
{
    /// <summary>  
    /// Extension methods for Windows Forms controls  
    /// </summary>  
    public static class ControlExtensions
    {
        /// <summary>  
        /// Enables double buffering on a control to prevent flickering during redraws  
        /// </summary>  
        /// <param name="control">The control to enable double buffering for</param>  
        public static void SetDoubleBuffered(this Control control, bool enable = true)
        {
            // DoubleBuffered is a protected property, so we need to use reflection to access it  
            PropertyInfo? doubleBufferedProperty =
                control.GetType().GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic);

            if (doubleBufferedProperty != null)
            {
                doubleBufferedProperty.SetValue(control, enable, null);
            }
        }
    }
}
