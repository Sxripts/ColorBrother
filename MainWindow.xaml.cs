using Accord;
using System;
using System.Linq;
using System.Windows;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ColorBrother
{
    public partial class MainWindow : Window
    {
        private bool isRightMouseDown = false;
        private readonly Bitmap screenCapture;

        public MainWindow()
        {
            InitializeComponent();
            screenCapture = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        }

        private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => isRightMouseDown = true;

        private void MainWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e) => isRightMouseDown = false;

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (isRightMouseDown)
            {
                // Center of the screen, assuming targetCircle is always in the center
                System.Windows.Point targetCenter = new(SystemParameters.PrimaryScreenWidth / 2, SystemParameters.PrimaryScreenHeight / 2);
                System.Windows.Point currentMousePosition = Mouse.GetPosition(this);

                double distance = (targetCenter - currentMousePosition).Length;

                // Checks if the mouse is within the circle
                if (distance <= targetCircle.Width / 2)
                {
                    System.Windows.Media.Color pixelColor = GetPixelColorAtMousePosition(currentMousePosition);

                    if (pixelColor == System.Windows.Media.Colors.Yellow)
                    {
                        // Move the mouse to the yellow color's position
                        MoveMouseToPosition(new IntPoint((int)currentMousePosition.X, (int)currentMousePosition.Y));
                    }
                }
            }
        }

        private List<IntPoint> GetColorObjectPoints(System.Windows.Point centerPosition, System.Windows.Media.Color targetColor)
        {
            List<IntPoint> colorPoints = new();

            using (Graphics graphics = Graphics.FromImage(screenCapture))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, screenCapture.Size);
            }

            int radius = (int)targetCircle.Width / 2;
            Rectangle cropRect = new((int)centerPosition.X - radius, (int)centerPosition.Y - radius, radius * 2, radius * 2);
            using (Bitmap croppedCapture = screenCapture.Clone(cropRect, screenCapture.PixelFormat))
            {
                // Analyze the pixels within the cropped region to find the target color
                for (int y = 0; y < croppedCapture.Height; y++)
                {
                    for (int x = 0; x < croppedCapture.Width; x++)
                    {
                        Color pixelColor = croppedCapture.GetPixel(x, y);

                        if (pixelColor.R == targetColor.R && pixelColor.G == targetColor.G && pixelColor.B == targetColor.B)
                        {
                            colorPoints.Add(new IntPoint(x + cropRect.X, y + cropRect.Y));
                        }
                    }
                }
            }

            return colorPoints;
        }

        private static System.Windows.Media.Color GetPixelColorAtMousePosition(System.Windows.Point mousePosition)
        {
            IntPtr screenDC = NativeMethods.GetDC(IntPtr.Zero);
            System.Windows.Media.Color pixelColor = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);

            if (screenDC != IntPtr.Zero)
            {
                int x = (int)mousePosition.X;
                int y = (int)mousePosition.Y;

                int color = NativeMethods.GetPixel(screenDC, x, y);
                pixelColor = System.Windows.Media.Color.FromArgb((byte)(color >> 16), (byte)(color >> 8), (byte)color, 0);

                _ = NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);
            }

            return pixelColor;
        }

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        private void MoveMouseToPosition(IntPoint position)
        {
            System.Windows.Point globalPosition = this.PointToScreen(new System.Windows.Point(position.X, position.Y));
            SetCursorPos((int)globalPosition.X, (int)globalPosition.Y);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);
    }
}