using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ColorBrother
{
    public partial class FovWindow : Window
    {
        private readonly DispatcherTimer _timer;

        public FovWindow() // Corrected constructor name
        {
            InitializeComponent();
            PositionOverlay();
            MonitorRegion();
        }

        private void PositionOverlay()
        {
            // Center the overlay on the screen
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
        }

        private bool IsColorMatch(Color color1, Color color2)
        {
            int redDifference = color1.R - color2.R;
            int greenDifference = color1.G - color2.G;
            int blueDifference = color1.B - color2.B;
            double distance = Math.Sqrt(redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference);
            const double threshold = 50.0;
            return distance < threshold;
        }

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateEllipticRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("user32.dll")]
        private static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);

        private Timer _monitorTimer;
        private Color _previousColor;

        private void MonitorRegion()
        {
            // Define the region to monitor (adjust the coordinates as needed)
            IntPtr region = CreateEllipticRgn((int)Left, (int)Top, (int)(Left + Width), (int)(Top + Height));

            // Get the device context for the screen
            IntPtr hdc = GetDC(IntPtr.Zero);

            // Select the region into the device context
            SelectObject(hdc, region);

            // Initialize the previous color
            _previousColor = GetPixelColor(hdc, (int)Left + 50, (int)Top + 50);

            // Start the timer to monitor the region for changes
            _monitorTimer = new Timer(MonitorRegionCallback, hdc, 0, 100); // Check every 100 milliseconds
        }

        private void MonitorRegionCallback(object state)
        {
            IntPtr hdc = (IntPtr)state;

            // Check the color of a pixel within the region
            Color currentColor = GetPixelColor(hdc, (int)Left + 50, (int)Top + 50);

            // Check if the color has changed
            if (currentColor != _previousColor)
            {
                // The color has changed, so check if it matches the target color
                Color targetColor = ColorTranslator.FromHtml("#e5ed02");
                if (IsColorMatch(currentColor, targetColor))
                {
                    // Move the mouse to the target color's position
                    SetCursorPos((int)Left + 50, (int)Top + 50);
                }

                // Update the previous color
                _previousColor = currentColor;
            }
        }

        private Color GetPixelColor(IntPtr hdc, int x, int y)
        {
            int colorRef = GetPixel(hdc, x, y);
            int red = colorRef & 0xFF;
            int green = (colorRef >> 8) & 0xFF;
            int blue = (colorRef >> 16) & 0xFF;
            return Color.FromArgb(red, green, blue);
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
    }
}
