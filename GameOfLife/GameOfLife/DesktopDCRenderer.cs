using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GameOfLife
{
    public class DesktopDCRenderer : IDisposable
    {
        // === WinAPI для работы с Desktop DC ===
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        private const uint SRCCOPY = 0x00CC0020;

        private IntPtr _desktopDC = IntPtr.Zero;
        private IntPtr _memoryDC = IntPtr.Zero;
        private IntPtr _bitmap = IntPtr.Zero;
        private IntPtr _oldBitmap = IntPtr.Zero;
        private int _width = 0;
        private int _height = 0;
        private bool _disposed = false;

        public int Width => _width;
        public int Height => _height;

        public bool Initialize()
        {
            try
            {
                // Получаем размеры рабочего стола
                RECT rect;
                GetWindowRect(GetDesktopWindow(), out rect);
                _width = rect.Width;
                _height = rect.Height;

                // Получаем DC рабочего стола
                _desktopDC = GetWindowDC(GetDesktopWindow());
                if (_desktopDC == IntPtr.Zero)
                    return false;

                // Создаём совместимый DC в памяти
                _memoryDC = CreateCompatibleDC(_desktopDC);
                if (_memoryDC == IntPtr.Zero)
                    return false;

                // Создаём совместимый битмап
                _bitmap = CreateCompatibleBitmap(_desktopDC, _width, _height);
                if (_bitmap == IntPtr.Zero)
                    return false;

                // Выбираем битмап в memory DC
                _oldBitmap = SelectObject(_memoryDC, _bitmap);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Graphics GetGraphics()
        {
            if (_memoryDC == IntPtr.Zero)
                return null;

            return Graphics.FromHdc(_memoryDC);
        }

        public void Render()
        {
            if (_desktopDC == IntPtr.Zero || _memoryDC == IntPtr.Zero)
                return;

            // Копируем из memory DC на desktop DC
            BitBlt(_desktopDC, 0, 0, _width, _height, _memoryDC, 0, 0, SRCCOPY);
        }

        public void Clear()
        {
            using (var g = GetGraphics())
            {
                if (g != null)
                    g.Clear(Color.Black);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_oldBitmap != IntPtr.Zero)
            {
                SelectObject(_memoryDC, _oldBitmap);
                _oldBitmap = IntPtr.Zero;
            }

            if (_bitmap != IntPtr.Zero)
            {
                DeleteObject(_bitmap);
                _bitmap = IntPtr.Zero;
            }

            if (_memoryDC != IntPtr.Zero)
            {
                DeleteDC(_memoryDC);
                _memoryDC = IntPtr.Zero;
            }

            if (_desktopDC != IntPtr.Zero)
            {
                ReleaseDC(GetDesktopWindow(), _desktopDC);
                _desktopDC = IntPtr.Zero;
            }

            _disposed = true;
        }
    }
}