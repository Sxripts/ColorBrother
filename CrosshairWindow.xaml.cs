using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;

namespace ColorBrother
{
    public partial class CrosshairWindow : Window
    {
        // Importing the required WinAPI functions to set window attributes
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Constants for the SetWindowLong and SetWindowPos methods
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;
        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        public CrosshairWindow()
        {
            InitializeComponent();

            // Set the window size to match the screen
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            // Subscribe to the window's Loaded event
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Retrieve this window's handle
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            // Make the window non-clickable
            SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_TRANSPARENT | WS_EX_LAYERED);

            // Keep this window above all other applications
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

            // Position the window at the center of the screen
            CenterWindow();

            // Set the window position to cover the entire screen
            this.Left = 0;
            this.Top = 0;

            // Keep this window above all other applications
            SetWindowPos(new WindowInteropHelper(this).Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        private void CenterWindow()
        {
            // Calculate the position to center the window on the screen
            double left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            double top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;

            // Move the window to the calculated position
            this.Left = left;
            this.Top = top;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        // Your specific yellow color
        private readonly Color targetColor = ColorTranslator.FromHtml("#FFFF00");

        // Define your FOV size
        private readonly int fovSize = 50;

        // Continuously track the mouse
        public void TrackMouse()
        {
            // Check if the right mouse button is pressed
            while ((Control.MouseButtons & MouseButtons.Right) != 0)
            {
                if (GetCursorPos(out System.Drawing.Point cursorPosition))
                {
                    // Capture the screen within the FOV
                    Bitmap screenshot = CaptureFOV(cursorPosition, fovSize);

                    // Apply color filtering to find the target color
                    ColorFiltering colorFilter = new ColorFiltering
                    {
                        Red = new IntRange(targetColor.R, targetColor.R),
                        Green = new IntRange(targetColor.G, targetColor.G),
                        Blue = new IntRange(targetColor.B, targetColor.B)
                    };

                    Bitmap processedImage = colorFilter.Apply(screenshot);
                    UnmanagedImage unmanagedImage = UnmanagedImage.FromManagedImage(processedImage);
                    BlobCounter blobCounter = new BlobCounter();
                    blobCounter.ProcessImage(unmanagedImage);
                    Rectangle[] rects = blobCounter.GetObjectsRectangles();

                    // If the color is detected, move the cursor
                    if (rects.Length > 0)
                    {
                        // Snap the cursor to the detected color
                        SetCursorPos(rects[0].X + cursorPosition.X - fovSize / 2, rects[0].Y + cursorPosition.Y - fovSize / 2);
                    }
                }

                // Small delay to prevent excessive CPU usage
                System.Threading.Thread.Sleep(10);
            }
        }

        // Capture the screen within the FOV
        private Bitmap CaptureFOV(System.Drawing.Point center, int size)
        {
            // Define the boundaries of the region to capture
            int left = center.X - size / 2;
            int top = center.Y - size / 2;

            // Create a Bitmap to store the screenshot
            Bitmap screenshot = new Bitmap(size, size);

            // Create a Graphics object to draw the screenshot into the Bitmap
            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                // Capture the screen region defined by the boundaries
                graphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(size, size));
            }

            return screenshot;
        }
    }
}
