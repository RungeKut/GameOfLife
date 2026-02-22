using System;
using System.Linq;
using GameOfLife.Entities;
using GameOfLife.Resources;

namespace GameOfLife.Genome
{
    /// <summary>
    /// Методы расширения для класса Genome.
    /// 
    /// Выносят логику принятия решений из основного класса Genome,
    /// улучшая разделение ответственности и тестируемость.
    /// </summary>
    public static class GenomeExtensions
    {
        /// <summary>
        /// Принимает решение о следующем действии бота.
        /// 
        /// Этот метод реализует "мозг" бота: анализирует текущую ситуацию
        /// и выбирает наиболее приоритетное действие на основе генома.
        /// 
        /// Алгоритм:
        /// 1. Сбор информации о контексте (энергия, угрозы, ресурсы)
        /// 2. Генерация возможных действий с приоритетами
        /// 3. Выбор действия с наивысшим приоритетом
        /// 4. Добавление стохастичности для естественного поведения
        /// 
        /// Метод детерминирован при фиксированном seed Random,
        /// что важно для воспроизводимости симуляций.
        /// </summary>
        /// <param name="genome">Геном, принимающий решение.</param>
        /// <param name="bot">Бот, для которого принимается решение.</param>
        /// <param name="context">Контекст поведения (ситуация).</param>
        /// <returns>Решение с действием для выполнения.</returns>
        public static BehaviorDecision Decide(
            this Genome genome,
            Bot bot,
            BehaviorContext context)
        {
            if (genome == null)
                return BehaviorDecision.RestAction();

            // Словарь возможных действий и их приоритетов
            var candidates = new System.Collections.Generic.List<(BehaviorDecision, int)>();

            // Генерируем действия на основе контекста
            switch (context)
            {
                case BehaviorContext.Threatened:
                    AddFleeActions(candidates, genome, bot);
                    AddDefensiveActions(candidates, genome, bot);
                    break;

                case BehaviorContext.Hungry:
                    AddGatherActions(candidates, genome, bot);
                    AddHuntingActions(candidates, genome, bot);
                    break;

                case BehaviorContext.Reproducing:
                    AddReproductionActions(candidates, genome, bot);
                    AddEnergyGatheringActions(candidates, genome, bot);
                    break;

                case BehaviorContext.Gathering:
                    AddGatherActions(candidates, genome, bot);
                    AddExplorationActions(candidates, genome, bot);
                    break;

                case BehaviorContext.Building:
                    AddBuildingActions(candidates, genome, bot);
                    AddResourceGatheringActions(candidates, genome, bot);
                    break;

                case BehaviorContext.Idle:
                default:
                    AddExplorationActions(candidates, genome, bot);
                    AddMaintenanceActions(candidates, genome, bot);
                    break;
            }

            // Выбираем действие с наивысшим приоритетом
            if (candidates.Count == 0)
                return BehaviorDecision.RestAction();

            // Добавляем небольшую случайность для естественности
            var random = new Random();
            var best = candidates.OrderByDescending(c => c.Item2 + random.Next(10)).First();

            return best.Item1;
        }

        #region Генераторы действий по контекстам

        /// <summary>
        /// Добавляет действия бегства при угрозе.
        /// </summary>
        private static void AddFleeActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            // Бегство имеет высокий приоритет при угрозе
            int basePriority = genome.ActionPriorities.ContainsKey(BehaviorContext.Threatened)
                ? genome.ActionPriorities[BehaviorContext.Threatened]
                : 90;

            // Находим направление от предполагаемой угрозы
            // (в полной реализации: анализ окружения)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int newX = bot.X + dx;
                    int newY = bot.Y + dy;

                    // Проверяем валидность клетки (упрощённо)
                    if (newX >= 0 && newX < 100 && newY >= 0 && newY < 100)
                    {
                        candidates.Add((
                            BehaviorDecision.FleeTo(newX, newY, basePriority),
                            basePriority
                        ));
                    }
                }
            }
        }

        /// <summary>
        /// Добавляет действия сбора ресурсов.
        /// </summary>
        private static void AddGatherActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            int basePriority = genome.ActionPriorities.ContainsKey(BehaviorContext.Hungry)
                ? genome.ActionPriorities[BehaviorContext.Hungry]
                : 80;

            // Предпочитаем ресурсы согласно геному
            foreach (var pref in genome.ResourcePreferences.OrderByDescending(p => p.Value))
            {
                if (pref.Value > 0.5f) // Только предпочтительные ресурсы
                {
                    candidates.Add((
                        BehaviorDecision.GatherResource(pref.Key, basePriority),
                        (int)(basePriority * pref.Value)
                    ));
                }
            }
        }

        /// <summary>
        /// Добавляет действия исследования и перемещения.
        /// </summary>
        private static void AddExplorationActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            int basePriority = 40;

            // Используем публичный метод бота для получения предпочтительного направления
            var (dx, dy) = bot.GetPreferredMovementDelta();

            if (dx != 0 || dy != 0) // Если есть предпочтительное направление
            {
                int newX = bot.X + dx;
                int newY = bot.Y + dy;

                // Проверяем валидность клетки (упрощённо — в полной реализации запрос к движку)
                if (newX >= 0 && newX < 100 && newY >= 0 && newY < 100)
                {
                    candidates.Add((
                        BehaviorDecision.MoveTo(newX, newY, basePriority),
                        basePriority
                    ));
                }
            }
        }

        /// <summary>
        /// Добавляет действия для накопления энергии.
        /// </summary>
        private static void AddEnergyGatheringActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            if (bot.Energy < genome.MaxEnergy * genome.ReproductionThreshold)
            {
                AddGatherActions(candidates, genome, bot);
            }
        }

        /// <summary>
        /// Добавляет действия размножения.
        /// </summary>
        private static void AddReproductionActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            if (genome.CanReproduce(bot.Energy))
            {
                int priority = genome.ActionPriorities.ContainsKey(BehaviorContext.Reproducing)
                    ? genome.ActionPriorities[BehaviorContext.Reproducing]
                    : 70;

                candidates.Add((
                    BehaviorDecision.RestAction(priority), // Подготовка к размножению
                    priority
                ));
            }
        }

        /// <summary>
        /// Добавляет действия отдыха и восстановления.
        /// </summary>
        private static void AddMaintenanceActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            if (bot.Energy < genome.MaxEnergy * 0.8f)
            {
                candidates.Add((
                    BehaviorDecision.RestAction(35),
                    35
                ));
            }
        }

        /// <summary>
        /// Добавляет защитные действия (контратака).
        /// </summary>
        private static void AddDefensiveActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            if (genome.Aggression > 0.5f && bot.Health > genome.MaxHealth * 0.5f)
            {
                candidates.Add((
                    BehaviorDecision.AttackTarget(bot.X, bot.Y, 60),
                    (int)(60 * genome.Aggression)
                ));
            }
        }

        /// <summary>
        /// Добавляет действия охоты на других ботов.
        /// </summary>
        private static void AddHuntingActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            if (genome.Aggression > 0.3f)
            {
                candidates.Add((
                    BehaviorDecision.AttackTarget(bot.X, bot.Y, 40),
                    (int)(40 * genome.Aggression)
                ));
            }
        }

        /// <summary>
        /// Добавляет действия сбора ресурсов для строительства.
        /// </summary>
        private static void AddResourceGatheringActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            // Предпочитаем строительные материалы
            if (ResourceType.Metal != null &&
                genome.ResourcePreferences.TryGetValue(ResourceType.Metal, out var pref) &&
                pref > 0.5f)
            {
                candidates.Add((
                    BehaviorDecision.GatherResource(ResourceType.Metal, 50),
                    50
                ));
            }
        }

        /// <summary>
        /// Добавляет действия строительства.
        /// </summary>
        private static void AddBuildingActions(
            System.Collections.Generic.List<(BehaviorDecision, int)> candidates,
            Genome genome, Bot bot)
        {
            // В полной реализации: проверка наличия материалов и места
            if (bot.Inventory.HasEnough(ResourceType.Metal, 5))
            {
                candidates.Add((
                    BehaviorDecision.RestAction(45), // Подготовка к строительству
                    45
                ));
            }
        }

        #endregion
    }
}