using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Resources
{
    /// <summary>
    /// Хранилище ресурсов сущности (бота или структуры).
    /// 
    /// Этот класс управляет коллекцией ресурсов различных типов,
    /// предоставляя методы для добавления, удаления, проверки
    /// наличия и конвертации ресурсов.
    /// 
    /// Особенности реализации:
    /// - Потокобезопасность через блокировки
    /// - Эффективный поиск по типу ресурса (Dictionary)
    /// - Поддержка ограничения максимального веса
    /// - События для уведомления об изменениях
    /// </summary>
    public class ResourcePool
    {
        #region Приватные поля

        /// <summary>
        /// Внутреннее хранилище ресурсов: тип -> количество.
        /// 
        /// Используется Dictionary для быстрого доступа O(1)
        /// к ресурсам по их типу.
        /// </summary>
        private readonly Dictionary<ResourceType, float> _resources;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Максимальный общий вес ресурсов в пуле.
        /// -1 означает неограниченную вместимость.
        /// </summary>
        private float _maxCapacity;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Максимальная вместимость пула в единицах веса.
        /// 
        /// Если значение -1, вместимость не ограничена.
        /// При попытке добавить ресурсы сверх лимита
        /// операция завершится с false или частично.
        /// </summary>
        public float MaxCapacity
        {
            get => _maxCapacity;
            set
            {
                lock (_lockObject)
                {
                    _maxCapacity = value;
                }
            }
        }

        /// <summary>
        /// Текущий общий вес всех ресурсов в пуле.
        /// 
        /// Вычисляется как сумма количеств всех типов ресурсов.
        /// Используется для проверки лимита вместимости.
        /// </summary>
        public float TotalWeight
        {
            get
            {
                lock (_lockObject)
                {
                    return _resources.Values.Sum();
                }
            }
        }

        /// <summary>
        /// Индексатор для доступа к количеству ресурса по типу.
        /// 
        /// Возвращает 0 если ресурс отсутствует.
        /// Установка значения через индексатор не поддерживается —
        /// используйте методы Add и Remove для явного контроля.
        /// </summary>
        /// <param name="type">Тип запрашиваемого ресурса.</param>
        /// <returns>Количество ресурса данного типа (0 если нет).</returns>
        public float this[ResourceType type]
        {
            get
            {
                lock (_lockObject)
                {
                    return _resources.TryGetValue(type, out var amount) ? amount : 0;
                }
            }
        }

        /// <summary>
        /// Список типов ресурсов, присутствующих в пуле.
        /// 
        /// Возвращает копию списка для предотвращения
        /// модификации коллекции извне во время итерации.
        /// </summary>
        public IReadOnlyList<ResourceType> ResourceTypes
        {
            get
            {
                lock (_lockObject)
                {
                    return _resources.Keys.ToList().AsReadOnly();
                }
            }
        }

        #endregion

        #region События

        /// <summary>
        /// Событие изменения количества ресурса.
        /// 
        /// Вызывается после успешного добавления или удаления.
        /// Аргументы: тип ресурса, новое количество, изменение (+/-).
        /// </summary>
        public event Action<ResourceType, float, float> OnResourceChanged;

        /// <summary>
        /// Событие достижения лимита вместимости.
        /// 
        /// Вызывается когда попытка добавить ресурсы
        /// не может быть выполнена из-за превышения MaxCapacity.
        /// </summary>
        public event Action<ResourceType, float> OnCapacityReached;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый пустой пул ресурсов.
        /// </summary>
        /// <param name="maxCapacity">
        /// Максимальная вместимость. -1 для неограниченной.
        /// </param>
        public ResourcePool(float maxCapacity = -1)
        {
            _resources = new Dictionary<ResourceType, float>();
            _maxCapacity = maxCapacity;
        }

        #endregion

        #region Методы добавления ресурсов

        /// <summary>
        /// Добавляет указанное количество ресурса в пул.
        /// 
        /// Если добавление превысит лимит вместимости:
        /// - Добавляется только возможное количество
        /// - Возвращается false
        /// - Вызывается событие OnCapacityReached
        /// 
        /// Метод потокобезопасен.
        /// </summary>
        /// <param name="type">Тип добавляемого ресурса.</param>
        /// <param name="amount">Количество для добавления (должно быть >= 0).</param>
        /// <returns>True если всё количество добавлено, иначе false.</returns>
        public bool Add(ResourceType type, float amount)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (amount < 0)
                throw new ArgumentException("Количество не может быть отрицательным", nameof(amount));

            lock (_lockObject)
            {
                // Проверяем лимит вместимости
                if (_maxCapacity >= 0)
                {
                    float newTotal = TotalWeight + amount;
                    if (newTotal > _maxCapacity)
                    {
                        // Добавляем только возможное количество
                        float canAdd = _maxCapacity - TotalWeight;
                        if (canAdd > 0)
                        {
                            AddInternal(type, canAdd);
                            OnCapacityReached?.Invoke(type, amount - canAdd);
                        }
                        else
                        {
                            OnCapacityReached?.Invoke(type, amount);
                        }
                        return false;
                    }
                }

                return AddInternal(type, amount);
            }
        }

        /// <summary>
        /// Внутренний метод добавления без проверки лимитов.
        /// Должен вызываться только внутри блокировки.
        /// </summary>
        private bool AddInternal(ResourceType type, float amount)
        {
            if (_resources.TryGetValue(type, out var current))
            {
                _resources[type] = current + amount;
            }
            else
            {
                _resources[type] = amount;
            }

            OnResourceChanged?.Invoke(type, _resources[type], amount);
            return true;
        }

        /// <summary>
        /// Добавляет несколько типов ресурсов за одну операцию.
        /// 
        /// Если какой-либо ресурс не может быть добавлен полностью,
        /// вся операция откатывается (atomicity).
        /// </summary>
        /// <param name="resources">
        /// Словарь тип->количество для добавления.
        /// </param>
        /// <returns>True если все ресурсы добавлены, иначе false.</returns>
        public bool AddMany(Dictionary<ResourceType, float> resources)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));

            lock (_lockObject)
            {
                // Сначала проверяем, поместится ли всё
                float additionalWeight = resources.Values.Sum();
                if (_maxCapacity >= 0 && TotalWeight + additionalWeight > _maxCapacity)
                {
                    return false;
                }

                // Добавляем все ресурсы
                foreach (var kvp in resources)
                {
                    AddInternal(kvp.Key, kvp.Value);
                }

                return true;
            }
        }

        #endregion

        #region Методы удаления ресурсов

        /// <summary>
        /// Удаляет указанное количество ресурса из пула.
        /// 
        /// Если запрошено больше, чем есть:
        /// - Удаляется всё доступное количество
        /// - Возвращается false
        /// 
        /// Метод потокобезопасен.
        /// </summary>
        /// <param name="type">Тип удаляемого ресурса.</param>
        /// <param name="amount">Количество для удаления (должно быть >= 0).</param>
        /// <returns>True если всё количество удалено, иначе false.</returns>
        public bool Remove(ResourceType type, float amount)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (amount < 0)
                throw new ArgumentException("Количество не может быть отрицательным", nameof(amount));

            lock (_lockObject)
            {
                if (!_resources.TryGetValue(type, out var current))
                    return false;

                float toRemove = Math.Min(amount, current);
                float newAmount = current - toRemove;

                if (newAmount <= 0)
                {
                    _resources.Remove(type);
                }
                else
                {
                    _resources[type] = newAmount;
                }

                OnResourceChanged?.Invoke(type, newAmount, -toRemove);
                return toRemove >= amount;
            }
        }

        /// <summary>
        /// Полностью очищает пул от всех ресурсов.
        /// 
        /// Вызывает событие OnResourceChanged для каждого
        /// удалённого типа ресурса с новым количеством 0.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                foreach (var kvp in _resources.ToList())
                {
                    _resources.Remove(kvp.Key);
                    OnResourceChanged?.Invoke(kvp.Key, 0, -kvp.Value);
                }
            }
        }

        #endregion

        #region Методы проверки

        /// <summary>
        /// Проверяет наличие достаточного количества ресурса.
        /// </summary>
        /// <param name="type">Тип проверяемого ресурса.</param>
        /// <param name="amount">Требуемое количество.</param>
        /// <returns>True если ресурса достаточно, иначе false.</returns>
        public bool HasEnough(ResourceType type, float amount)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            lock (_lockObject)
            {
                return _resources.TryGetValue(type, out var current) && current >= amount;
            }
        }

        /// <summary>
        /// Проверяет наличие всех указанных ресурсов в нужном количестве.
        /// </summary>
        /// <param name="requirements">
        /// Словарь тип->требуемое количество.
        /// </param>
        /// <returns>True если все требования удовлетворены.</returns>
        public bool HasEnoughMany(Dictionary<ResourceType, float> requirements)
        {
            if (requirements == null)
                throw new ArgumentNullException(nameof(requirements));

            lock (_lockObject)
            {
                foreach (var kvp in requirements)
                {
                    if (!_resources.TryGetValue(kvp.Key, out var current) ||
                        current < kvp.Value)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Возвращает доступное количество ресурса.
        /// </summary>
        /// <param name="type">Тип запрашиваемого ресурса.</param>
        /// <returns>Количество ресурса (0 если отсутствует).</returns>
        public float GetAmount(ResourceType type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            lock (_lockObject)
            {
                return _resources.TryGetValue(type, out var amount) ? amount : 0;
            }
        }

        #endregion

        #region Конвертация ресурсов

        /// <summary>
        /// Конвертирует один тип ресурса в другой по заданному коэффициенту.
        /// 
        /// Пример: конвертация 10 Food в 5 Biomass (коэффициент 0.5)
        /// 
        /// Операция атомарна: либо выполняется полностью, либо не выполняется.
        /// </summary>
        /// <param name="fromType">Тип исходного ресурса.</param>
        /// <param name="toType">Тип целевого ресурса.</param>
        /// <param name="fromAmount">Количество исходного ресурса.</param>
        /// <param name="conversionRate">Коэффициент конвертации (to/from).</param>
        /// <returns>True если конвертация успешна, иначе false.</returns>
        public bool Convert(
            ResourceType fromType,
            ResourceType toType,
            float fromAmount,
            float conversionRate)
        {
            if (fromType == null)
                throw new ArgumentNullException(nameof(fromType));
            if (toType == null)
                throw new ArgumentNullException(nameof(toType));
            if (fromAmount <= 0)
                throw new ArgumentException("Количество должно быть положительным", nameof(fromAmount));
            if (conversionRate <= 0)
                throw new ArgumentException("Коэффициент должен быть положительным", nameof(conversionRate));

            lock (_lockObject)
            {
                // Проверяем наличие исходного ресурса
                if (!HasEnough(fromType, fromAmount))
                    return false;

                // Проверяем вместимость для нового ресурса
                float toAmount = fromAmount * conversionRate;
                if (_maxCapacity >= 0)
                {
                    float newTotal = TotalWeight - fromAmount + toAmount;
                    if (newTotal > _maxCapacity)
                        return false;
                }

                // Выполняем конвертацию
                Remove(fromType, fromAmount);
                AddInternal(toType, toAmount);

                return true;
            }
        }

        #endregion

        #region Сериализация и клонирование

        /// <summary>
        /// Создаёт копию пула ресурсов.
        /// 
        /// Возвращает новый объект с теми же данными,
        /// но независимый от оригинала.
        /// </summary>
        /// <returns>Новый экземпляр ResourcePool с копией данных.</returns>
        public ResourcePool Clone()
        {
            lock (_lockObject)
            {
                var clone = new ResourcePool(_maxCapacity);
                foreach (var kvp in _resources)
                {
                    clone._resources[kvp.Key] = kvp.Value;
                }
                return clone;
            }
        }

        /// <summary>
        /// Возвращает словарь с копией всех ресурсов.
        /// 
        /// Используется для безопасного чтения состояния
        /// извне без риска модификации исходной коллекции.
        /// </summary>
        /// <returns>Копия словаря тип->количество.</returns>
        public Dictionary<ResourceType, float> ToDictionary()
        {
            lock (_lockObject)
            {
                return new Dictionary<ResourceType, float>(_resources);
            }
        }

        #endregion
    }
}