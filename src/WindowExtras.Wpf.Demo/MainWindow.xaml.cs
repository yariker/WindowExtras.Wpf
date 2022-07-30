using System.Windows;
using System.Windows.Navigation;

namespace WindowExtras.Wpf.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowShadowDemo? _shadowDemo;
        private SystemMenuDemo? _systemMenuDemo;
        private ScreenDemo? _screenDemo;
        private SystemCommandsDemo? _systemCommandsDemo;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnShadowDemoClick(object sender, RequestNavigateEventArgs e)
        {
            CreateOrActivate(ref _shadowDemo);
        }

        private void OnSystemMenuDemoClick(object sender, RequestNavigateEventArgs e)
        {
            CreateOrActivate(ref _systemMenuDemo);
        }

        private void OnScreenDemoClick(object sender, RequestNavigateEventArgs e)
        {
            CreateOrActivate(ref _screenDemo);
        }

        private void OnSystemCommandsClick(object sender, RequestNavigateEventArgs e)
        {
            CreateOrActivate(ref _systemCommandsDemo);
        }

        private void CreateOrActivate<T>(ref T? window) where T: Window, new()
        {
            if (window?.IsVisible == true)
            {
                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }

                window.Activate();
            }
            else
            {
                window = new T { Owner = this };
                window.ShowDialog();
            }
        }
    }
}
