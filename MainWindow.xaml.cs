using System.Windows;

namespace ColorBrother
{
    public partial class MainWindow : Window
    {
        private readonly OverlayWindow _overlayWindow;

        public MainWindow()
        {
            InitializeComponent();
            _overlayWindow = new OverlayWindow();
        }

        private void ToggleOverlay(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow.IsVisible)
                _overlayWindow.Hide();
            else
                _overlayWindow.Show();
        }
    }
}