using System;
using GameOfLife.Resources;

namespace GameOfLife.Genome
{
    /// <summary>
    /// Тип действия, которое может выполнить бот.
    /// 
    /// Определяет набор доступных команд в системе принятия решений.
    /// Каждое действие имеет свою стоимость и условия выполнения.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Перемещение в соседнюю клетку.
        /// Требует: энергия, свободная целевая клетка.
        /// </summary>
        Move,

        /// <summary>
        /// Сбор ресурсов в текущей клетке.
        /// Требует: наличие ресурса, свободное место в инвентаре.
        /// </summary>
        Gather,

        /// <summary>
        /// Атака на бота в соседней клетке.
        /// Требует: энергия, наличие цели, достаточная агрессия.
        /// </summary>
        Attack,

        /// <summary>
        /// Строительство структуры в текущей клетке.
        /// Требует: энергия, строительные материалы в инвентаре.
        /// </summary>
        Build,

        /// <summary>
        /// Конвертация ресурсов по рецепту.
        /// Требует: исходные ресурсы, знание рецепта.
        /// </summary>
        Convert,

        /// <summary>
        /// Отдых: восстановление энергии без действий.
        /// Требует: безопасное окружение.
        /// </summary>
        Rest,

        /// <summary>
        /// Размножение: создание потомка.
        /// Требует: достаточная энергия, свободная соседняя клетка.
        /// </summary>
        Reproduce,

        /// <summary>
        /// Бегство от угрозы.
        /// Требует: энергия, доступное направление отхода.
        /// </summary>
        Flee
    }

    /// <summary>
    /// Решение, принятое системой поведения бота.
    /// 
    /// Этот класс инкапсулирует результат процесса принятия решения:
    /// какое действие выполнить, где, с какими параметрами.
    /// 
    /// Решение создаётся методом Genome.Decide() на основе:
    /// - Текущего состояния бота (энергия, здоровье, инвентарь)
    /// - Контекста окружения (угрозы, ресурсы, другие боты)
    /// - Параметров генома (склонности, программа поведения)
    /// - Случайных факторов (для стохастического поведения)
    /// </summary>
    public class BehaviorDecision
    {
        /// <summary>
        /// Тип действия для выполнения.
        /// </summary>
        public ActionType Action { get; set; }

        /// <summary>
        /// Целевая координата X (если применимо).
        /// Используется для Move, Attack, Build.
        /// </summary>
        public int TargetX { get; set; }

        /// <summary>
        /// Целевая координата Y (если применимо).
        /// Используется для Move, Attack, Build.
        /// </summary>
        public int TargetY { get; set; }

        /// <summary>
        /// Тип ресурса для действия (если применимо).
        /// Используется для Gather, Convert.
        /// </summary>
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Количество ресурса (если применимо).
        /// Используется для Convert, Build.
        /// </summary>
        public float ResourceAmount { get; set; }

        /// <summary>
        /// Приоритет решения (0-100).
        /// Используется при выборе между несколькими возможными действиями.
        /// Более высокое значение = более предпочтительное действие.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Создаёт решение по умолчанию (бездействие).
        /// </summary>
        public BehaviorDecision()
        {
            Action = ActionType.Rest;
            Priority = 0;
        }

        /// <summary>
        /// Создаёт решение для перемещения.
        /// </summary>
        public static BehaviorDecision MoveTo(int x, int y, int priority = 50)
        {
            return new BehaviorDecision
            {
                Action = ActionType.Move,
                TargetX = x,
                TargetY = y,
                Priority = priority
            };
        }

        /// <summary>
        /// Создаёт решение для сбора ресурса.
        /// </summary>
        public static BehaviorDecision GatherResource(ResourceType type, int priority = 60)
        {
            return new BehaviorDecision
            {
                Action = ActionType.Gather,
                ResourceType = type,
                Priority = priority
            };
        }

        /// <summary>
        /// Создаёт решение для атаки.
        /// </summary>
        public static BehaviorDecision AttackTarget(int x, int y, int priority = 70)
        {
            return new BehaviorDecision
            {
                Action = ActionType.Attack,
                TargetX = x,
                TargetY = y,
                Priority = priority
            };
        }

        /// <summary>
        /// Создаёт решение для отдыха.
        /// </summary>
        public static BehaviorDecision RestAction(int priority = 30)
        {
            return new BehaviorDecision
            {
                Action = ActionType.Rest,
                Priority = priority
            };
        }

        /// <summary>
        /// Создаёт решение для бегства.
        /// </summary>
        public static BehaviorDecision FleeTo(int x, int y, int priority = 90)
        {
            return new BehaviorDecision
            {
                Action = ActionType.Flee,
                TargetX = x,
                TargetY = y,
                Priority = priority
            };
        }

        /// <summary>
        /// Проверяет, требует ли решение целевых координат.
        /// </summary>
        public bool RequiresTarget =>
            Action == ActionType.Move ||
            Action == ActionType.Attack ||
            Action == ActionType.Build ||
            Action == ActionType.Flee;

        /// <summary>
        /// Проверяет, требует ли решение типа ресурса.
        /// </summary>
        public bool RequiresResource =>
            Action == ActionType.Gather ||
            Action == ActionType.Convert;

        /// <summary>
        /// Возвращает строковое представление решения для отладки.
        /// </summary>
        public override string ToString()
        {
            string target = RequiresTarget ? $"({TargetX},{TargetY})" : "";
            string resource = RequiresResource ? $"[{ResourceType?.Name}]" : "";
            return $"{Action}{target}{resource} (P:{Priority})";
        }
    }
}