using System;

namespace GameOfLife.Environment
{
    /// <summary>
    /// Магнитная аномалия в мире.
    /// 
    /// Представляет область с искажённым магнитным полем
    /// которая влияет на навигацию ботов:
    /// - Дезориентация при движении
    /// - Ошибки в программе движения
    /// - Случайное изменение направления
    /// 
    /// В отличие от радиоактивных зон:
    /// - Не наносит урон здоровью
    /// - Не вызывает мутации
    /// - Влияет только на навигацию и движение
    /// 
    /// Использование:
    /// var anomaly = new MagneticField(50, 50, 15, 0.5f);
    /// var direction = anomaly.GetDistortedDirection(originalDirection, random);
    /// </summary>
    public class MagneticField
    {
        #region Приватные поля

        /// <summary>
        /// Уникальный идентификатор аномалии.
        /// </summary>
        private readonly Guid _id;

        /// <summary>
        /// Центр аномалии по координате X.
        /// </summary>
        private int _centerX;

        /// <summary>
        /// Центр аномалии по координате Y.
        /// </summary>
        private int _centerY;

        /// <summary>
        /// Радиус аномалии в клетках.
        /// </summary>
        private int _radius;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Уникальный идентификатор аномалии.
        /// </summary>
        public Guid Id => _id;

        /// <summary>
        /// Центр аномалии по координате X.
        /// </summary>
        public int CenterX
        {
            get => _centerX;
            set => _centerX = value;
        }

        /// <summary>
        /// Центр аномалии по координате Y.
        /// </summary>
        public int CenterY
        {
            get => _centerY;
            set => _centerY = value;
        }

        /// <summary>
        /// Радиус аномалии в клетках.
        /// </summary>
        public int Radius
        {
            get => _radius;
            set => _radius = Math.Max(1, value);
        }

        /// <summary>
        /// Сила магнитного искажения.
        /// 
        /// Диапазон: 0.0 - 1.0
        /// - 0.0 - 0.3: слабое искажение, почти незаметно
        /// - 0.3 - 0.6: среднее искажение, заметные ошибки
        /// - 0.6 - 1.0: сильное искажение, полная дезориентация
        /// 
        /// Влияет на вероятность изменения направления движения.
        /// </summary>
        public float DistortionStrength { get; set; }

        /// <summary>
        /// Индикатор активности аномалии.
        /// </summary>
        public bool IsActive { get; set; } = true;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую магнитную аномалию.
        /// </summary>
        /// <param name="centerX">
        /// Центр аномалии по координате X.
        /// </param>
        /// <param name="centerY">
        /// Центр аномалии по координате Y.
        /// </param>
        /// <param name="radius">
        /// Радиус аномалии в клетках.
        /// </param>
        /// <param name="distortionStrength">
        /// Сила искажения (0.0 - 1.0).
        /// </param>
        public MagneticField(
            int centerX,
            int centerY,
            int radius,
            float distortionStrength)
        {
            _id = Guid.NewGuid();
            _centerX = centerX;
            _centerY = centerY;
            _radius = radius;
            DistortionStrength = Math.Max(0, Math.Min(1, distortionStrength));
            IsActive = true;
        }

        #endregion

        #region Методы воздействия

        /// <summary>
        /// Проверяет находится ли точка в аномалии.
        /// </summary>
        public bool ContainsPoint(int x, int y)
        {
            if (!IsActive)
                return false;

            int dx = x - _centerX;
            int dy = y - _centerY;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            return distance <= _radius;
        }

        /// <summary>
        /// Получает уровень искажения в конкретной точке.
        /// 
        /// Уменьшается с расстоянием от центра.
        /// </summary>
        public float GetDistortionAtPoint(int x, int y)
        {
            if (!ContainsPoint(x, y))
                return 0;

            int dx = x - _centerX;
            int dy = y - _centerY;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            float falloff = 1.0f - (distance / _radius);
            return DistortionStrength * falloff;
        }

        /// <summary>
        /// Искажает направление движения бота.
        /// 
        /// С вероятностью равной уровню искажения
        /// направление меняется на случайное.
        /// </summary>
        /// <param name="originalDirection">
        /// Исходное направление движения.
        /// </param>
        /// <param name="x">
        /// Координата X бота.
        /// </param>
        /// <param name="y">
        /// Координата Y бота.
        /// </param>
        /// <param name="random">
        /// Генератор случайных чисел.
        /// </param>
        /// <returns>
        /// Искажённое направление (или оригинальное).
        /// </returns>
        public Genome.MovementDirection GetDistortedDirection(
            Genome.MovementDirection originalDirection,
            int x,
            int y,
            Random random)
        {
            float distortion = GetDistortionAtPoint(x, y);

            if (distortion <= 0)
                return originalDirection;

            // Вероятность искажения равна уровню искажения
            if (random.NextDouble() < distortion)
            {
                var directions = Enum.GetValues(typeof(Genome.MovementDirection));
                return (Genome.MovementDirection)directions.GetValue(random.Next(directions.Length));
            }

            return originalDirection;
        }

        /// <summary>
        /// Применяет воздействие аномалии на бота.
        /// 
        /// Возвращает информацию о дезориентации.
        /// </summary>
        public MagneticEffect ApplyToBot(Entities.Bot bot, Random random)
        {
            var effect = new MagneticEffect();

            if (!IsActive)
                return effect;

            float distortion = GetDistortionAtPoint(bot.X, bot.Y);
            effect.DistortionLevel = distortion;

            if (distortion > 0)
            {
                effect.IsDisoriented = random.NextDouble() < distortion;
            }

            return effect;
        }

        #endregion
    }
}