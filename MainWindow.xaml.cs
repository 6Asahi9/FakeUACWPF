using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace FakeUACWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string themeDict = IsDarkTheme() ? "Themes/Dark.xaml" : "Themes/Light.xaml";

            var dictionary = new ResourceDictionary
            {
                Source = new Uri(themeDict, UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dictionary);

            try
            {
                string wallpaperPath = null;
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\\Desktop"))
                {
                    wallpaperPath = key?.GetValue("WallPaper")?.ToString();
                }

                if (!string.IsNullOrEmpty(wallpaperPath) && File.Exists(wallpaperPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(wallpaperPath);
                    bitmap.EndInit();

                    WallpaperImage.Source = bitmap;
                }
                else
                {
                    this.Background = Brushes.Black;
                }
            }
            catch
            {
                this.Background = Brushes.Black;
            }

            PlayUACSound();

            await Task.Delay(1500);
        }

        private static bool IsDarkTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object value = key?.GetValue("AppsUseLightTheme");
                    if (value is int intValue)
                        return intValue == 0;
                }
            }
            catch { }

            return true;
        }

        private void PlayUACSound()
        {
            string systemPath = @"C:\Windows\Media\dm\Windows User Account Control.wav";
            // string fallbackPath = Path.Combine("resources", "uac.wav");
            string fallbackPath = Path.Combine(AppContext.BaseDirectory, "resources", "uac.wav");

            string chosenPath = File.Exists(systemPath) ? systemPath : fallbackPath;

            try
            {
                using var player = new SoundPlayer(chosenPath);
                player.Play();
            }
            catch
            {
            }
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            UACBox.Visibility = Visibility.Collapsed;
            JumpscareVideo.Visibility = Visibility.Visible;

            PlayUACSound();

            string videoPath = Path.Combine(AppContext.BaseDirectory, "resources", "jumpscare_cut.mp4");

            if (File.Exists(videoPath))
            {
                // JumpscareVideo.Source = new Uri(Path.GetFullPath(videoPath));
                JumpscareVideo.Source = new Uri(videoPath, UriKind.Absolute);
                JumpscareVideo.Volume = 1.0;
                JumpscareVideo.MediaEnded += (_, _) => Application.Current.Shutdown();
                JumpscareVideo.Play();
            }
            else
            {
                MessageBox.Show("Jumpscare video not found!");
                Application.Current.Shutdown();
            }
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
