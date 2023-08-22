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
        private FovWindow? fovWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowFovWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (fovWindow != null)
            {
                fovWindow.Close();
                fovWindow = null;
            }
            else
            {
                fovWindow = new FovWindow();
                fovWindow.Show();
            }
        }
    }
}