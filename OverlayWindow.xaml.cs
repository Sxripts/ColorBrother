using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using System.Drawing;
using System.Windows.Input;
using System.Threading;
using SharpDX.Direct3D;
using System.Windows.Interop;
using System.Windows.Controls;
using SharpDX;
using SharpDX.D3DCompiler;

namespace ColorBrother
{
    public partial class OverlayWindow : Window
    {
        private readonly Timer _mouseControlTimer;
        private Device _device;
        private SwapChain _swapChain;
        private RenderTargetView _renderTargetView;

        // Importing WinAPI functions for mouse control
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        public OverlayWindow()
        {
            InitializeComponent();
            // Set the window to be always on top
            Topmost = true;
            // Center the window on the screen
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Initialize a timer to call HandleMouseControl every 100 milliseconds
            _mouseControlTimer = new Timer(HandleMouseControl, null, 0, 100);

            // Initialize Direct3D11 rendering
            InitializeRendering();

            // TODO: Initialize any required resources for color detection and rendering
        }

        // Method to detect the specific yellow color within the FOV
        private static System.Drawing.Point? DetectYellowColor()
        {
            // Capture the screen or FOV as a Bitmap
            Bitmap screenBitmap = CaptureScreen();

            // Search for the yellow color within the captured screen
            System.Drawing.Point? detectedColorPosition = FindDetectedColorPosition(screenBitmap);

            return detectedColorPosition;
        }

        // Method to capture the screen or FOV as a Bitmap
        private static Bitmap CaptureScreen()
        {
            // Define the dimensions of the screen or FOV
            int width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            // Create a Bitmap to hold the captured image
            Bitmap screenBitmap = new(width, height);

            // Create a Graphics object from the Bitmap
            using (Graphics graphics = Graphics.FromImage(screenBitmap))
            {
                // Copy the entire screen or FOV to the Bitmap
                graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(width, height));
            }

            return screenBitmap;
        }

        // Method to find the coordinates of the detected color within the captured image
        private static System.Drawing.Point? FindDetectedColorPosition(Bitmap image)
        {
            // Define the specific yellow color range
            int redMin = 230, redMax = 255;
            int greenMin = 220, greenMax = 255;
            int blueMin = 0, blueMax = 75;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);

                    if (pixelColor.R >= redMin && pixelColor.R <= redMax &&
                        pixelColor.G >= greenMin && pixelColor.G <= greenMax &&
                        pixelColor.B >= blueMin && pixelColor.B <= blueMax)
                    {
                        return new System.Drawing.Point(x, y);
                    }
                }
            }

            return null; // Return null if the color is not found
        }

        // Method to handle mouse control (now with an object parameter to match the Timer callback signature)
        private void HandleMouseControl(object state)
        {
            // Check if the right mouse button is pressed
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                // Detect the specific yellow color within the FOV
                System.Drawing.Point? detectedColorPosition = DetectYellowColor();

                // If the color is detected, move the cursor to the detected color location
                if (detectedColorPosition.HasValue)
                {
                    SetCursorPos(detectedColorPosition.Value.X, detectedColorPosition.Value.Y);
                }
            }
        }

        // You can call the HandleMouseControl method continuously, for example, using a timer or in a loop, to keep checking the mouse button state and handle the cursor movement accordingly.

        private void InitializeRendering()
        {
            // Create the swap chain description
            SwapChainDescription swapChainDesc = new()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription((int)this.Width, (int)this.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = new WindowInteropHelper(this).Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create the Direct3D11 device and the swap chain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out _device, out _swapChain);

            // Create the render target view
            using (Texture2D backBuffer = _swapChain.GetBackBuffer<Texture2D>(0))
            {
                _renderTargetView = new RenderTargetView(_device, backBuffer);
            }

            // Set the render target
            _device.ImmediateContext.OutputMerger.SetRenderTargets(_renderTargetView);

            // Set up the viewport
            _device.ImmediateContext.Rasterizer.SetViewport(new Viewport(0, 0, (int)this.Width, (int)this.Height));

            // Create a basic pixel shader to render a solid color
            using (var pixelShaderByteCode = ShaderBytecode.Compile("float4 main() : SV_TARGET { return float4(1, 0, 0, 1); }", "main", "ps_4_0", ShaderFlags.None, EffectFlags.None))
            {
                var pixelShader = new PixelShader(_device, pixelShaderByteCode);
                _device.ImmediateContext.PixelShader.Set(pixelShader);
            }

            // TODO: Create geometry for the circle and render it
        }

        // TODO: Implement the rendering logic for the overlay, such as drawing a circle or other shapes
        // You can use the _device and _renderTargetView to render the overlay
    }

    // Struct for WinAPI cursor position
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }
}
