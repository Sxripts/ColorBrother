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
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
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
                var screenBitmap = CaptureScreen((int)Left, (int)Top, (int)Width, (int)Height);
                var targetColor = ColorTranslator.FromHtml("#e5ed02"); // Target color

                for (int y = 0; y < screenBitmap.Height; y++)
                {
                    for (int x = 0; x < screenBitmap.Width; x++)
                    {
                        var pixelColor = screenBitmap.GetPixel(x, y);
                        bool isMatch = IsColorMatch(pixelColor, targetColor); // Check if colors match

                        if (isMatch)
                        {
                            SetCursorPos((int)Left + x, (int)Top + y);
                            return;
                        }
                    }
                }
            }
        }

        private bool IsColorMatch(Color color1, Color color2)
        {
            // Calculate the Euclidean distance between the two colors
            int redDifference = color1.R - color2.R;
            int greenDifference = color1.G - color2.G;
            int blueDifference = color1.B - color2.B;
            double distance = Math.Sqrt(redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference);

            // Define a threshold for what constitutes a "close match"
            // You can adjust this value based on your specific requirements
            const double threshold = 50.0;

            return distance < threshold;
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
