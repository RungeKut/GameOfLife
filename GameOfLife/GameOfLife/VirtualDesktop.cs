using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife
{
    public class MonitorInfo
    {
        public Screen Screen { get; set; }
        public int X { get; set; } // Позиция относительно виртуального/desktop (0,0)
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsPrimary { get; set; }
    }

    public static class VirtualDesktop
    {
        public static List<MonitorInfo> GetMonitors()
        {
            var monitors = new List<MonitorInfo>();
            int minX = 0, minY = 0, maxX = 0, maxY = 0;

            // Сначала определяем границы виртуального экрана
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Bounds.Left < minX) minX = screen.Bounds.Left;
                if (screen.Bounds.Top < minY) minY = screen.Bounds.Top;
                if (screen.Bounds.Right > maxX) maxX = screen.Bounds.Right;
                if (screen.Bounds.Bottom > maxY) maxY = screen.Bounds.Bottom;
            }

            // Нормализуем координаты (сдвигаем так, чтобы левый-верхний был 0,0)
            foreach (Screen screen in Screen.AllScreens)
            {
                monitors.Add(new MonitorInfo
                {
                    Screen = screen,
                    X = screen.Bounds.Left - minX,
                    Y = screen.Bounds.Top - minY,
                    Width = screen.Bounds.Width,
                    Height = screen.Bounds.Height,
                    IsPrimary = screen.Primary
                });
            }

            return monitors;
        }

        public static int GetTotalWidth(List<MonitorInfo> monitors)
        {
            int maxWidth = 0;
            foreach (var m in monitors)
            {
                if (m.X + m.Width > maxWidth)
                    maxWidth = m.X + m.Width;
            }
            return maxWidth;
        }

        public static int GetTotalHeight(List<MonitorInfo> monitors)
        {
            int maxHeight = 0;
            foreach (var m in monitors)
            {
                if (m.Y + m.Height > maxHeight)
                    maxHeight = m.Y + m.Height;
            }
            return maxHeight;
        }

        public static Point2D GetWorldSize(List<MonitorInfo> monitors, int cellSize = 1)
        {
            int totalWidth = GetTotalWidth(monitors) / cellSize;
            int totalHeight = GetTotalHeight(monitors) / cellSize;
            return new Point2D(totalWidth, totalHeight);
        }

        // Преобразование координат мира в экранные координаты
        public static Point WorldToScreen(int worldX, int worldY, int cellSize, List<MonitorInfo> monitors)
        {
            int pixelX = worldX * cellSize;
            int pixelY = worldY * cellSize;

            // Находим монитор, которому принадлежит эта точка
            foreach (var m in monitors)
            {
                if (pixelX >= m.X && pixelX < m.X + m.Width &&
                    pixelY >= m.Y && pixelY < m.Y + m.Height)
                {
                    // Возвращаем координаты относительно этого монитора
                    return new Point(pixelX - m.X, pixelY - m.Y);
                }
            }

            // Если не нашли (за границами), возвращаем относительно первого монитора
            return new Point(pixelX, pixelY);
        }

        // Проверка, принадлежит ли точка мира конкретному монитору
        public static bool IsPointOnMonitor(int worldX, int worldY, int cellSize, MonitorInfo monitor)
        {
            int pixelX = worldX * cellSize;
            int pixelY = worldY * cellSize;

            return pixelX >= monitor.X && pixelX < monitor.X + monitor.Width &&
                   pixelY >= monitor.Y && pixelY < monitor.Y + monitor.Height;
        }
    }
}