using System;
using System.Windows.Forms;

namespace GameOfLife
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool wallpaperMode = args.Length > 0 && args[0] == "--wallpaper";

            var form = new mainForm();

            if (wallpaperMode)
            {
                System.Threading.Thread.Sleep(1000);
                form.ToggleWallpaperMode();
            }

            Application.Run(form);
        }
    }
}