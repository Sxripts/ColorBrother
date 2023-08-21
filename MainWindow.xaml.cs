using SharpDX;
using System;
using System.Drawing;
using System.Drawing.Imaging;
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
            var FovWindow = new FovWindow();
            FovWindow.Show();
        }

        private void ShowFovWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var FovWindow = new FovWindow();
            FovWindow.Show();
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

        private static bool ColorsAreClose(Color color1, Color color2, int tolerance = 30)
        {
            return Math.Abs(color1.R - color2.R) <= tolerance &&
                   Math.Abs(color1.G - color2.G) <= tolerance &&
                   Math.Abs(color1.B - color2.B) <= tolerance;
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
                        if (ColorsAreClose(bitmap.GetPixel(x, y), targetColor))
                        {
                            SetCursorPos(x, y);
                            return;
                        }
                    }
                }
            }
        }

        private static Bitmap? CaptureScreen(FrameworkElement element)
        {
            var point = element.PointToScreen(new System.Windows.Point(0, 0));
            var rect = new Rectangle((int)point.X, (int)point.Y, (int)element.ActualWidth, (int)element.ActualHeight);

            // Create DXGI Factory
            var factory = new SharpDX.DXGI.Factory1();
            var adapter = factory.GetAdapter1(0);
            var device = new SharpDX.Direct3D11.Device(adapter);
            var output = adapter.GetOutput(0);
            var output1 = output.QueryInterface<SharpDX.DXGI.Output1>();

            // Create Staging texture CPU-accessible
            var textureDesc = new SharpDX.Direct3D11.Texture2DDescription()
            {
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read,
                BindFlags = SharpDX.Direct3D11.BindFlags.None,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Width = rect.Width,
                Height = rect.Height,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = SharpDX.Direct3D11.ResourceUsage.Staging
            };

            // Duplicate the output
            using var duplicatedOutput = output1.DuplicateOutput(device);

            // Try to get duplicated frame within given time
            var result = duplicatedOutput.TryAcquireNextFrame(1000, out SharpDX.DXGI.OutputDuplicateFrameInformation duplicateFrameInformation, out SharpDX.DXGI.Resource screenResource);
            if (result.Failure)
            {
                // Handle failure to acquire next frame
                return null;
            }

            // Copy resource into memory that can be accessed by the CPU
            using var screenTexture = screenResource.QueryInterface<SharpDX.Direct3D11.Texture2D>();
            using var stagingTexture = new SharpDX.Direct3D11.Texture2D(device, textureDesc);
            device.ImmediateContext.CopyResource(screenTexture, stagingTexture);

            // Get the desktop capture texture
            var mapSource = device.ImmediateContext.MapSubresource(stagingTexture, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

            // Create a bitmap to store the captured region
            var bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);

            // Copy pixels from screen capture Texture to GDI bitmap
            var boundsRect = new System.Drawing.Rectangle(0, 0, rect.Width, rect.Height);
            var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var sourcePtr = mapSource.DataPointer;
            var destPtr = mapDest.Scan0;
            for (int y = 0; y < rect.Height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, rect.Width * 4);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            // Release source and dest locks
            bitmap.UnlockBits(mapDest);
            device.ImmediateContext.UnmapSubresource(stagingTexture, 0);

            // Release all resources
            screenResource.Dispose();
            duplicatedOutput.ReleaseFrame();

            return bitmap;
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private void UpdateTargetCircle()
        {
            // Find the target window by its title (replace with the actual title)
            IntPtr targetWindowHandle = FindWindow(null, "Target Window Title");
            if (targetWindowHandle == IntPtr.Zero) return;

            // Get the target window's position and size
            GetWindowRect(targetWindowHandle, out RECT rect);

            // Update the targetCircle's position and size to match the target window
            targetCircle.Width = rect.Right - rect.Left;
            targetCircle.Height = rect.Bottom - rect.Top;
            targetCircle.Margin = new Thickness(rect.Left, rect.Top, 0, 0);
        }

        // Windows API to find a window by its class name and title
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        // Windows API to get a window's position and size
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        // Structure to hold the position and size of a window
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

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