using System;
using System.Collections.Generic;

namespace GameOfLife.Environment
{
    /// <summary>
    /// Радиоактивная зона в мире.
    /// 
    /// Представляет область с повышенным радиационным фоном
    /// которая влияет на находящихся в ней ботов:
    /// - Повреждение здоровья каждый тик
    /// - Повышенная вероятность мутаций генома
    /// - Возможность сбора радиоактивных ресурсов
    /// 
    /// Зоны могут быть:
    /// - Статические: созданные при генерации мира
    /// - Динамические: созданные деятельностью ботов
    /// - Временные: распадающиеся со временем
    /// 
    /// Архитектурное разделение:
    /// - RadiationZone: одна зона с параметрами
    /// - RadiationManager: управление всеми зонами в мире
    /// </summary>
    public class RadiationZone
    {
        #region Приватные поля

        /// <summary>
        /// Уникальный идентификатор зоны.
        /// </summary>
        private readonly Guid _id;

        /// <summary>
        /// Центр зоны по координате X.
        /// </summary>
        private int _centerX;

        /// <summary>
        /// Центр зоны по координате Y.
        /// </summary>
        private int _centerY;

        /// <summary>
        /// Радиус зоны в клетках.
        /// </summary>
        private int _radius;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Уникальный идентификатор зоны.
        /// </summary>
        public Guid Id => _id;

        /// <summary>
        /// Центр зоны по координате X.
        /// Может изменяться для движущихся зон.
        /// </summary>
        public int CenterX
        {
            get => _centerX;
            set => _centerX = value;
        }

        /// <summary>
        /// Центр зоны по координате Y.
        /// Может изменяться для движущихся зон.
        /// </summary>
        public int CenterY
        {
            get => _centerY;
            set => _centerY = value;
        }

        /// <summary>
        /// Радиус зоны в клетках.
        /// Определяет площадь воздействия.
        /// </summary>
        public int Radius
        {
            get => _radius;
            set => _radius = Math.Max(1, value);
        }

        /// <summary>
        /// Уровень радиации в центре зоны.
        /// 
        /// Диапазон: 0.0 - 1.0
        /// - 0.0 - 0.3: низкий уровень, минимальный урон
        /// - 0.3 - 0.6: средний уровень, заметный урон
        /// - 0.6 - 1.0: высокий уровень, опасная зона
        /// 
        /// Урон боту = RadiationLevel * DamagePerTick
        /// </summary>
        public float RadiationLevel { get; set; }

        /// <summary>
        /// Урон здоровью бота за тик в зоне.
        /// 
        /// Базовое значение умножается на RadiationLevel.
        /// По умолчанию 1.0 здоровья за тик при уровне 1.0.
        /// </summary>
        public float DamagePerTick { get; set; } = 1.0f;

        /// <summary>
        /// Вероятность мутации генома бота за тик в зоне.
        /// 
        /// Диапазон: 0.0 - 0.5
        /// - 0.0: мутаций нет
        /// - 0.1: 10% шанс мутации за тик
        /// - 0.5: 50% шанс мутации за тик
        /// 
        /// Мутации могут быть как полезными так и вредными.
        /// </summary>
        public float MutationChance { get; set; } = 0.01f;

        /// <summary>
        /// Скорость распада радиации за тик.
        /// 
        /// Диапазон: 0.0 - 1.0
        /// - 0.0: зона не распадается (постоянная)
        /// - 0.01: 1% радиации исчезает за тик
        /// - 1.0: зона исчезает мгновенно
        /// 
        /// Используется для временных зон (отходы ботов).
        /// </summary>
        public float DecayRate { get; set; } = 0.0f;

        /// <summary>
        /// Индикатор активности зоны.
        /// 
        /// false: зона неактивна, не влияет на ботов
        /// true: зона активна, применяется воздействие
        /// 
        /// Устанавливается в false когда RadiationLevel <= 0.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Тип происхождения зоны.
        /// 
        /// Natural: естественная зона (при генерации мира)
        /// Artificial: создана деятельностью ботов
        /// Temporary: временная зона (распадётся со временем)
        /// </summary>
        public RadiationZoneType ZoneType { get; set; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую радиоактивную зону.
        /// </summary>
        /// <param name="centerX">
        /// Центр зоны по координате X.
        /// </param>
        /// <param name="centerY">
        /// Центр зоны по координате Y.
        /// </param>
        /// <param name="radius">
        /// Радиус зоны в клетках.
        /// </param>
        /// <param name="radiationLevel">
        /// Уровень радиации (0.0 - 1.0).
        /// </param>
        /// <param name="zoneType">
        /// Тип происхождения зоны.
        /// </param>
        public RadiationZone(
            int centerX,
            int centerY,
            int radius,
            float radiationLevel,
            RadiationZoneType zoneType = RadiationZoneType.Natural)
        {
            _id = Guid.NewGuid();
            _centerX = centerX;
            _centerY = centerY;
            _radius = radius;
            RadiationLevel = Math.Max(0, Math.Min(1, radiationLevel));
            ZoneType = zoneType;
            IsActive = RadiationLevel > 0;
        }

        #endregion

        #region Методы воздействия

        /// <summary>
        /// Проверяет находится ли точка в зоне радиации.
        /// </summary>
        /// <param name="x">
        /// Координата X проверяемой точки.
        /// </param>
        /// <param name="y">
        /// Координата Y проверяемой точки.
        /// </param>
        /// <returns>
        /// True если точка находится в пределах зоны.
        /// </returns>
        public bool ContainsPoint(int x, int y)
        {
            if (!IsActive)
                return false;

            // Расчёт расстояния до центра зоны
            int dx = x - _centerX;
            int dy = y - _centerY;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            return distance <= _radius;
        }

        /// <summary>
        /// Рассчитывает уровень радиации в конкретной точке.
        /// 
        /// Уровень уменьшается с расстоянием от центра:
        /// - Центр: 100% от RadiationLevel
        /// - Край зоны: 0% от RadiationLevel
        /// - За пределами: 0
        /// 
        /// Формула: level * (1 - distance/radius)
        /// </summary>
        /// <param name="x">
        /// Координата X точки.
        /// </param>
        /// <param name="y">
        /// Координата Y точки.
        /// </param>
        /// <returns>
        /// Уровень радиации в точке (0.0 - 1.0).
        /// </returns>
        public float GetRadiationAtPoint(int x, int y)
        {
            if (!ContainsPoint(x, y))
                return 0;

            int dx = x - _centerX;
            int dy = y - _centerY;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // Линейное затухание от центра к краю
            float falloff = 1.0f - (distance / _radius);
            return RadiationLevel * falloff;
        }

        /// <summary>
        /// Применяет воздействие радиации на бота.
        /// 
        /// Выполняет:
        /// 1. Расчёт уровня радиации в позиции бота
        /// 2. Нанесение урона здоровью
        /// 3. Проверка на мутацию генома
        /// 4. Возврат информации о воздействии
        /// </summary>
        /// <param name="bot">
        /// Бот находящийся в зоне.
        /// </param>
        /// <param name="random">
        /// Генератор случайных чисел для мутаций.
        /// </param>
        /// <returns>
        /// Результат воздействия радиации.
        /// </returns>
        public RadiationEffect ApplyToBot(Entities.Bot bot, Random random)
        {
            var effect = new RadiationEffect();

            if (!IsActive)
                return effect;

            float level = GetRadiationAtPoint(bot.X, bot.Y);

            if (level <= 0)
                return effect;

            // Нанесение урона
            float damage = level * DamagePerTick;
            bot.TakeDamage(damage);
            effect.DamageDealt = damage;

            // Проверка на мутацию
            float mutationRoll = (float)random.NextDouble();
            if (mutationRoll < MutationChance * level)
            {
                bot.Genome.Mutate(random);
                effect.MutationOccurred = true;
            }

            return effect;
        }

        /// <summary>
        /// Обновляет состояние зоны (распад радиации).
        /// 
        /// Вызывается каждый тик симуляции.
        /// Уменьшает RadiationLevel на DecayRate.
        /// Деактивирует зону если уровень достиг 0.
        /// </summary>
        public void Update()
        {
            if (DecayRate <= 0)
                return;

            RadiationLevel -= DecayRate;

            if (RadiationLevel <= 0)
            {
                RadiationLevel = 0;
                IsActive = false;
            }
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Возвращает строковое представление зоны для отладки.
        /// </summary>
        public override string ToString()
        {
            return $"RadiationZone[{_id}] at ({_centerX},{_centerY}) r={_radius} lvl={RadiationLevel:F2}";
        }

        #endregion
    }
}