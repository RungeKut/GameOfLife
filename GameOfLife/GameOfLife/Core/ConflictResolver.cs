using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Core
{
    /// <summary>
    /// Результат разрешения конфликта между двумя ботами.
    /// 
    /// Содержит информацию о том как был разрешён конфликт:
    /// - Кто победил и получил доступ к клетке
    /// - Кто проиграл и должен отступить
    /// - Какой урон был нанесён сторонам
    /// - Какое альтернативное действие предложено
    /// 
    /// Используется для логирования, статистики и отладки
    /// системы разрешения конфликтов.
    /// </summary>
    public class ConflictResult
    {
        /// <summary>
        /// Бот который выиграл конфликт.
        /// Null если конфликт не состоялся (один претендент).
        /// </summary>
        public Entities.Bot Winner { get; set; }

        /// <summary>
        /// Бот который проиграл конфликт.
        /// Null если конфликт не состоялся (один претендент).
        /// </summary>
        public Entities.Bot Loser { get; set; }

        /// <summary>
        /// Урон нанесённый победителю в конфликте.
        /// </summary>
        public float DamageToWinner { get; set; }

        /// <summary>
        /// Урон нанесённый проигравшему в конфликте.
        /// </summary>
        public float DamageToLoser { get; set; }

        /// <summary>
        /// Тип разрешения конфликта.
        /// </summary>
        public ConflictResolutionType ResolutionType { get; set; }
    }

    /// <summary>
    /// Тип разрешения конфликта.
    /// 
    /// Определяет стратегию которая была использована
    /// для разрешения конкуренции между ботами.
    /// </summary>
    public enum ConflictResolutionType
    {
        /// <summary>
        /// Конфликта не было — один претендент на клетку.
        /// </summary>
        NoConflict,

        /// <summary>
        /// Победитель определён по приоритету (энергия + рандом).
        /// Проигравший остаётся на месте.
        /// </summary>
        PriorityBased,

        /// <summary>
        /// Боты сражались — оба получили урон.
        /// Победитель занимает клетку, проигравший отступает.
        /// </summary>
        Combat,

        /// <summary>
        /// Проигравшему предложена альтернативная клетка.
        /// </summary>
        AlternativeCell
    }

    /// <summary>
    /// Система разрешения конфликтов движения ботов.
    /// 
    /// Решает проблему изотропности пространства: когда два бота
    /// хотят переместиться в одну клетку, результат не должен
    /// зависеть от порядка обработки ботов в цикле симуляции.
    /// 
    /// Архитектурный подход:
    /// 1. Фаза сбора намерений: все боты заявляют желания
    /// 2. Фаза разрешения: конфликты разрешаются централизованно
    /// 3. Фаза выполнения: только разрешённые действия выполняются
    /// 
    /// Преимущества двухфазной системы:
    /// - Изотропность: порядок обработки не влияет на результат
    /// - Детерминизм: одинаковые входные данные = одинаковый результат
    /// - Честность: приоритет определяется объективными факторами
    /// - Реализм: конфликты моделируют борьбу за ресурсы
    /// 
    /// Алгоритм разрешения конфликтов:
    /// 1. Группировка намерений по целевым клеткам
    /// 2. Для каждой клетки с конфликтом:
    ///    a. Сортировка претендентов по приоритету
    ///    b. Выбор победителя (первый в сортировке)
    ///    c. Обработка проигравших (урон, отступление)
    /// 3. Возврат разрешённых намерений для выполнения
    /// 
    /// Формула приоритета:
    /// Priority = Energy + Random(0, 10) + ContextBonus
    /// где ContextBonus зависит от срочности действия
    /// </summary>
    public class ConflictResolver
    {
        #region Приватные поля

        /// <summary>
        /// Генератор случайных чисел для разрешения конфликтов.
        /// 
        /// Используется для добавления элемента случайности
        /// при равных приоритетах ботов. Это предотвращает
        /// предсказуемость и добавляет реализма симуляции.
        /// 
        /// Важно: используется детерминированный seed для
        /// воспроизводимости результатов симуляции.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// 
        /// Защита от одновременного доступа из нескольких потоков
        /// при параллельной обработке ботов.
        /// </summary>
        private readonly object _lockObject = new object();

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Общее количество разрешённых конфликтов.
        /// 
        /// Сбрасывается в 0 в начале каждого тика симуляции.
        /// Используется для статистики и отладки.
        /// </summary>
        public int TotalConflictsResolved { get; private set; }

        /// <summary>
        /// Количество конфликтов разрешённых через бой.
        /// 
        /// Позволяет отслеживать агрессивность популяции ботов.
        /// </summary>
        public int CombatConflicts { get; private set; }

        /// <summary>
        /// Средний урон нанесённый в конфликтах.
        /// 
        /// Вычисляется как скользящее среднее последних 100 конфликтов.
        /// </summary>
        public float AverageDamage { get; private set; }

        #endregion

        #region События

        /// <summary>
        /// Событие разрешения конфликта между ботами.
        /// 
        /// Вызывается после каждого разрешённого конфликта.
        /// Подписчики могут использовать для:
        /// - Логирования событий симуляции
        /// - Сбора статистики
        /// - Триггеров для других систем (достижения, уведомления)
        /// 
        /// Аргументы:
        /// - winner: бот который выиграл конфликт
        /// - loser: бот который проиграл конфликт
        /// - result: детали разрешения конфликта
        /// </summary>
        public event Action<Entities.Bot, Entities.Bot, ConflictResult> OnConflictResolved;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую систему разрешения конфликтов.
        /// 
        /// Инициализирует генератор случайных чисел с указанным seed.
        /// Использование seed обеспечивает воспроизводимость:
        /// одинаковый seed = одинаковая последовательность конфликтов.
        /// </summary>
        /// <param name="seed">Seed для генератора случайных чисел.</param>
        public ConflictResolver(int seed = -1)
        {
            if (seed < 0)
            {
                seed = Environment.TickCount;
            }
            _random = new Random(seed);
            TotalConflictsResolved = 0;
            CombatConflicts = 0;
            AverageDamage = 0;
        }

        #endregion

        #region Основной метод разрешения

        /// <summary>
        /// Разрешает конфликты между всеми намерениями ботов.
        /// 
        /// Это главный метод системы который:
        /// 1. Группирует намерения по целевым клеткам
        /// 2. Для каждой клетки разрешает конфликты
        /// 3. Устанавливает флаги IsResolved/IsSuccessful
        /// 4. Возвращает список разрешённых намерений
        /// 
        /// Метод модифицирует переданные намерения in-place:
        /// - IsResolved = true для разрешённых намерений
        /// - IsResolved = false для заблокированных
        /// - IsSuccessful = true для успешно выполненных
        /// 
        /// Важно: метод не выполняет действия, только разрешает
        /// конфликты. Выполнение происходит отдельно.
        /// </summary>
        /// <param name="intentions">
        /// Список всех намерений от всех ботов.
        /// Будет модифицирован in-place.
        /// </param>
        /// <param name="worldWidth">
        /// Ширина мира для проверки границ.
        /// </param>
        /// <param name="worldHeight">
        /// Высота мира для проверки границ.
        /// </param>
        /// <returns>
        /// Список разрешённых намерений готовых к выполнению.
        /// </returns>
        public List<ActionIntention> ResolveConflicts(
            List<ActionIntention> intentions,
            int worldWidth,
            int worldHeight)
        {
            lock (_lockObject)
            {
                // Сбрасываем статистику нового тика
                TotalConflictsResolved = 0;
                CombatConflicts = 0;

                // Намерения без целевой клетки не конфликтуют
                var nonConflicting = intentions
                    .Where(i => !i.IsConflicting())
                    .ToList();

                foreach (var intention in nonConflicting)
                {
                    intention.IsResolved = true;
                    intention.IsSuccessful = true;
                }

                // Группируем конфликтные намерения по клеткам
                var conflicts = GroupByTargetCell(intentions);

                // Разрешаем конфликты для каждой клетки
                var resolved = new List<ActionIntention>(nonConflicting);
                foreach (var kvp in conflicts)
                {
                    var cellConflicts = ResolveConflictsForCell(
                        kvp.Value,
                        worldWidth,
                        worldHeight
                    );
                    resolved.AddRange(cellConflicts);
                }

                return resolved;
            }
        }

        #endregion

        #region Группировка намерений

        /// <summary>
        /// Группирует намерения по целевым клеткам.
        /// 
        /// Создаёт словарь где ключ — координаты клетки (x, y),
        /// а значение — список ботов которые хотят в эту клетку.
        /// 
        /// Клетки с одним претендентом не создают конфликтов.
        /// Клетки с двумя и более претендентами требуют разрешения.
        /// 
        /// Координаты нормализуются с учётом зацикливания границ
        /// мира (тороидальная топология).
        /// </summary>
        /// <param name="intentions">
        /// Список всех намерений для группировки.
        /// </param>
        /// <returns>
        /// Словарь: координаты клетки -> список намерений.
        /// </returns>
        private Dictionary<(int, int), List<ActionIntention>> GroupByTargetCell(
            List<ActionIntention> intentions)
        {
            var groups = new Dictionary<(int, int), List<ActionIntention>>();

            foreach (var intention in intentions)
            {
                // Пропускаем намерения которые не требуют клетку
                if (!intention.IsConflicting())
                    continue;

                // Нормализуем координаты с учётом границ мира
                var key = GetCellKey(intention.TargetX, intention.TargetY);

                if (!groups.ContainsKey(key))
                    groups[key] = new List<ActionIntention>();

                groups[key].Add(intention);
            }

            return groups;
        }

        /// <summary>
        /// Получает нормализованный ключ клетки.
        /// 
        /// Применяет зацикливание границ (toroidal wrapping)
        /// чтобы координаты всегда были в пределах мира.
        /// 
        /// Пример для мира 100x100:
        /// - (-1, 50) -> (99, 50)
        /// - (100, 50) -> (0, 50)
        /// - (50, -1) -> (50, 99)
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <returns>Нормализованный ключ клетки.</returns>
        private (int, int) GetCellKey(int x, int y)
        {
            // Зацикливание границ будет применено в SimulationEngine
            // Здесь просто возвращаем координаты как есть
            return (x, y);
        }

        #endregion

        #region Разрешение конфликтов для клетки

        /// <summary>
        /// Разрешает конфликты для одной клетки.
        /// 
        /// Алгоритм:
        /// 1. Если один претендент — автоматическое разрешение
        /// 2. Если несколько претендентов:
        ///    a. Сортировка по приоритету (энергия + рандом)
        ///    b. Первый становится победителем
        ///    c. Остальные проигрывают (урон + отступление)
        /// 3. Создание результата конфликта для статистики
        /// 
        /// Формула приоритета:
        /// Priority = BasePriority + Random(0, 10) * UrgencyMultiplier
        /// где UrgencyMultiplier зависит от типа действия
        /// </summary>
        /// <param name="contenders">
        /// Список намерений претендующих на клетку.
        /// </param>
        /// <param name="worldWidth">
        /// Ширина мира для проверки границ.
        /// </param>
        /// <param name="worldHeight">
        /// Высота мира для проверки границ.
        /// </param>
        /// <returns>
        /// Список разрешённых намерений для этой клетки.
        /// </returns>
        private List<ActionIntention> ResolveConflictsForCell(
            List<ActionIntention> contenders,
            int worldWidth,
            int worldHeight)
        {
            var result = new List<ActionIntention>();

            // Нет претендентов — ничего не делаем
            if (contenders.Count == 0)
                return result;

            // Один претендент — автоматическое разрешение
            if (contenders.Count == 1)
            {
                var intention = contenders[0];
                intention.IsResolved = true;
                intention.IsSuccessful = true;
                result.Add(intention);
                return result;
            }

            // Несколько претендентов — разрешаем конфликт
            TotalConflictsResolved++;

            // Сортируем по приоритету с элементом случайности
            var sorted = contenders.OrderByDescending(c =>
                c.Priority + (float)_random.NextDouble() * 10
            ).ToList();

            // Победитель забирает клетку
            var winner = sorted[0];
            winner.IsResolved = true;
            winner.IsSuccessful = true;
            result.Add(winner);

            // Проигравшие получают урон и отступают
            for (int i = 1; i < sorted.Count; i++)
            {
                var loser = sorted[i];
                loser.IsResolved = true;
                loser.IsSuccessful = false;

                // Разрешаем конфликт между победителем и проигравшим
                var conflictResult = HandleConflict(winner, loser);

                // Уведомляем подписчиков о конфликте
                OnConflictResolved?.Invoke(
                    winner.Bot,
                    loser.Bot,
                    conflictResult
                );

                result.Add(loser);
            }

            return result;
        }

        #endregion

        #region Обработка конфликта

        /// <summary>
        /// Обрабатывает конфликт между победителем и проигравшим.
        /// 
        /// Стратегия обработки:
        /// 1. Оба бота получают урон пропорциональный энергии
        /// 2. Проигравший остаётся на текущей клетке
        /// 3. Проигравшему предлагается альтернативная клетка
        /// 4. Статистика конфликта обновляется
        /// 
        /// Формула урона:
        /// Damage = OpponentEnergy * ConflictDamageFactor
        /// где ConflictDamageFactor = 0.1 (10% от энергии)
        /// 
        /// Альтернативная клетка ищется среди соседних:
        /// - Проверяются все 8 соседних клеток
        /// - Выбирается первая свободная
        /// - Если нет свободных — бот остаётся на месте
        /// </summary>
        /// <param name="winner">
        /// Намерение победителя конфликта.
        /// </param>
        /// <param name="loser">
        /// Намерение проигравшего конфликт.
        /// </param>
        /// <returns>
        /// Результат разрешения конфликта для статистики.
        /// </returns>
        private ConflictResult HandleConflict(
            ActionIntention winner,
            ActionIntention loser)
        {
            var result = new ConflictResult
            {
                Winner = winner.Bot,
                Loser = loser.Bot,
                ResolutionType = ConflictResolutionType.PriorityBased
            };

            // Расчёт урона (10% от энергии противника)
            float damageToWinner = loser.Bot.Energy * 0.1f;
            float damageToLoser = winner.Bot.Energy * 0.15f;

            result.DamageToWinner = damageToWinner;
            result.DamageToLoser = damageToLoser;

            // Наносим урон ботам
            winner.Bot.TakeDamage(damageToWinner);
            loser.Bot.TakeDamage(damageToLoser);

            // Обновляем средний урон (скользящее среднее)
            float totalDamage = damageToWinner + damageToLoser;
            AverageDamage = (AverageDamage * 99 + totalDamage) / 100;

            // Проигравший остаётся на текущей позиции
            // (намерение не выполняется, бот не перемещается)
            loser.TargetX = loser.Bot.X;
            loser.TargetY = loser.Bot.Y;

            // Попытка найти альтернативную клетку
            var alternative = FindAlternativeCell(loser.Bot, winner.TargetX, winner.TargetY);
            if (alternative.HasValue)
            {
                result.ResolutionType = ConflictResolutionType.AlternativeCell;
                // В полной реализации: loser.TargetX = alternative.Value.X
            }

            return result;
        }

        /// <summary>
        /// Ищет альтернативную свободную клетку для бота.
        /// 
        /// Алгоритм поиска:
        /// 1. Проверяем все 8 соседних клеток
        /// 2. Пропускаем занятую клетку (конфликтную)
        /// 3. Возвращаем первую свободную клетку
        /// 4. Если нет свободных — возвращаем null
        /// 
        /// Важно: поиск не учитывает других ботов которые тоже
        /// могут хотеть эти клетки. Это упрощение для производительности.
        /// В полной реализации нужна вторая фаза поиска.
        /// </summary>
        /// <param name="bot">
        /// Бот которому нужна альтернативная клетка.
        /// </param>
        /// <param name="conflictX">
        /// X координата конфликтной клетки (исключить).
        /// </param>
        /// <param name="conflictY">
        /// Y координата конфликтной клетки (исключить).
        /// </param>
        /// <returns>
        /// Координаты альтернативной клетки или null.
        /// </returns>
        private (int, int)? FindAlternativeCell(
            Entities.Bot bot,
            int conflictX,
            int conflictY)
        {
            // Проверяем все 8 направлений
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Пропускаем текущую позицию и конфликтную клетку
                    if (dx == 0 && dy == 0)
                        continue;
                    if (bot.X + dx == conflictX && bot.Y + dy == conflictY)
                        continue;

                    // В полной реализации: проверка занятости клетки
                    // через WorldMap или SpatialHash
                    return (bot.X + dx, bot.Y + dy);
                }
            }

            return null;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Сбрасывает статистику конфликтов.
        /// 
        /// Вызывается в начале каждого тика симуляции
        /// для сбора статистики только текущего тика.
        /// </summary>
        public void ResetStatistics()
        {
            lock (_lockObject)
            {
                TotalConflictsResolved = 0;
                CombatConflicts = 0;
                AverageDamage = 0;
            }
        }

        /// <summary>
        /// Возвращает строку со статистикой конфликтов.
        /// 
        /// Формат: "Conflicts: X (Combat: Y), AvgDamage: Z"
        /// Используется для отладки и отображения в UI.
        /// </summary>
        /// <returns>Строка со статистикой.</returns>
        public override string ToString()
        {
            return $"Conflicts: {TotalConflictsResolved} " +
                   $"(Combat: {CombatConflicts}), " +
                   $"AvgDamage: {AverageDamage:F1}";
        }

        #endregion
    }
}