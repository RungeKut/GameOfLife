using System;

namespace GameOfLife.Genome
{
    /// <summary>
    /// Направление перемещения в сетке мира.
    /// 
    /// Включает 8 направлений по сторонам света и диагоналям,
    /// а также команду остаться на месте.
    /// 
    /// Используется в программе движения генома для определения
    /// желаемого вектора перемещения бота.
    /// </summary>
    public enum MovementDirection
    {
        /// <summary>
        /// Движение на север (уменьшение Y).
        /// Вектор: (0, -1)
        /// </summary>
        North,

        /// <summary>
        /// Движение на северо-восток.
        /// Вектор: (1, -1)
        /// </summary>
        NorthEast,

        /// <summary>
        /// Движение на восток (увеличение X).
        /// Вектор: (1, 0)
        /// </summary>
        East,

        /// <summary>
        /// Движение на юго-восток.
        /// Вектор: (1, 1)
        /// </summary>
        SouthEast,

        /// <summary>
        /// Движение на юг (увеличение Y).
        /// Вектор: (0, 1)
        /// </summary>
        South,

        /// <summary>
        /// Движение на юго-запад.
        /// Вектор: (-1, 1)
        /// </summary>
        SouthWest,

        /// <summary>
        /// Движение на запад (уменьшение X).
        /// Вектор: (-1, 0)
        /// </summary>
        West,

        /// <summary>
        /// Движение на северо-запад.
        /// Вектор: (-1, -1)
        /// </summary>
        NorthWest,

        /// <summary>
        /// Остаться на текущей клетке.
        /// Вектор: (0, 0)
        /// 
        /// Используется для пауз в программе движения
        /// или когда бот ждёт благоприятных условий.
        /// </summary>
        Stay
    }

    /// <summary>
    /// Инструкция движения для программы генома.
    /// 
    /// Определяет одно действие в последовательности:
    /// направление и количество тиков для его выполнения.
    /// 
    /// Пример: инструкция (East, 3) означает "двигаться на восток
    /// в течение 3 тиков, если не прервано внешним событием".
    /// </summary>
    public class MovementInstruction : IEquatable<MovementInstruction>
    {
        /// <summary>
        /// Направление движения.
        /// </summary>
        public MovementDirection Direction { get; set; }

        /// <summary>
        /// Количество тиков для выполнения этого направления.
        /// Минимальное значение: 1.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Текущий счётчик выполнения (внутреннее использование).
        /// Сбрасывается при начале выполнения инструкции.
        /// </summary>
        internal int CurrentTick { get; set; }

        /// <summary>
        /// Создаёт новую инструкцию движения.
        /// </summary>
        /// <param name="direction">Направление перемещения.</param>
        /// <param name="duration">Длительность в тиках (мин. 1).</param>
        public MovementInstruction(MovementDirection direction, int duration)
        {
            Direction = direction;
            Duration = Math.Max(1, duration);
            CurrentTick = 0;
        }

        /// <summary>
        /// Создаёт копию инструкции.
        /// </summary>
        /// <param name="other">Инструкция для копирования.</param>
        public MovementInstruction(MovementInstruction other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Direction = other.Direction;
            Duration = other.Duration;
            CurrentTick = other.CurrentTick;
        }

        /// <summary>
        /// Возвращает вектор смещения для данного направления.
        /// </summary>
        /// <returns>Кортеж (dx, dy) для применения к координатам.</returns>
        public (int dx, int dy) GetDelta()
        {
            return Direction switch
            {
                MovementDirection.North => (0, -1),
                MovementDirection.NorthEast => (1, -1),
                MovementDirection.East => (1, 0),
                MovementDirection.SouthEast => (1, 1),
                MovementDirection.South => (0, 1),
                MovementDirection.SouthWest => (-1, 1),
                MovementDirection.West => (-1, 0),
                MovementDirection.NorthWest => (-1, -1),
                MovementDirection.Stay => (0, 0),
                _ => (0, 0)
            };
        }

        /// <summary>
        /// Сбрасывает счётчик выполнения инструкции.
        /// Вызывается при начале выполнения новой инструкции.
        /// </summary>
        public void Reset()
        {
            CurrentTick = 0;
        }

        /// <summary>
        /// Инкрементирует счётчик и проверяет завершение.
        /// </summary>
        /// <returns>True если инструкция выполнена полностью.</returns>
        public bool Advance()
        {
            CurrentTick++;
            return CurrentTick >= Duration;
        }

        /// <summary>
        /// Проверяет, завершена ли инструкция.
        /// </summary>
        public bool IsComplete => CurrentTick >= Duration;

        #region Переопределения

        public bool Equals(MovementInstruction other)
        {
            return other != null &&
                   Direction == other.Direction &&
                   Duration == other.Duration;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MovementInstruction);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Direction, Duration);
        }

        public override string ToString()
        {
            return $"{Direction}({Duration})";
        }

        #endregion
    }
}