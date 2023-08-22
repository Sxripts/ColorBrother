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

        public OverlayWindow()
        {
            InitializeComponent();
            PositionOverlay();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // Adjust as needed
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void PositionOverlay()
        {
            // Center the overlay on the screen
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                // Capture the screen within the target circle
                var screenBitmap = CaptureScreen((int)Left, (int)Top, (int)Width, (int)Height);

                // Check for the specified color
                var targetColor = ColorTranslator.FromHtml("#e5ed02");
                for (int y = 0; y < screenBitmap.Height; y++)
                {
                    for (int x = 0; x < screenBitmap.Width; x++)
                    {
                        var pixelColor = screenBitmap.GetPixel(x, y);
                        if (IsColorMatch(pixelColor, targetColor))
                        {
                            // Move the mouse to the color
                            SetCursorPos((int)Left + x, (int)Top + y);
                            return;
                        }
                    }
                }
            }
        }

        private bool IsColorMatch(Color color1, Color color2)
        {
            // Implement logic to determine if the colors are a close match
            // This could involve checking the Euclidean distance between the colors, etc.
            // Adjust the threshold as needed
            return Math.Abs(color1.R - color2.R) < 10 &&
                   Math.Abs(color1.G - color2.G) < 10 &&
                   Math.Abs(color1.B - color2.B) < 10;
        }

        private Bitmap CaptureScreen(int left, int top, int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));
            }
            return bitmap;
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
    }
}
