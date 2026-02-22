using System;

namespace GameOfLife.Environment
{
    /// <summary>
    /// Система погоды и климата мира.
    /// 
    /// Управляет динамическими изменениями окружающей среды:
    /// - Сила и направление ветра
    /// - Температура и влажность
    /// - Погодные явления (дождь, буря, туман)
    /// - Сезонные изменения
    /// 
    /// Влияние на ботов:
    /// - Ветер сдувает лёгкие ресурсы и слабых ботов
    /// - Экстремальная температура увеличивает расход энергии
    /// - Погодные явления могут ограничивать видимость и движение
    /// 
    /// Архитектурное разделение:
    /// - WeatherSystem: общая погода для всего мира
    /// - TemperatureMap: локальная температура по клеткам
    /// - WindSystem: расчёт воздействия ветра
    /// </summary>
    public class WeatherSystem
    {
        #region Приватные поля

        /// <summary>
        /// Генератор случайных чисел для погодных эффектов.
        /// Используется детерминированный seed для воспроизводимости.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// Текущий тик симуляции для расчёта циклических изменений.
        /// </summary>
        private long _currentTick;

        /// <summary>
        /// Длительность одного дня в тиках симуляции.
        /// По умолчанию 1000 тиков = 1 день.
        /// </summary>
        private int _dayLength;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Текущая погода.
        /// 
        /// Определяет видимые эффекты и влияние на окружение:
        /// - Clear: ясная погода, нормальные условия
        /// - Rain: дождь, ресурсы регенерируют быстрее
        /// - Storm: буря, сильный ветер, опасность для ботов
        /// - Fog: туман, ограниченная видимость для ботов
        /// </summary>
        public WeatherCondition CurrentWeather { get; private set; }

        /// <summary>
        /// Сила ветра от 0 (штиль) до 1 (ураган).
        /// 
        /// Влияет на:
        /// - Перемещение лёгких ресурсов
        /// - Расход энергии ботов при движении
        /// - Вероятность сдувания слабых ботов
        /// 
        /// Значение обновляется каждый WeatherUpdateInterval тиков.
        /// </summary>
        public float WindStrength { get; private set; }

        /// <summary>
        /// Направление ветра в градусах (0-360).
        /// 
        /// 0 = север, 90 = восток, 180 = юг, 270 = запад.
        /// Используется для расчёта вектора перемещения ресурсов.
        /// </summary>
        public float WindDirection { get; private set; }

        /// <summary>
        /// Средняя температура мира в текущий момент.
        /// 
        /// Диапазон: -20 до +50 градусов.
        /// Влияет на расход энергии ботов:
        /// - Оптимальная температура (15-25): нормальный расход
        /// - Холод (<10): увеличенный расход на обогрев
        /// - Жара (>35): увеличенный расход на охлаждение
        /// </summary>
        public float Temperature { get; private set; }

        /// <summary>
        /// Влажность воздуха от 0 (сухо) до 1 (влажно).
        /// 
        /// Влияет на:
        /// - Скорость регенерации ресурсов (вода, пища)
        /// - Вероятность погодных явлений
        /// - Комфортность условий для ботов
        /// </summary>
        public float Humidity { get; private set; }

        /// <summary>
        /// Прогресс текущего дня от 0.0 до 1.0.
        /// 
        /// 0.0 = рассвет, 0.5 = полдень, 1.0 = закат.
        /// Используется для расчёта температуры и освещения.
        /// </summary>
        public float DayProgress { get; private set; }

        /// <summary>
        /// Интервал обновления погоды в тиках.
        /// 
        /// По умолчанию 100 тиков между изменениями погоды.
        /// Меньшее значение = более динамичная погода.
        /// </summary>
        public int WeatherUpdateInterval { get; set; } = 100;

        #endregion

        #region События

        /// <summary>
        /// Событие изменения погоды.
        /// 
        /// Вызывается когда CurrentWeather меняется на новое значение.
        /// Подписчики могут использовать для:
        /// - Уведомления ботов об изменении условий
        /// - Триггеров для поведенческих изменений
        /// - Логирования и статистики
        /// </summary>
        public event Action<WeatherCondition, WeatherCondition> OnWeatherChanged;

        /// <summary>
        /// Событие смены дня и ночи.
        /// 
        /// Вызывается при переходе через порог дня/ночи.
        /// Аргумент: true = день, false = ночь.
        /// </summary>
        public event Action<bool> OnDayNightChanged;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую систему погоды.
        /// </summary>
        /// <param name="seed">
        /// Seed для генератора случайных чисел.
        /// -1 = случайный seed, >=0 = детерминированный.
        /// </param>
        /// <param name="dayLength">
        /// Длительность дня в тиках симуляции.
        /// </param>
        public WeatherSystem(int seed = -1, int dayLength = 1000)
        {
            if (seed < 0)
            {
                seed = System.Environment.TickCount;
            }
            _random = new Random(seed);
            _dayLength = dayLength;
            _currentTick = 0;
            CurrentWeather = WeatherCondition.Clear;
            WindStrength = 0;
            WindDirection = 0;
            Temperature = 20;
            Humidity = 0.5f;
            DayProgress = 0;
        }

        #endregion

        #region Основной метод обновления

        /// <summary>
        /// Обновляет состояние погоды на один тик симуляции.
        /// 
        /// Выполняет следующие операции:
        /// 1. Инкремент текущего тика
        /// 2. Расчёт прогресса дня (0.0 - 1.0)
        /// 3. Обновление температуры на основе времени суток
        /// 4. Проверка необходимости смены погоды
        /// 5. Обновление параметров ветра
        /// 6. Уведомление подписчиков об изменениях
        /// 
        /// Вызывается движком каждый тик симуляции.
        /// </summary>
        /// <param name="tick">
        /// Текущий номер тика симуляции.
        /// </param>
        public void Update(long tick)
        {
            _currentTick = tick;

            // Расчёт прогресса дня
            DayProgress = (tick % _dayLength) / (float)_dayLength;

            // Обновление температуры на основе времени суток
            UpdateTemperature();

            // Обновление влажности
            UpdateHumidity();

            // Проверка смены погоды
            if (_currentTick % WeatherUpdateInterval == 0)
            {
                UpdateWeather();
                UpdateWind();
            }
        }

        #endregion

        #region Методы обновления параметров

        /// <summary>
        /// Обновляет температуру на основе времени суток.
        /// 
        /// Формула:
        /// BaseTemp + Sin(DayProgress * 2π) * Amplitude
        /// 
        /// Пример:
        /// - Ночь (0.0, 1.0): 15°C
        /// - Полдень (0.5): 25°C
        /// - Утро/вечер (0.25, 0.75): 20°C
        /// </summary>
        private void UpdateTemperature()
        {
            float baseTemp = 20.0f;
            float amplitude = 10.0f;

            // Синусоида для суточного цикла
            float tempVariation = (float)Math.Sin(DayProgress * Math.PI * 2);

            Temperature = baseTemp + tempVariation * amplitude;
        }

        /// <summary>
        /// Обновляет влажность воздуха.
        /// 
        /// Зависит от:
        /// - Текущей погоды (дождь = высокая влажность)
        /// - Времени суток (ночь = выше влажность)
        /// - Случайных флуктуаций
        /// </summary>
        private void UpdateHumidity()
        {
            float baseHumidity = 0.5f;

            // Дождь повышает влажность
            if (CurrentWeather == WeatherCondition.Rain)
            {
                baseHumidity = 0.8f;
            }

            // Ночь повышает влажность
            if (DayProgress < 0.25f || DayProgress > 0.75f)
            {
                baseHumidity += 0.1f;
            }

            // Случайная флуктуация
            float fluctuation = (float)(_random.NextDouble() - 0.5) * 0.1f;

            Humidity = Math.Max(0, Math.Min(1, baseHumidity + fluctuation));
        }

        /// <summary>
        /// Обновляет текущее погодное условие.
        /// 
        /// Вероятности смены погоды:
        /// - Clear: 60%
        /// - Rain: 20%
        /// - Storm: 10%
        /// - Fog: 10%
        /// 
        /// Шторм возможен только при высокой влажности.
        /// </summary>
        private void UpdateWeather()
        {
            var oldWeather = CurrentWeather;
            double roll = _random.NextDouble();

            // Шторм только при высокой влажности
            bool canStorm = Humidity > 0.7f;

            if (roll < 0.6f)
            {
                CurrentWeather = WeatherCondition.Clear;
            }
            else if (roll < 0.8f)
            {
                CurrentWeather = WeatherCondition.Rain;
            }
            else if (roll < 0.9f && canStorm)
            {
                CurrentWeather = WeatherCondition.Storm;
            }
            else
            {
                CurrentWeather = WeatherCondition.Fog;
            }

            // Уведомляем подписчиков если погода изменилась
            if (oldWeather != CurrentWeather)
            {
                OnWeatherChanged?.Invoke(oldWeather, CurrentWeather);
            }
        }

        /// <summary>
        /// Обновляет параметры ветра.
        /// 
        /// Сила ветра зависит от погоды:
        /// - Clear: 0.0 - 0.3
        /// - Rain: 0.2 - 0.5
        /// - Storm: 0.6 - 1.0
        /// - Fog: 0.0 - 0.1
        /// 
        /// Направление меняется случайно с инерцией.
        /// </summary>
        private void UpdateWind()
        {
            // Базовая сила ветра в зависимости от погоды
            float minStrength = CurrentWeather switch
            {
                WeatherCondition.Storm => 0.6f,
                WeatherCondition.Rain => 0.2f,
                WeatherCondition.Fog => 0.0f,
                _ => 0.0f
            };

            float maxStrength = CurrentWeather switch
            {
                WeatherCondition.Storm => 1.0f,
                WeatherCondition.Rain => 0.5f,
                WeatherCondition.Fog => 0.1f,
                _ => 0.3f
            };

            WindStrength = (float)(minStrength + _random.NextDouble() * (maxStrength - minStrength));

            // Направление ветра (0-360 градусов)
            // Небольшая инерция: новое направление близко к старому
            float directionChange = (float)(_random.NextDouble() - 0.5) * 90;
            WindDirection = (WindDirection + directionChange + 360) % 360;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Проверяет является ли текущий момент днём.
        /// </summary>
        /// <returns>True если день (0.25 - 0.75 прогресса дня).</returns>
        public bool IsDaytime()
        {
            return DayProgress >= 0.25f && DayProgress <= 0.75f;
        }

        /// <summary>
        /// Проверяет является ли текущий момент ночью.
        /// </summary>
        /// <returns>True если ночь (вне диапазона 0.25 - 0.75).</returns>
        public bool IsNighttime()
        {
            return !IsDaytime();
        }

        /// <summary>
        /// Получает вектор ветра в координатах мира.
        /// </summary>
        /// <returns>Кортеж (dx, dy) нормализованный от -1 до 1.</returns>
        public (float dx, float dy) GetWindVector()
        {
            double radians = WindDirection * Math.PI / 180;
            float dx = (float)Math.Sin(radians);
            float dy = -(float)Math.Cos(radians);
            return (dx * WindStrength, dy * WindStrength);
        }

        /// <summary>
        /// Рассчитывает модификатор расхода энергии из-за погоды.
        /// 
        /// Формула:
        /// 1.0 + |Temperature - Optimal| * Factor + WindStrength * Factor
        /// 
        /// Пример:
        /// - Оптимальные условия: 1.0 (нормальный расход)
        /// - Холод + ветер: 1.5 (расход +50%)
        /// - Жара + шторм: 2.0 (расход +100%)
        /// </summary>
        /// <returns>Модификатор расхода энергии (>= 1.0).</returns>
        public float GetEnergyCostModifier()
        {
            float optimalTemp = 20.0f;
            float tempFactor = 0.02f;
            float windFactor = 0.3f;

            float tempPenalty = Math.Abs(Temperature - optimalTemp) * tempFactor;
            float windPenalty = WindStrength * windFactor;

            return 1.0f + tempPenalty + windPenalty;
        }

        /// <summary>
        /// Проверяет может ли бот быть сдут ветром.
        /// 
        /// Вероятность сдувания зависит от:
        /// - Силы ветра
        /// - Энергии бота (слабые боты сдуваются легче)
        /// - Текущей погоды (шторм = выше вероятность)
        /// </summary>
        /// <param name="botEnergy">
        /// Текущая энергия бота.
        /// </param>
        /// <param name="maxEnergy">
        /// Максимальная энергия бота.
        /// </param>
        /// <returns>
        /// True если бот будет сдут в этом тике.
        /// </returns>
        public bool WillBotBeBlownAway(float botEnergy, float maxEnergy)
        {
            // Шторм всегда может сдуть
            if (CurrentWeather == WeatherCondition.Storm && WindStrength > 0.8f)
            {
                return true;
            }

            // Расчёт вероятности сдувания
            float weaknessFactor = 1.0f - (botEnergy / maxEnergy);
            float blowChance = WindStrength * weaknessFactor * 0.5f;

            return _random.NextDouble() < blowChance;
        }

        /// <summary>
        /// Получает направление сдувания бота ветром.
        /// </summary>
        /// <returns>
        /// Кортеж (dx, dy) направления сдувания.
        /// </returns>
        public (int dx, int dy) GetBlowDirection()
        {
            var (windDx, windDy) = GetWindVector();
            return (
                (int)Math.Sign(windDx),
                (int)Math.Sign(windDy)
            );
        }

        #endregion
    }
}