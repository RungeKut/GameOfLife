using GameOfLife.Core;
using GameOfLife.Resources;
using System;

namespace GameOfLife.Entities
{
    /// <summary>
    /// Хранилище — структура для хранения дополнительных ресурсов.
    /// 
    /// Особенности хранилища:
    /// - Предоставляет дополнительный инвентарь для владельца
    /// - Может хранить ресурсы нескольких типов
    /// - Имеет лимит вместимости зависящий от уровня
    /// - Может быть ограблено другими ботами
    /// - Требует защиты от разрушения
    /// 
    /// Стратегия использования:
    /// - Строить рядом с базой для безопасного хранения
    /// - Увеличивать уровень для большей вместимости
    /// - Защищать стенами и ловушками
    /// - Использовать для накопления редких ресурсов
    /// 
    /// Наследует Structure с дополнительной логикой хранения.
    /// </summary>
    public class Storage : Structure
    {
        #region Приватные поля

        /// <summary>
        /// Хранилище ресурсов структуры.
        /// </summary>
        private readonly ResourcePool _storage;

        /// <summary>
        /// Максимальная вместимость хранилища.
        /// </summary>
        private float _maxCapacity;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Хранилище ресурсов (только для чтения).
        /// </summary>
        public ResourcePool Storage => _storage;

        /// <summary>
        /// Текущая заполненность хранилища.
        /// </summary>
        public float CurrentCapacity => _storage.TotalWeight;

        /// <summary>
        /// Максимальная вместимость хранилища.
        /// </summary>
        public float MaxCapacity => _maxCapacity;

        /// <summary>
        /// Процент заполненности хранилища.
        /// </summary>
        public float FillPercentage
        {
            get
            {
                if (_maxCapacity <= 0)
                    return 0;
                return CurrentCapacity / _maxCapacity * 100;
            }
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новое хранилище.
        /// </summary>
        public Storage(int id, int x, int y, Bot owner = null)
            : base(id, x, y, StructureType.Storage, owner)
        {
            _storage = new ResourcePool(-1);
            InitializeCapacity();
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует вместимость хранилища.
        /// </summary>
        private void InitializeCapacity()
        {
            // Вместимость зависит от уровня структуры
            _maxCapacity = 100 * Level;
            _storage.MaxCapacity = _maxCapacity;
        }

        #endregion

        #region Переопределённые методы

        /// <summary>
        /// Обновляет хранилище каждый тик.
        /// </summary>
        public override void Tick(SimulationEngine engine)
        {
            base.Tick(engine);

            if (IsActive)
            {
                // Обновляем вместимость при улучшении
                _maxCapacity = 100 * Level;
                _storage.MaxCapacity = _maxCapacity;
            }
        }

        /// <summary>
        /// Улучшает хранилище.
        /// </summary>
        public override bool Upgrade(ResourcePool upgradeCost)
        {
            bool success = base.Upgrade(upgradeCost);

            if (success)
            {
                InitializeCapacity();
            }

            return success;
        }

        #endregion

        #region Операции с ресурсами

        /// <summary>
        /// Добавляет ресурсы в хранилище.
        /// </summary>
        public bool Deposit(ResourceType type, float amount)
        {
            if (!IsActive)
                return false;

            return _storage.Add(type, amount);
        }

        /// <summary>
        /// Извлекает ресурсы из хранилища.
        /// </summary>
        public bool Withdraw(ResourceType type, float amount)
        {
            if (!IsActive)
                return false;

            return _storage.Remove(type, amount);
        }

        /// <summary>
        /// Проверяет наличие ресурса в хранилище.
        /// </summary>
        public bool HasResource(ResourceType type, float amount)
        {
            return _storage.HasEnough(type, amount);
        }

        /// <summary>
        /// Возвращает количество ресурса в хранилище.
        /// </summary>
        public float GetResourceAmount(ResourceType type)
        {
            return _storage.GetAmount(type);
        }

        /// <summary>
        /// Очищает хранилище (при разрушении).
        /// </summary>
        public ResourcePool Clear()
        {
            var contents = _storage.Clone();
            _storage.Clear();
            return contents;
        }

        #endregion

        #region Защита хранилища

        /// <summary>
        /// Проверяет имеет ли бот доступ к хранилищу.
        /// </summary>
        public bool HasAccess(Bot bot)
        {
            if (bot == null)
                return false;

            // Владелец всегда имеет доступ
            if (IsOwnedBy(bot))
                return true;

            // В полной реализации: проверка альянсов, разрешений
            return false;
        }

        /// <summary>
        /// Пытается ограбить хранилище.
        /// </summary>
        public ResourcePool TryRaid(Bot raider)
        {
            if (!IsActive || HasAccess(raider))
                return null;

            // В полной реализации: логика ограбления
            // - Шанс успеха зависит от уровня хранилища
            // - Часть ресурсов может быть украдена
            // - Владелец получает уведомление

            return null;
        }

        #endregion

        #region Переопределение Destroy

        /// <summary>
        /// Уничтожает хранилище с сохранением ресурсов.
        /// </summary>
        protected override void Destroy()
        {
            // Оставляем ресурсы на месте разрушения
            var contents = Clear();

            // В полной реализации: создание ResourceNode на месте
            base.Destroy();
        }

        #endregion
    }
}