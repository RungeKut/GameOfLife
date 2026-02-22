using System;
using System.Drawing;
using System.Windows.Forms;
using GameOfLife.Core;

namespace GameOfLife.Rendering
{
    /// <summary>
    /// Рендерер для отрисовки в WinForms PictureBox.
    /// </summary>
    public class WinFormsRenderer : IRenderer
    {
        #region Поля

        private PictureBox _pictureBox;
        private BitmapRenderer _bitmapRenderer;
        private Bitmap _cachedBitmap;
        private Size _lastPictureBoxSize;

        // ✅ НОВОЕ: Позиция камеры для корректного тайлинга
        private int _cameraWorldX;
        private int _cameraWorldY;

        #endregion

        #region Свойства

        public RenderConfig Config
        {
            get => _bitmapRenderer.Config;
            set => _bitmapRenderer.Config = value;
        }

        public PictureBox PictureBox => _pictureBox;

        #endregion

        #region События

        public event Action<RenderStats> OnRenderCompleted;

        #endregion

        #region Конструктор

        public WinFormsRenderer(PictureBox pictureBox, RenderConfig config = null)
        {
            _pictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));

            _pictureBox.SizeMode = PictureBoxSizeMode.Normal;
            _pictureBox.BackColor = Color.Black;

            _bitmapRenderer = new BitmapRenderer(config);
            _bitmapRenderer.OnRenderCompleted += OnBitmapRenderCompleted;

            _lastPictureBoxSize = pictureBox.Size;
            _cameraWorldX = 0;
            _cameraWorldY = 0;
        }

        #endregion

        #region IRenderer реализация

        /// <summary>
        /// Устанавливает позицию камеры для корректного тайлинга.
        /// Вызывается из mainForm перед отрисовкой.
        /// </summary>
        public void SetCameraPosition(int worldX, int worldY)
        {
            _cameraWorldX = worldX;
            _cameraWorldY = worldY;
        }

        public void Render(WorldState worldState)
        {
            _bitmapRenderer.Render(worldState);
        }

        public void SaveFrame(string filePath)
        {
            _bitmapRenderer.SaveFrame(filePath);
        }

        public void BeginRecording()
        {
            _bitmapRenderer.BeginRecording();
        }

        public void EndRecording(string outputPath, int fps)
        {
            _bitmapRenderer.EndRecording(outputPath, fps);
        }

        #endregion

        #region Обработчики событий

        private void OnBitmapRenderCompleted(RenderStats stats)
        {
            if (_pictureBox.InvokeRequired)
            {
                _pictureBox.Invoke(new Action<RenderStats>(UpdatePictureBox), stats);
            }
            else
            {
                UpdatePictureBox(stats);
            }

            OnRenderCompleted?.Invoke(stats);
        }

        /// <summary>
        /// Обновляет изображение в PictureBox с учётом позиции камеры.
        /// </summary>
        private void UpdatePictureBox(RenderStats stats)
        {
            if (_pictureBox == null || _bitmapRenderer.CurrentFrame == null)
                return;

            _cachedBitmap?.Dispose();
            _cachedBitmap = new Bitmap(_pictureBox.Width, _pictureBox.Height);

            using (Graphics g = Graphics.FromImage(_cachedBitmap))
            {
                g.Clear(Color.Black);

                Bitmap worldBitmap = _bitmapRenderer.CurrentFrame;
                int worldWidth = worldBitmap.Width;
                int worldHeight = worldBitmap.Height;

                // ✅ КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: Рассчитываем смещение на основе позиции камеры
                // Это обеспечивает соответствие между отображением и координатами мыши
                int offsetX = 0;
                int offsetY = 0;

                if (worldWidth > 0 && worldHeight > 0)
                {
                    // Смещение на основе позиции камеры в мире
                    // Отрицательное смещение чтобы центр PictureBox показывал _cameraWorldX, _cameraWorldY
                    offsetX = -(_cameraWorldX * Config.CellSize) % worldWidth;
                    offsetY = -(_cameraWorldY * Config.CellSize) % worldHeight;

                    // Корректировка для отрицательных значений
                    if (offsetX < 0) offsetX += worldWidth;
                    if (offsetY < 0) offsetY += worldHeight;
                }

                // ✅ Тайлим мир со смещением
                for (int x = offsetX - worldWidth; x < _pictureBox.Width; x += worldWidth)
                {
                    for (int y = offsetY - worldHeight; y < _pictureBox.Height; y += worldHeight)
                    {
                        g.DrawImage(worldBitmap, x, y);
                    }
                }
            }

            _pictureBox.Image?.Dispose();
            _pictureBox.Image = _cachedBitmap;
            _pictureBox.Refresh();

            _lastPictureBoxSize = _pictureBox.Size;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _bitmapRenderer.OnRenderCompleted -= OnBitmapRenderCompleted;
            _bitmapRenderer.Dispose();
            _cachedBitmap?.Dispose();

            if (_pictureBox != null)
            {
                _pictureBox.Image = null;
            }
        }

        #endregion
    }
}