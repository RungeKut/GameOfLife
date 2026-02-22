using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Resources
{
    /// <summary>
    /// База данных рецептов конвертации.
    /// 
    /// Хранит все доступные в мире рецепты и предоставляет
    /// методы для поиска, фильтрации и управления рецептами.
    /// 
    /// Архитектурные особенности:
    /// - Глобальный синглтон для доступа из любой части кода
    /// - Поддержка динамического добавления рецептов
    /// - Категоризация рецептов по типам
    /// - Кэширование для быстрого поиска
    /// 
    /// Использование:
    /// var db = RecipeDatabase.Instance;
    /// var recipe = db.GetRecipe("BiomassCraft");
    /// var recipes = db.GetRecipesByCategory(ResourceCategory.Crafting);
    /// </summary>
    public class RecipeDatabase
    {
        #region Приватные поля

        /// <summary>
        /// Единственный экземпляр базы данных (синглтон).
        /// </summary>
        private static RecipeDatabase _instance;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Все рецепты по идентификатору.
        /// </summary>
        private readonly Dictionary<string, ConversionRecipe> _recipes;

        /// <summary>
        /// Рецепты сгруппированные по категории.
        /// </summary>
        private readonly Dictionary<string, List<ConversionRecipe>> _recipesByCategory;

        /// <summary>
        /// Генератор случайных чисел для случайных рецептов.
        /// </summary>
        private readonly Random _random;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Единственный экземпляр базы данных.
        /// 
        /// Ленивая инициализация при первом обращении.
        /// Потокобезопасна благодаря блокировке.
        /// </summary>
        public static RecipeDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(RecipeDatabase))
                    {
                        if (_instance == null)
                        {
                            _instance = new RecipeDatabase();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Общее количество рецептов в базе.
        /// </summary>
        public int RecipeCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _recipes.Count;
                }
            }
        }

        /// <summary>
        /// Все рецепты в базе (только для чтения).
        /// 
        /// Возвращает копию коллекции для предотвращения
        /// модификации извне.
        /// </summary>
        public IReadOnlyCollection<ConversionRecipe> AllRecipes
        {
            get
            {
                lock (_lockObject)
                {
                    return _recipes.Values.ToList().AsReadOnly();
                }
            }
        }

        #endregion

        #region События

        /// <summary>
        /// Событие добавления нового рецепта.
        /// 
        /// Вызывается когда новый рецепт добавляется в базу.
        /// Подписчики могут использовать для:
        /// - Обновления UI списка рецептов
        /// - Логирования изменений
        /// - Триггеров для других систем
        /// </summary>
        public event Action<ConversionRecipe> OnRecipeAdded;

        /// <summary>
        /// Событие удаления рецепта.
        /// 
        /// Вызывается когда рецепт удаляется из базы.
        /// </summary>
        public event Action<ConversionRecipe> OnRecipeRemoved;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый экземпляр базы данных рецептов.
        /// 
        /// Приватный конструктор для паттерна синглтон.
        /// Инициализирует базовые рецепты при создании.
        /// </summary>
        private RecipeDatabase()
        {
            _recipes = new Dictionary<string, ConversionRecipe>();
            _recipesByCategory = new Dictionary<string, List<ConversionRecipe>>();
            _random = new Random();

            // Инициализируем базовые рецепты
            InitializeDefaultRecipes();
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует базовые рецепты по умолчанию.
        /// 
        /// Вызывается при создании базы данных.
        /// Добавляет стандартные рецепты которые доступны
        /// всем ботам с начала симуляции.
        /// </summary>
        private void InitializeDefaultRecipes()
        {
            // Рецепт создания биомассы
            AddRecipe(ConversionRecipe.CreateBiomassRecipe(), "Basic");

            // Рецепт получения энергии из биомассы
            AddRecipe(ConversionRecipe.CreateEnergyRecipe(), "Basic");

            // Рецепт переработки радиоактивного материала
            AddRecipe(ConversionRecipe.CreateRadioactiveRecipe(), "Advanced");
        }

        #endregion

        #region Управление рецептами

        /// <summary>
        /// Добавляет рецепт в базу данных.
        /// 
        /// Если рецепт с таким ID уже существует, он будет
        /// заменён новым. Вызывает событие OnRecipeAdded.
        /// </summary>
        /// <param name="recipe">
        /// Рецепт для добавления.
        /// </param>
        /// <param name="category">
        /// Категория рецепта для группировки.
        /// </param>
        public void AddRecipe(ConversionRecipe recipe, string category = "Uncategorized")
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            lock (_lockObject)
            {
                // Добавляем в основной словарь
                _recipes[recipe.Id] = recipe;

                // Добавляем в категорию
                if (!_recipesByCategory.ContainsKey(category))
                {
                    _recipesByCategory[category] = new List<ConversionRecipe>();
                }

                // Удаляем старую версию если существует
                _recipesByCategory[category].RemoveAll(r => r.Id == recipe.Id);
                _recipesByCategory[category].Add(recipe);

                OnRecipeAdded?.Invoke(recipe);
            }
        }

        /// <summary>
        /// Удаляет рецепт из базы данных по ID.
        /// </summary>
        /// <param name="recipeId">
        /// Идентификатор рецепта для удаления.
        /// </param>
        /// <returns>
        /// True если рецепт найден и удалён, иначе false.
        /// </returns>
        public bool RemoveRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
                return false;

            lock (_lockObject)
            {
                if (_recipes.TryGetValue(recipeId, out var recipe))
                {
                    _recipes.Remove(recipeId);

                    // Удаляем из всех категорий
                    foreach (var category in _recipesByCategory.Values)
                    {
                        category.RemoveAll(r => r.Id == recipeId);
                    }

                    OnRecipeRemoved?.Invoke(recipe);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Получает рецепт по идентификатору.
        /// </summary>
        /// <param name="recipeId">
        /// Идентификатор искомого рецепта.
        /// </param>
        /// <returns>
        /// Рецепт если найден, иначе null.
        /// </returns>
        public ConversionRecipe GetRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
                return null;

            lock (_lockObject)
            {
                _recipes.TryGetValue(recipeId, out var recipe);
                return recipe;
            }
        }

        /// <summary>
        /// Получает все рецепты из указанной категории.
        /// </summary>
        /// <param name="category">
        /// Название категории для поиска.
        /// </param>
        /// <returns>
        /// Список рецептов в категории (может быть пустым).
        /// </returns>
        public List<ConversionRecipe> GetRecipesByCategory(string category)
        {
            lock (_lockObject)
            {
                if (_recipesByCategory.TryGetValue(category, out var recipes))
                {
                    return recipes.ToList();
                }
                return new List<ConversionRecipe>();
            }
        }

        /// <summary>
        /// Получает все доступные категории рецептов.
        /// </summary>
        /// <returns>
        /// Список названий категорий.
        /// </returns>
        public List<string> GetAllCategories()
        {
            lock (_lockObject)
            {
                return _recipesByCategory.Keys.ToList();
            }
        }

        #endregion

        #region Поиск рецептов

        /// <summary>
        /// Находит рецепты которые бот может выполнить.
        /// 
        /// Фильтрует все рецепты по критериям:
        /// - Наличие ресурсов в инвентаре
        /// - Достаточно энергии
        /// - Соответствие уровня навыка
        /// </summary>
        /// <param name="botInventory">
        /// Инвентарь бота для проверки.
        /// </param>
        /// <param name="botEnergy">
        /// Текущая энергия бота.
        /// </param>
        /// <param name="botSkillLevel">
        /// Уровень навыка бота.
        /// </param>
        /// <returns>
        /// Список рецептов которые могут быть выполнены.
        /// </returns>
        public List<ConversionRecipe> GetExecutableRecipes(
            ResourcePool botInventory,
            float botEnergy,
            int botSkillLevel = 0)
        {
            var result = new List<ConversionRecipe>();

            lock (_lockObject)
            {
                foreach (var recipe in _recipes.Values)
                {
                    if (recipe.CanExecute(botInventory, botEnergy, botSkillLevel))
                    {
                        result.Add(recipe);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Находит лучший рецепт для выполнения.
        /// 
        /// Критерии выбора:
        /// 1. Рецепт может быть выполнен
        /// 2. Максимальная эффективность
        /// 3. Минимальное время выполнения
        /// </summary>
        /// <param name="botInventory">
        /// Инвентарь бота для проверки.
        /// </param>
        /// <param name="botEnergy">
        /// Текущая энергия бота.
        /// </param>
        /// <param name="botSkillLevel">
        /// Уровень навыка бота.
        /// </param>
        /// <returns>
        /// Лучший рецепт или null если нет доступных.
        /// </returns>
        public ConversionRecipe GetBestRecipe(
            ResourcePool botInventory,
            float botEnergy,
            int botSkillLevel = 0)
        {
            var executable = GetExecutableRecipes(botInventory, botEnergy, botSkillLevel);

            if (executable.Count == 0)
                return null;

            // Сортируем по эффективности (убывание) и времени (возрастание)
            return executable
                .OrderByDescending(r => r.Efficiency)
                .ThenBy(r => r.ConversionTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// Находит случайный рецепт из категории.
        /// 
        /// Используется для стохастического поведения ботов
        /// когда нет явного предпочтения рецепта.
        /// </summary>
        /// <param name="category">
        /// Категория для выбора рецепта.
        /// </param>
        /// <returns>
        /// Случайный рецепт или null если категория пуста.
        /// </returns>
        public ConversionRecipe GetRandomRecipe(string category = null)
        {
            lock (_lockObject)
            {
                List<ConversionRecipe> recipes;

                if (string.IsNullOrEmpty(category))
                {
                    recipes = _recipes.Values.ToList();
                }
                else if (_recipesByCategory.TryGetValue(category, out var catRecipes))
                {
                    recipes = catRecipes.ToList();
                }
                else
                {
                    return null;
                }

                if (recipes.Count == 0)
                    return null;

                int index = _random.Next(recipes.Count);
                return recipes[index];
            }
        }

        #endregion

        #region Сериализация

        /// <summary>
        /// Экспортирует все рецепты в словарь для сохранения.
        /// 
        /// Используется системой сохранения для сериализации
        /// изученных рецептов бота.
        /// </summary>
        /// <returns>
        /// Словарь ID->Recipe для сериализации.
        /// </returns>
        public Dictionary<string, object> ExportForSave()
        {
            var result = new Dictionary<string, object>();

            lock (_lockObject)
            {
                foreach (var kvp in _recipes)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Импортирует рецепты из сохранённых данных.
        /// 
        /// Используется системой загрузки для восстановления
        /// рецептов после загрузки сохранения.
        /// </summary>
        /// <param name="savedData">
        /// Словарь с сохранёнными рецептами.
        /// </param>
        public void ImportFromSave(Dictionary<string, object> savedData)
        {
            if (savedData == null)
                return;

            lock (_lockObject)
            {
                foreach (var kvp in savedData)
                {
                    if (kvp.Value is ConversionRecipe recipe)
                    {
                        _recipes[kvp.Key] = recipe;
                    }
                }
            }
        }

        #endregion

        #region Утилиты

        /// <summary>
        /// Очищает все рецепты из базы.
        /// 
        /// Используется для сброса состояния или при завершении
        /// симуляции. Вызывает OnRecipeRemoved для каждого рецепта.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                var allRecipes = _recipes.Values.ToList();
                _recipes.Clear();
                _recipesByCategory.Clear();

                foreach (var recipe in allRecipes)
                {
                    OnRecipeRemoved?.Invoke(recipe);
                }
            }
        }

        /// <summary>
        /// Проверяет существует ли рецепт с указанным ID.
        /// </summary>
        public bool HasRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
                return false;

            lock (_lockObject)
            {
                return _recipes.ContainsKey(recipeId);
            }
        }

        #endregion
    }
}