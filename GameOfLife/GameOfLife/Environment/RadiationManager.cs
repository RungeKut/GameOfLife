using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Environment
{
    /// <summary>
    /// Менеджер радиоактивных зон в мире.
    /// 
    /// Управляет коллекцией всех радиоактивных зон:
    /// - Добавление новых зон
    /// - Удаление неактивных зон
    /// - Расчёт общего радиационного фона в клетке
    /// - Обновление состояния всех зон
    /// 
    /// Оптимизация:
    /// - Пространственное хеширование для быстрого поиска
    /// - Кэширование уровней радиации для часто используемых клеток
    /// - Периодическая очистка неактивных зон
    /// 
    /// Использование:
    /// var manager = new RadiationManager();
    /// manager.AddZone(new RadiationZone(50, 50, 10, 0.5f));
    /// float level = manager.GetRadiationAt(52, 52);
    /// </summary>
    public class RadiationManager
    {
        #region Приватные поля

        /// <summary>
        /// Коллекция всех активных зон.
        /// </summary>
        private readonly List<RadiationZone> _zones;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Генератор случайных чисел для мутаций.
        /// </summary>
        private readonly Random _random;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Количество активных радиоактивных зон.
        /// </summary>
        public int ActiveZoneCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _zones.Count(z => z.IsActive);
                }
            }
        }

        /// <summary>
        /// Общее количество зон (включая неактивные).
        /// </summary>
        public int TotalZoneCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _zones.Count;
                }
            }
        }

        #endregion

        #region События

        /// <summary>
        /// Событие добавления новой зоны.
        /// </summary>
        public event Action<RadiationZone> OnZoneAdded;

        /// <summary>
        /// Событие удаления зоны.
        /// </summary>
        public event Action<RadiationZone> OnZoneRemoved;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый менеджер радиоактивных зон.
        /// </summary>
        /// <param name="seed">
        /// Seed для генератора случайных чисел.
        /// </param>
        public RadiationManager(int seed = -1)
        {
            _zones = new List<RadiationZone>();

            if (seed < 0)
            {
                seed = System.Environment.TickCount;
            }
            _random = new Random(seed);
        }

        #endregion

        #region Управление зонами

        /// <summary>
        /// Добавляет новую радиоактивную зону в мир.
        /// </summary>
        /// <param name="zone">
        /// Зона для добавления.
        /// </param>
        public void AddZone(RadiationZone zone)
        {
            if (zone == null)
                throw new ArgumentNullException(nameof(zone));

            lock (_lockObject)
            {
                _zones.Add(zone);
                OnZoneAdded?.Invoke(zone);
            }
        }

        /// <summary>
        /// Удаляет зону по идентификатору.
        /// </summary>
        /// <param name="zoneId">
        /// Идентификатор зоны для удаления.
        /// </param>
        /// <returns>
        /// True если зона найдена и удалена.
        /// </returns>
        public bool RemoveZone(Guid zoneId)
        {
            lock (_lockObject)
            {
                var zone = _zones.FirstOrDefault(z => z.Id == zoneId);
                if (zone != null)
                {
                    _zones.Remove(zone);
                    OnZoneRemoved?.Invoke(zone);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Удаляет все неактивные зоны.
        /// 
        /// Вызывается периодически для очистки памяти
        /// от зон с RadiationLevel = 0.
        /// </summary>
        public void CleanupInactiveZones()
        {
            lock (_lockObject)
            {
                var inactive = _zones.Where(z => !z.IsActive).ToList();
                foreach (var zone in inactive)
                {
                    _zones.Remove(zone);
                    OnZoneRemoved?.Invoke(zone);
                }
            }
        }

        #endregion

        #region Запросы радиации

        /// <summary>
        /// Получает общий уровень радиации в клетке.
        /// 
        /// Суммирует вклад всех зон которые покрывают эту клетку.
        /// Учитывает затухание радиации с расстоянием.
        /// </summary>
        /// <param name="x">
        /// Координата X клетки.
        /// </param>
        /// <param name="y">
        /// Координата Y клетки.
        /// </param>
        /// <returns>
        /// Общий уровень радиации (0.0 - 1.0+).
        /// </returns>
        public float GetRadiationAt(int x, int y)
        {
            lock (_lockObject)
            {
                float totalRadiation = 0;

                foreach (var zone in _zones)
                {
                    if (zone.IsActive)
                    {
                        totalRadiation += zone.GetRadiationAtPoint(x, y);
                    }
                }

                return totalRadiation;
            }
        }

        /// <summary>
        /// Проверяет находится ли клетка в любой радиоактивной зоне.
        /// </summary>
        /// <param name="x">
        /// Координата X клетки.
        /// </param>
        /// <param name="y">
        /// Координата Y клетки.
        /// </param>
        /// <returns>
        /// True если клетка находится в зоне.
        /// </returns>
        public bool IsInRadiationZone(int x, int y)
        {
            lock (_lockObject)
            {
                foreach (var zone in _zones)
                {
                    if (zone.IsActive && zone.ContainsPoint(x, y))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Получает все зоны которые покрывают клетку.
        /// </summary>
        /// <param name="x">
        /// Координата X клетки.
        /// </param>
        /// <param name="y">
        /// Координата Y клетки.
        /// </param>
        /// <returns>
        /// Список зон покрывающих клетку.
        /// </returns>
        public List<RadiationZone> GetZonesAt(int x, int y)
        {
            lock (_lockObject)
            {
                return _zones
                    .Where(z => z.IsActive && z.ContainsPoint(x, y))
                    .ToList();
            }
        }

        #endregion

        #region Обновление

        /// <summary>
        /// Обновляет состояние всех зон.
        /// 
        /// Вызывается каждый тик симуляции.
        /// Применяет распад радиации ко всем зонам.
        /// </summary>
        public void Update()
        {
            lock (_lockObject)
            {
                foreach (var zone in _zones)
                {
                    zone.Update();
                }

                // Периодическая очистка неактивных зон
                if (_zones.Count > 100)
                {
                    CleanupInactiveZones();
                }
            }
        }

        /// <summary>
        /// Применяет воздействие радиации на бота.
        /// 
        /// Находит все зоны в позиции бота и применяет
        /// их воздействие. Возвращает суммарный эффект.
        /// </summary>
        /// <param name="bot">
        /// Бот для воздействия.
        /// </param>
        /// <returns>
        /// Суммарный эффект радиации.
        /// </returns>
        public RadiationEffect ApplyRadiationToBot(Entities.Bot bot)
        {
            var totalEffect = new RadiationEffect();

            lock (_lockObject)
            {
                foreach (var zone in _zones)
                {
                    if (zone.IsActive && zone.ContainsPoint(bot.X, bot.Y))
                    {
                        var effect = zone.ApplyToBot(bot, _random);
                        totalEffect.DamageDealt += effect.DamageDealt;
                        totalEffect.MutationOccurred |= effect.MutationOccurred;
                    }
                }
            }

            return totalEffect;
        }

        #endregion

        #region Генерация зон

        /// <summary>
        /// Генерирует случайные радиоактивные зоны в мире.
        /// 
        /// Используется при инициализации мира для создания
        /// естественных радиоактивных областей.
        /// </summary>
        /// <param name="worldWidth">
        /// Ширина мира в клетках.
        /// </param>
        /// <param name="worldHeight">
        /// Высота мира в клетках.
        /// </param>
        /// <param name="zoneCount">
        /// Количество зон для создания.
        /// </param>
        /// <param name="minRadius">
        /// Минимальный радиус зоны.
        /// </param>
        /// <param name="maxRadius">
        /// Максимальный радиус зоны.
        /// </param>
        public void GenerateRandomZones(
            int worldWidth,
            int worldHeight,
            int zoneCount,
            int minRadius = 5,
            int maxRadius = 20)
        {
            for (int i = 0; i < zoneCount; i++)
            {
                int centerX = _random.Next(worldWidth);
                int centerY = _random.Next(worldHeight);
                int radius = _random.Next(minRadius, maxRadius + 1);
                float level = (float)(_random.NextDouble() * 0.5f + 0.3f);

                var zone = new RadiationZone(
                    centerX,
                    centerY,
                    radius,
                    level,
                    RadiationZoneType.Natural
                );

                AddZone(zone);
            }
        }

        #endregion
    }
}