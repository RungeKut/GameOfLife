using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GameOfLife
{
    public static class WallpaperHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter,
            string className, string windowTitle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        private static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth,
            int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg,
            IntPtr wParam, IntPtr lParam);

        private const int SW_SHOW = 5;
        private const int WM_SHELLHOOK = 0x052C;

        public static IntPtr GetWorkerW()
        {
            IntPtr progman = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Progman", null);
            SendMessage(progman, WM_SHELLHOOK, IntPtr.Zero, IntPtr.Zero);

            System.Threading.Thread.Sleep(300);

            IntPtr workerW = IntPtr.Zero;
            IntPtr current = IntPtr.Zero;

            while (true)
            {
                current = FindWindowEx(IntPtr.Zero, current, "WorkerW", null);
                if (current == IntPtr.Zero) break;

                IntPtr defView = FindWindowEx(current, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (defView != IntPtr.Zero)
                {
                    workerW = FindWindowEx(IntPtr.Zero, current, "WorkerW", null);
                    if (workerW != IntPtr.Zero)
                    {
                        return workerW;
                    }
                }
            }
            return IntPtr.Zero;
        }

        public static void SetAsWallpaper(Form form)
        {
            IntPtr workerW = GetWorkerW();

            if (workerW == IntPtr.Zero)
            {
                MessageBox.Show("Не удалось найти окно обоев. Попробуйте свернуть все окна и попробовать снова.");
                return;
            }

            form.Show();
            Application.DoEvents();
            System.Threading.Thread.Sleep(200);

            SetParent(form.Handle, workerW);

            Screen primary = Screen.PrimaryScreen;
            MoveWindow(form.Handle,
                primary.Bounds.X,
                primary.Bounds.Y,
                primary.Bounds.Width,
                primary.Bounds.Height,
                true);

            ShowWindow(form.Handle, SW_SHOW);

            Application.DoEvents();
        }

        public static void RestoreToNormal(Form form)
        {
            SetParent(form.Handle, IntPtr.Zero);
            form.ShowInTaskbar = true;
        }
    }
}