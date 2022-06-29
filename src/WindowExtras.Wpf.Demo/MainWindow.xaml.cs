using System.Windows;
using System.Windows.Navigation;

namespace WindowExtras.Wpf.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShadowDemo? _shadowDemo;
        private SystemMenuDemo? _systemMenuDemo;

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

        private static void CreateOrActivate<T>(ref T? window) where T: Window, new()
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
                window = new T();
                window.Show();
            }
        }
    }
}
