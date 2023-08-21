using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ColorBrother
{
    public partial class MainWindow : Window
    {
        private readonly IntPtr _hookID = IntPtr.Zero;
        private bool _rightMouseButtonHeld = false;

        public MainWindow()
        {
            InitializeComponent();
            _hookID = SetHook(HookCallback);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using System.Diagnostics.ProcessModule curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_RBUTTONDOWN == (MouseMessages)wParam)
            {
                _rightMouseButtonHeld = true;
            }
            else if (nCode >= 0 && MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam)
            {
                _rightMouseButtonHeld = false;
            }

            if (_rightMouseButtonHeld)
            {
                CheckColorAndMoveMouse();
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void CheckColorAndMoveMouse()
        {
            if (_rightMouseButtonHeld)
            {
                var bitmap = CaptureScreen(targetCircle);
                var targetColor = ColorTranslator.FromHtml("#e5ed02");

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        if (bitmap.GetPixel(x, y) == targetColor)
                        {
                            // Move the mouse to the detected color
                            SetCursorPos(x, y);
                            return;
                        }
                    }
                }
            }
        }

        private static Bitmap CaptureScreen(FrameworkElement element)
        {
            var point = element.PointToScreen(new System.Windows.Point(0, 0));
            var rect = new Rectangle((int)point.X, (int)point.Y, (int)element.ActualWidth, (int)element.ActualHeight);
            var bitmap = new Bitmap(rect.Width, rect.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
            }
            return bitmap;
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }
    }
}