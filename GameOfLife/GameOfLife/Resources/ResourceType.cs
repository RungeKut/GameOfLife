using System;
using System.Collections.Generic;

namespace GameOfLife.Resources
{
    /// <summary>
    /// Тип ресурса в экосистеме.
    /// 
    /// Этот класс описывает характеристики различных типов ресурсов,
    /// которые могут существовать в мире: еда, вода, материалы,
    /// энергия, опасные вещества и т.д.
    /// 
    /// Класс реализует шаблон "Перечисляемый тип" (Enum Class),
    /// что позволяет:
    /// - Добавлять сложные свойства к каждому типу
    /// - Обеспечивать типобезопасность при использовании
    /// - Легко расширять набор ресурсов без изменения кода
    /// - Использовать в качестве ключей в словарях и коллекциях
    /// </summary>
    public class ResourceType : IEquatable<ResourceType>
    {
        #region Статические экземпляры (предопределённые типы)

        /// <summary>
        /// Пища: базовый ресурс для восстановления энергии.
        /// 
        /// Характеристики:
        /// - Легко переносится (сдувается ветром)
        /// - Даёт энергию при потреблении
        /// - Не является материалом для строительства
        /// - Не токсичен для ботов
        /// </summary>
        public static readonly ResourceType Food = new ResourceType(
            id: 1,
            name: "Food",
            isLight: true,
            isToxic: false,
            isEnergy: true,
            isMaterial: false,
            energyValue: 10.0f,
            decayRate: 0.01f
        );

        /// <summary>
        /// Вода: ресурс для поддержания жизнедеятельности.
        /// 
        /// Характеристики:
        /// - Легко переносится (сдувается ветром)
        /// - Даёт небольшую энергию
        /// - Необходима для некоторых действий ботов
        /// </summary>
        public static readonly ResourceType Water = new ResourceType(
            id: 2,
            name: "Water",
            isLight: true,
            isToxic: false,
            isEnergy: true,
            isMaterial: false,
            energyValue: 5.0f,
            decayRate: 0.005f
        );

        /// <summary>
        /// Металл: материал для строительства и крафта.
        /// 
        /// Характеристики:
        /// - Тяжёлый (не сдувается ветром)
        /// - Не даёт энергии при потреблении
        /// - Используется для строительства укрытий
        /// - Может конвертироваться в другие ресурсы
        /// </summary>
        public static readonly ResourceType Metal = new ResourceType(
            id: 3,
            name: "Metal",
            isLight: false,
            isToxic: false,
            isEnergy: false,
            isMaterial: true,
            energyValue: 0.0f,
            decayRate: 0.0f
        );

        /// <summary>
        /// Радиоактивный материал: опасный, но энергетически ценный.
        /// 
        /// Характеристики:
        /// - Тяжёлый (не сдувается ветром)
        /// - Даёт много энергии, но наносит урон
        /// - Вызывает мутации генома при контакте
        /// - Может использоваться продвинутыми ботами
        /// </summary>
        public static readonly ResourceType Radioactive = new ResourceType(
            id: 4,
            name: "Radioactive",
            isLight: false,
            isToxic: true,
            isEnergy: true,
            isMaterial: false,
            energyValue: 50.0f,
            decayRate: 0.001f
        );

        /// <summary>
        /// Биомасса: органический материал от погибших ботов.
        /// 
        /// Характеристики:
        /// - Легко переносится
        /// - Даёт умеренную энергию
        /// - Может разлагаться со временем
        /// - Привлекает определённые типы ботов
        /// </summary>
        public static readonly ResourceType Biomass = new ResourceType(
            id: 5,
            name: "Biomass",
            isLight: true,
            isToxic: false,
            isEnergy: true,
            isMaterial: false,
            energyValue: 8.0f,
            decayRate: 0.02f
        );

        /// <summary>
        /// Список всех предопределённых типов ресурсов.
        /// 
        /// Используется для итерации по всем типам,
        /// инициализации словарей и валидации входных данных.
        /// </summary>
        public static readonly IReadOnlyList<ResourceType> All = new[]
        {
            Food, Water, Metal, Radioactive, Biomass
        };

        #endregion

        #region Приватные поля

        /// <summary>
        /// Уникальный числовой идентификатор типа.
        /// Используется для быстрой сравнения и хеширования.
        /// </summary>
        private readonly int _id;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Уникальный идентификатор типа ресурса.
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Человеко-читаемое название типа.
        /// Используется для отображения в UI и логирования.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Индикатор "лёгкости" ресурса.
        /// 
        /// true: ресурс может быть перемещён ветром
        /// false: ресурс остаётся на месте независимо от погоды
        /// 
        /// Используется системой WeatherSystem для расчёта
        /// распространения ресурсов по карте.
        /// </summary>
        public bool IsLight { get; }

        /// <summary>
        /// Индикатор токсичности ресурса.
        /// 
        /// true: контакт с ресурсом наносит урон ботам
        /// false: ресурс безопасен при контакте
        /// 
        /// Токсичные ресурсы могут:
        /// - Снижать здоровье бота при сборе
        /// - Вызывать мутации генома
        /// - Требовать специальной защиты для использования
        /// </summary>
        public bool IsToxic { get; }

        /// <summary>
        /// Индикатор энергетической ценности.
        /// 
        /// true: ресурс может быть потреблён для получения энергии
        /// false: ресурс не даёт энергии при потреблении
        /// 
        /// Энергетические ресурсы используются ботами для:
        /// - Восполнения запаса энергии
        /// - Выполнения действий (движение, атака, строительство)
        /// - Поддержания базового метаболизма
        /// </summary>
        public bool IsEnergy { get; }

        /// <summary>
        /// Индикатор строительной пригодности.
        /// 
        /// true: ресурс может использоваться для строительства
        /// false: ресурс не подходит для создания структур
        /// 
        /// Строительные материалы требуются для:
        /// - Создания укрытий от аномалий
        /// - Строительства оборонительных сооружений
        /// - Крафта инструментов и улучшений
        /// </summary>
        public bool IsMaterial { get; }

        /// <summary>
        /// Количество энергии, получаемое при потреблении единицы ресурса.
        /// 
        /// Значение 0 означает, что ресурс не даёт энергии.
        /// Отрицательные значения возможны для токсичных ресурсов
        /// (потребление наносит урон вместо лечения).
        /// 
        /// Единица измерения: условные единицы энергии.
        /// </summary>
        public float EnergyValue { get; }

        /// <summary>
        /// Скорость естественного распада ресурса за тик.
        /// 
        /// Значение в диапазоне [0, 1]:
        /// - 0: ресурс не распадается (постоянный)
        /// - 0.01: 1% ресурса исчезает каждый тик
        /// - 1.0: ресурс исчезает мгновенно
        /// 
        /// Используется ResourceManager для моделирования
        /// естественного разложения органических материалов.
        /// </summary>
        public float DecayRate { get; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый тип ресурса с заданными характеристиками.
        /// 
        /// Конструктор приватный для внешних сборок, чтобы
        /// предотвратить создание дублирующих типов.
        /// Предопределённые типы создаются в статическом конструкторе.
        /// </summary>
        /// <param name="id">Уникальный числовой идентификатор.</param>
        /// <param name="name">Человеко-читаемое название.</param>
        /// <param name="isLight">Может ли ресурс переноситься ветром.</param>
        /// <param name="isToxic">Является ли ресурс токсичным.</param>
        /// <param name="isEnergy">Даёт ли ресурс энергию при потреблении.</param>
        /// <param name="isMaterial">Может ли ресурс использоваться для строительства.</param>
        /// <param name="energyValue">Количество энергии от единицы ресурса.</param>
        /// <param name="decayRate">Скорость естественного распада за тик.</param>
        private ResourceType(
            int id,
            string name,
            bool isLight,
            bool isToxic,
            bool isEnergy,
            bool isMaterial,
            float energyValue,
            float decayRate)
        {
            _id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsLight = isLight;
            IsToxic = isToxic;
            IsEnergy = isEnergy;
            IsMaterial = isMaterial;
            EnergyValue = energyValue;
            DecayRate = Math.Max(0, Math.Min(1, decayRate));
        }

        #endregion

        #region Переопределения методов

        /// <summary>
        /// Сравнивает текущий тип ресурса с другим объектом.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as ResourceType);
        }

        /// <summary>
        /// Сравнивает текущий тип ресурса с другим типом.
        /// Сравнение выполняется по уникальному идентификатору.
        /// </summary>
        public bool Equals(ResourceType other)
        {
            return other != null && _id == other._id;
        }

        /// <summary>
        /// Возвращает хеш-код типа ресурса.
        /// Используется при хранении в словарях и HashSet.
        /// </summary>
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        /// <summary>
        /// Возвращает строковое представление типа ресурса.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Операторы

        /// <summary>
        /// Оператор равенства для типов ресурсов.
        /// </summary>
        public static bool operator ==(ResourceType left, ResourceType right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        /// <summary>
        /// Оператор неравенства для типов ресурсов.
        /// </summary>
        public static bool operator !=(ResourceType left, ResourceType right)
        {
            return !(left == right);
        }

        #endregion
    }
}