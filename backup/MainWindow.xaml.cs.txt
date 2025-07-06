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
            // Load the current wallpaper
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

            // Play the fake UAC sound
            PlayUACSound();

            // Delay to simulate loading
            await Task.Delay(1500);
        }

        private void PlayUACSound()
        {
            string systemPath = @"C:\Windows\Media\dm\Windows User Account Control.wav";
            string fallbackPath = Path.Combine("resources", "uac.wav");
            string chosenPath = File.Exists(systemPath) ? systemPath : fallbackPath;

            try
            {
                using var player = new SoundPlayer(chosenPath);
                player.Play();
            }
            catch
            {
                // If audio fails, just ignore
            }
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            UACBox.Visibility = Visibility.Collapsed;
            JumpscareVideo.Visibility = Visibility.Visible;

            PlayUACSound();

            string videoPath = Path.Combine("resources", "jumpscare_cut.mp4");

            if (File.Exists(videoPath))
            {
                JumpscareVideo.Source = new Uri(Path.GetFullPath(videoPath));
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
