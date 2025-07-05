using Microsoft.Win32;

namespace FakeUACWPF
{
    public static class WallpaperUtils
    {
        public static string GetCurrentWallpaperPath()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false))
            {
                return key?.GetValue("WallPaper")?.ToString();
            }
        }
    }
}
