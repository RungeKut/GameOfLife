using System;
using System.Collections.Generic;

namespace GameOfLife.Resources
{
    /// <summary>
    /// Рецепт конвертации ресурсов.
    /// 
    /// Определяет как одни ресурсы могут быть преобразованы в другие.
    /// Используется ботами для крафта, переработки и создания новых материалов.
    /// 
    /// Архитектурные особенности:
    /// - Поддержка множественных входных и выходных ресурсов
    /// - Учёт времени выполнения конвертации
    /// - Требования к навыкам бота (опционально)
    /// - Вероятность успеха (для стохастических рецептов)
    /// 
    /// Примеры рецептов:
    /// - 10 Food + 5 Water → 3 Biomass
    /// - 5 Metal + 2 Energy → 1 Tool
    /// - 1 Radioactive → 50 Energy (с риском мутации)
    /// 
    /// Использование:
    /// var recipe = new ConversionRecipe("BiomassCraft", 10);
    /// recipe.AddInput(ResourceType.Food, 10);
    /// recipe.AddOutput(ResourceType.Biomass, 3);
    /// </summary>
    public class ConversionRecipe : IEquatable<ConversionRecipe>
    {
        #region Приватные поля

        /// <summary>
        /// Уникальный идентификатор рецепта.
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// Входные ресурсы для конвертации.
        /// Ключ: тип ресурса, Значение: количество.
        /// </summary>
        private readonly Dictionary<ResourceType, float> _inputs;

        /// <summary>
        /// Выходные ресурсы после конвертации.
        /// Ключ: тип ресурса, Значение: количество.
        /// </summary>
        private readonly Dictionary<ResourceType, float> _outputs;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Уникальный идентификатор рецепта.
        /// 
        /// Используется для:
        /// - Сохранения изученных рецептов ботом
        /// - Поиска рецепта в базе рецептов
        /// - Сериализации при сохранении
        /// </summary>
        public string Id => _id;

        /// <summary>
        /// Человеко-читаемое название рецепта.
        /// 
        /// Примеры: "Создание биомассы", "Переработка металла"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Время выполнения конвертации в тиках симуляции.
        /// 
        /// 0 = мгновенная конвертация
        /// 1+ = требуется время для выполнения
        /// 
        /// Во время конвертации бот не может выполнять другие действия.
        /// </summary>
        public int ConversionTime { get; set; } = 0;

        /// <summary>
        /// Вероятность успеха конвертации (0.0 - 1.0).
        /// 
        /// 1.0 = всегда успешно
        /// 0.5 = 50% шанс успеха
        /// 
        /// При неудаче ресурсы теряются без получения выхода.
        /// </summary>
        public float SuccessChance { get; set; } = 1.0f;

        /// <summary>
        /// Минимальный навык бота для выполнения рецепта.
        /// 
        /// 0 = может выполнить любой бот
        /// 1+ = требуется определённый уровень навыка
        /// 
        /// Используется для сложных рецептов которые требуют
        /// эволюционного развития бота.
        /// </summary>
        public int RequiredSkillLevel { get; set; } = 0;

        /// <summary>
        /// Количество энергии требуемое для конвертации.
        /// 
        /// Дополнительная стоимость сверх стоимости ресурсов.
        /// 0 = энергия не требуется
        /// </summary>
        public float EnergyCost { get; set; } = 0;

        /// <summary>
        /// Входные ресурсы рецепта (только для чтения).
        /// 
        /// Возвращает копию словаря для предотвращения
        /// модификации извне.
        /// </summary>
        public IReadOnlyDictionary<ResourceType, float> Inputs
        {
            get
            {
                return new Dictionary<ResourceType, float>(_inputs);
            }
        }

        /// <summary>
        /// Выходные ресурсы рецепта (только для чтения).
        /// 
        /// Возвращает копию словаря для предотвращения
        /// модификации извне.
        /// </summary>
        public IReadOnlyDictionary<ResourceType, float> Outputs
        {
            get
            {
                return new Dictionary<ResourceType, float>(_outputs);
            }
        }

        /// <summary>
        /// Общее количество входных ресурсов.
        /// 
        /// Используется для быстрой проверки без итерации.
        /// </summary>
        public float TotalInputAmount
        {
            get
            {
                float total = 0;
                foreach (var amount in _inputs.Values)
                {
                    total += amount;
                }
                return total;
            }
        }

        /// <summary>
        /// Общее количество выходных ресурсов.
        /// 
        /// Используется для расчёта эффективности рецепта.
        /// </summary>
        public float TotalOutputAmount
        {
            get
            {
                float total = 0;
                foreach (var amount in _outputs.Values)
                {
                    total += amount;
                }
                return total;
            }
        }

        /// <summary>
        /// Эффективность рецепта (выход / вход).
        /// 
        /// Значение > 1.0 = рецепт выгодный (увеличивает ресурсы)
        /// Значение < 1.0 = рецепт убыточный (требует больше чем даёт)
        /// Значение = 1.0 = нейтральный рецепт
        /// 
        /// Используется ботами для выбора оптимальных рецептов.
        /// </summary>
        public float Efficiency
        {
            get
            {
                if (TotalInputAmount <= 0)
                    return 0;
                return TotalOutputAmount / TotalInputAmount;
            }
        }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Создаёт новый рецепт конвертации.
        /// </summary>
        /// <param name="id">
        /// Уникальный идентификатор рецепта.
        /// </param>
        /// <param name="conversionTime">
        /// Время выполнения в тиках.
        /// </param>
        public ConversionRecipe(string id, int conversionTime = 0)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID не может быть пустым", nameof(id));

            _id = id;
            _inputs = new Dictionary<ResourceType, float>();
            _outputs = new Dictionary<ResourceType, float>();
            Name = id;
            ConversionTime = conversionTime;
            SuccessChance = 1.0f;
            RequiredSkillLevel = 0;
            EnergyCost = 0;
        }

        #endregion

        #region Методы добавления ресурсов

        /// <summary>
        /// Добавляет входной ресурс к рецепту.
        /// 
        /// Вызывается при создании рецепта для определения
        /// какие ресурсы требуются для конвертации.
        /// </summary>
        /// <param name="type">
        /// Тип добавляемого ресурса.
        /// </param>
        /// <param name="amount">
        /// Количество ресурса (должно быть > 0).
        /// </param>
        /// <returns>
        /// Сам рецепт для цепочки вызовов (fluent interface).
        /// </returns>
        public ConversionRecipe AddInput(ResourceType type, float amount)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (amount <= 0)
                throw new ArgumentException("Количество должно быть положительным", nameof(amount));

            if (_inputs.ContainsKey(type))
            {
                _inputs[type] += amount;
            }
            else
            {
                _inputs[type] = amount;
            }

            return this;
        }

        /// <summary>
        /// Добавляет выходной ресурс к рецепту.
        /// 
        /// Вызывается при создании рецепта для определения
        /// какие ресурсы будут получены после конвертации.
        /// </summary>
        /// <param name="type">
        /// Тип добавляемого ресурса.
        /// </param>
        /// <param name="amount">
        /// Количество ресурса (должно быть > 0).
        /// </param>
        /// <returns>
        /// Сам рецепт для цепочки вызовов (fluent interface).
        /// </returns>
        public ConversionRecipe AddOutput(ResourceType type, float amount)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (amount <= 0)
                throw new ArgumentException("Количество должно быть положительным", nameof(amount));

            if (_outputs.ContainsKey(type))
            {
                _outputs[type] += amount;
            }
            else
            {
                _outputs[type] = amount;
            }

            return this;
        }

        /// <summary>
        /// Добавляет множественные входные ресурсы.
        /// </summary>
        /// <param name="resources">
        /// Словарь тип->количество входных ресурсов.
        /// </param>
        /// <returns>
        /// Сам рецепт для цепочки вызовов.
        /// </returns>
        public ConversionRecipe AddInputs(Dictionary<ResourceType, float> resources)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));

            foreach (var kvp in resources)
            {
                AddInput(kvp.Key, kvp.Value);
            }

            return this;
        }

        /// <summary>
        /// Добавляет множественные выходные ресурсы.
        /// </summary>
        /// <param name="resources">
        /// Словарь тип->количество выходных ресурсов.
        /// </param>
        /// <returns>
        /// Сам рецепт для цепочки вызовов.
        /// </returns>
        public ConversionRecipe AddOutputs(Dictionary<ResourceType, float> resources)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));

            foreach (var kvp in resources)
            {
                AddOutput(kvp.Key, kvp.Value);
            }

            return this;
        }

        #endregion

        #region Проверка и выполнение

        /// <summary>
        /// Проверяет может ли бот выполнить этот рецепт.
        /// 
        /// Выполняет следующие проверки:
        /// 1. Наличие всех входных ресурсов в инвентаре
        /// 2. Достаточно ли энергии у бота
        /// 3. Соответствует ли навык бота требованиям
        /// 4. Есть ли свободное место в инвентаре для выхода
        /// </summary>
        /// <param name="botInventory">
        /// Инвентарь бота который хочет выполнить рецепт.
        /// </param>
        /// <param name="botEnergy">
        /// Текущая энергия бота.
        /// </param>
        /// <param name="botSkillLevel">
        /// Уровень навыка бота.
        /// </param>
        /// <returns>
        /// True если рецепт может быть выполнен, иначе false.
        /// </returns>
        public bool CanExecute(
            ResourcePool botInventory,
            float botEnergy,
            int botSkillLevel = 0)
        {
            if (botInventory == null)
                return false;

            // Проверка входных ресурсов
            foreach (var kvp in _inputs)
            {
                if (!botInventory.HasEnough(kvp.Key, kvp.Value))
                    return false;
            }

            // Проверка энергии
            if (botEnergy < EnergyCost)
                return false;

            // Проверка навыка
            if (botSkillLevel < RequiredSkillLevel)
                return false;

            // Проверка места в инвентаре для выхода
            float outputWeight = TotalOutputAmount;
            float inputWeight = TotalInputAmount;
            float netChange = outputWeight - inputWeight;

            if (netChange > 0)
            {
                float availableSpace = botInventory.MaxCapacity - botInventory.TotalWeight;
                if (availableSpace < netChange && botInventory.MaxCapacity >= 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Выполняет конвертацию ресурсов.
        /// 
        /// Модифицирует инвентарь бота:
        /// - Удаляет входные ресурсы
        /// - Добавляет выходные ресурсы
        /// - Списывает энергию
        /// 
        /// Важно: метод не проверяет CanExecute().
        /// Вызывающий код должен проверить возможность выполнения.
        /// </summary>
        /// <param name="botInventory">
        /// Инвентарь бота для модификации.
        /// </param>
        /// <param name="random">
        /// Генератор случайных чисел для проверки успеха.
        /// </param>
        /// <returns>
        /// True если конвертация успешна, false если неудача.
        /// </returns>
        public bool Execute(ResourcePool botInventory, Random random)
        {
            if (botInventory == null)
                return false;

            // Проверка шанса успеха
            if (random.NextDouble() > SuccessChance)
            {
                // Неудача: теряем часть входных ресурсов
                foreach (var kvp in _inputs)
                {
                    botInventory.Remove(kvp.Key, kvp.Value * 0.5f);
                }
                return false;
            }

            // Удаляем входные ресурсы
            foreach (var kvp in _inputs)
            {
                botInventory.Remove(kvp.Key, kvp.Value);
            }

            // Добавляем выходные ресурсы
            foreach (var kvp in _outputs)
            {
                botInventory.Add(kvp.Key, kvp.Value);
            }

            return true;
        }

        #endregion

        #region Переопределения

        /// <summary>
        /// Сравнивает рецепт с другим объектом.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as ConversionRecipe);
        }

        /// <summary>
        /// Сравнивает рецепт с другим рецептом по ID.
        /// </summary>
        public bool Equals(ConversionRecipe other)
        {
            return other != null && _id == other._id;
        }

        /// <summary>
        /// Возвращает хеш-код рецепта.
        /// </summary>
        public override int GetHashCode()
        {
            return _id?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Возвращает строковое представление рецепта.
        /// </summary>
        public override string ToString()
        {
            var inputStr = string.Join(" + ", _inputs);
            var outputStr = string.Join(" + ", _outputs);
            return $"{Name}: {inputStr} → {outputStr}";
        }

        #endregion

        #region Статические фабрики

        /// <summary>
        /// Создаёт стандартный рецепт создания биомассы.
        /// 
        /// 10 Food + 5 Water → 3 Biomass
        /// </summary>
        public static ConversionRecipe CreateBiomassRecipe()
        {
            return new ConversionRecipe("BiomassCraft", 5)
                .AddInput(ResourceType.Food, 10)
                .AddInput(ResourceType.Water, 5)
                .AddOutput(ResourceType.Biomass, 3);
        }

        /// <summary>
        /// Создаёт рецепт переработки биомассы в энергию.
        /// 
        /// 5 Biomass → 20 Energy (условно)
        /// </summary>
        public static ConversionRecipe CreateEnergyRecipe()
        {
            return new ConversionRecipe("EnergyFromBiomass", 3)
                .AddInput(ResourceType.Biomass, 5)
                .AddOutput(ResourceType.Food, 20); // Food как источник энергии
        }

        /// <summary>
        /// Устанавливает вероятность успеха конвертации.
        /// </summary>
        /// <param name="chance">Вероятность от 0.0 до 1.0.</param>
        /// <returns>Сам рецепт для продолжения цепочки вызовов.</returns>
        public ConversionRecipe WithSuccessChance(float chance)
        {
            SuccessChance = Math.Max(0, Math.Min(1, chance));
            return this;
        }

        /// <summary>
        /// Создаёт рецепт переработки радиоактивного материала.
        /// 
        /// 1 Radioactive → 50 Energy (с риском)
        /// </summary>
        public static ConversionRecipe CreateRadioactiveRecipe()
        {
            return new ConversionRecipe("RadioactiveProcessing", 10)
                .AddInput(ResourceType.Radioactive, 1)
                .AddOutput(ResourceType.Food, 50)
                .WithSuccessChance(0.7f); // 30% риск неудачи
        }

        #endregion
    }
}