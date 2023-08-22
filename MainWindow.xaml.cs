using SharpDX;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ColorBrother
{
    public partial class MainWindow : Window
    {
        private FovWindow _fovWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleFOV_Click(object sender, RoutedEventArgs e)
        {
            if (_fovWindow == null)
            {
                _fovWindow = new FovWindow();
                _fovWindow.Show();
            }
            else
            {
                _fovWindow.Close();
                _fovWindow = null;
            }
        }

    }
}