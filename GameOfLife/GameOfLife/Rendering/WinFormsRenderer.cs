using System;
using System.Drawing;
using System.Windows.Forms;
using GameOfLife.Core;

namespace GameOfLife.Rendering
{
    /// <summary>
    /// Рендерер для отрисовки в WinForms PictureBox.
    /// 
    /// Этот класс служит мостом между абстрактной системой рендеринга
    /// и конкретным UI элементом Windows Forms (PictureBox).
    /// 
    /// Архитектурные особенности:
    /// - Делегирование: использует BitmapRenderer для фактической отрисовки
    /// - Маршалинг: автоматически переключается в UI поток для обновления PictureBox
    /// - Разделение ответственности: логика отрисовки в BitmapRenderer, UI в этом классе
    /// 
    /// Преимущества такого подхода:
    /// - BitmapRenderer можно тестировать без UI
    /// - WinFormsRenderer можно заменить на другой UI (WPF, Console, Web)
    /// - Отрисовка в память происходит в фоне, UI обновляется отдельно
    /// </summary>
    public class WinFormsRenderer : IRenderer
    {
        #region Поля

        /// <summary>
        /// PictureBox для отрисовки.
        /// 
        /// Это UI элемент Windows Forms, который отображает изображение.
        /// Ссылка хранится для обновления Image свойства.
        /// 
        /// Важно: PictureBox принадлежит UI потоку. Все операции с ним
        /// должны выполняться через Invoke если вызов из другого потока.
        /// </summary>
        private PictureBox _pictureBox;

        /// <summary>
        /// Внутренний BitmapRenderer для отрисовки в память.
        /// 
        /// Этот объект выполняет фактическую отрисовку состояния мира
        /// в Bitmap. WinFormsRenderer только копирует результат в PictureBox.
        /// 
        /// Такое разделение позволяет:
        /// - Переиспользовать логику отрисовки
        /// - Тестировать отрисовку без UI
        /// - Легко заменить UI на другой тип
        /// </summary>
        private BitmapRenderer _bitmapRenderer;

        /// <summary>
        /// Флаг необходимости обновления UI.
        /// 
        /// Используется для оптимизации: если несколько обновлений
        /// произошли быстро, можно объединить их в одно обновление UI.
        /// </summary>
        private bool _needsRefresh;

        #endregion

        #region Свойства

        /// <summary>
        /// Конфигурация рендерера.
        /// 
        /// Делегирует чтение и запись внутреннему BitmapRenderer.
        /// Изменения конфигурации применяются к следующему кадру.
        /// </summary>
        public RenderConfig Config
        {
            get => _bitmapRenderer.Config;
            set => _bitmapRenderer.Config = value;
        }

        /// <summary>
        /// Связанный PictureBox.
        /// 
        /// Возвращает ссылку на UI элемент. Используется для отладки
        /// и инспекции состояния.
        /// 
        /// Внимание: не модифицируйте PictureBox напрямую через это свойство.
        /// Все изменения должны идти через методы рендерера.
        /// </summary>
        public PictureBox PictureBox => _pictureBox;

        #endregion

        #region События

        /// <summary>
        /// Событие завершения отрисовки кадра.
        /// 
        /// Делегирует событие от внутреннего BitmapRenderer.
        /// Подписчики получают статистику после обновления UI.
        /// </summary>
        public event Action<RenderStats> OnRenderCompleted;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый экземпляр WinFormsRenderer.
        /// 
        /// Инициализирует внутренний BitmapRenderer и подписывается
        /// на его события для обновления UI.
        /// 
        /// PictureBox должен быть создан и добавлен на форму до
        /// передачи в этот конструктор.
        /// </summary>
        /// <param name="pictureBox">
        /// PictureBox для отрисовки.
        /// Не может быть null.
        /// </param>
        /// <param name="config">
        /// Конфигурация рендерера.
        /// Если null, используется конфигурация по умолчанию.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается если pictureBox равен null.
        /// </exception>
        public WinFormsRenderer(PictureBox pictureBox, RenderConfig config = null)
        {
            _pictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));
            _bitmapRenderer = new BitmapRenderer(config);

            // Подписываемся на событие завершения отрисовки
            // Это позволяет обновлять UI после отрисовки в память
            _bitmapRenderer.OnRenderCompleted += OnBitmapRenderCompleted;
        }

        #endregion

        #region IRenderer реализация

        /// <summary>
        /// Отрисовывает состояние мира в PictureBox.
        /// 
        /// Метод делегирует фактическую отрисовку BitmapRenderer,
        /// который создаёт Bitmap в памяти. Обновление PictureBox
        /// происходит асинхронно через событие OnBitmapRenderCompleted.
        /// 
        /// Этот метод может вызываться из любого потока (включая
        /// фоновый поток симуляции).
        /// </summary>
        /// <param name="worldState">Состояние мира для отрисовки.</param>
        public void Render(WorldState worldState)
        {
            // Делегируем отрисовку в память
            _bitmapRenderer.Render(worldState);
        }

        /// <summary>
        /// Сохраняет текущий кадр в файл.
        /// 
        /// Делегирует метод внутреннему BitmapRenderer.
        /// </summary>
        /// <param name="filePath">Путь к файлу для сохранения.</param>
        public void SaveFrame(string filePath)
        {
            _bitmapRenderer.SaveFrame(filePath);
        }

        /// <summary>
        /// Начинает запись анимации.
        /// 
        /// Делегирует метод внутреннему BitmapRenderer.
        /// </summary>
        public void BeginRecording()
        {
            _bitmapRenderer.BeginRecording();
        }

        /// <summary>
        /// Завершает запись и сохраняет GIF.
        /// 
        /// Делегирует метод внутреннему BitmapRenderer.
        /// </summary>
        /// <param name="outputPath">Путь к выходному файлу.</param>
        /// <param name="fps">Количество кадров в секунду.</param>
        public void EndRecording(string outputPath, int fps)
        {
            _bitmapRenderer.EndRecording(outputPath, fps);
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик завершения отрисовки Bitmap.
        /// 
        /// Вызывается внутренним BitmapRenderer после отрисовки кадра.
        /// Этот метод отвечает за обновление PictureBox в UI потоке.
        /// 
        /// Критически важно: обновление UI должно выполняться в UI потоке.
        /// Если метод вызван из фонового потока, используется Invoke.
        /// </summary>
        /// <param name="stats">Статистика отрисовки кадра.</param>
        private void OnBitmapRenderCompleted(RenderStats stats)
        {
            // Проверка необходимости маршалинга в UI поток
            if (_pictureBox.InvokeRequired)
            {
                // Вызываем метод в UI потоке
                _pictureBox.Invoke(new Action<RenderStats>(UpdatePictureBox), stats);
            }
            else
            {
                // Уже в UI потоке, обновляем напрямую
                UpdatePictureBox(stats);
            }

            // Пробрасываем событие дальше для других подписчиков
            OnRenderCompleted?.Invoke(stats);
        }

        /// <summary>
        /// Обновляет изображение в PictureBox.
        /// 
        /// Этот метод должен вызываться только из UI потока.
        /// Выполняет следующие операции:
        /// 1. Освобождает старое изображение (предотвращает утечку памяти)
        /// 2. Клонирует Bitmap из рендерера (для безопасности)
        /// 3. Устанавливает новое изображение в PictureBox
        /// 4. Принудительно обновляет PictureBox
        /// 
        /// Клонирование необходимо потому что BitmapRenderer может
        /// переиспользовать свой Bitmap для следующего кадра.
        /// </summary>
        /// <param name="stats">Статистика отрисовки (не используется напрямую).</param>
        private void UpdatePictureBox(RenderStats stats)
        {
            // Защита от null
            if (_pictureBox == null || _bitmapRenderer.CurrentFrame == null)
                return;

            // Освобождаем старое изображение (предотвращаем утечку GDI объектов)
            _pictureBox.Image?.Dispose();

            // Клонируем Bitmap для PictureBox
            // Это важно: renderer может переиспользовать свой Bitmap
            _pictureBox.Image = new Bitmap(_bitmapRenderer.CurrentFrame);

            // Принудительно обновляем PictureBox
            _pictureBox.Refresh();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы рендерера.
        /// 
        /// Выполняет следующие операции:
        /// 1. Отписывается от событий BitmapRenderer
        /// 2. Освобождает BitmapRenderer
        /// 3. Освобождает изображение PictureBox
        /// 
        /// После вызова этого метода рендерер не может использоваться.
        /// </summary>
        public void Dispose()
        {
            // Отписываемся от событий (предотвращаем утечки)
            _bitmapRenderer.OnRenderCompleted -= OnBitmapRenderCompleted;

            // Освобождаем внутренний рендерер
            _bitmapRenderer.Dispose();

            // Освобождаем изображение PictureBox
            if (_pictureBox != null)
            {
                _pictureBox.Image?.Dispose();
                _pictureBox.Image = null;
            }
        }

        #endregion
    }
}