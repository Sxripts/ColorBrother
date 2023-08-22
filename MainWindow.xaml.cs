using System.Windows;

namespace ColorBrother
{
    public partial class MainWindow : Window
    {
        private CrosshairWindow _crosshairWindow;

        public MainWindow()
        {
            InitializeComponent();
            _crosshairWindow = new CrosshairWindow();
        }

        private void OnToggleCrosshair(object sender, RoutedEventArgs e)
        {
            if (_crosshairWindow.IsVisible)
                _crosshairWindow.Hide();
            else
                _crosshairWindow.Show();
        }
    }
}