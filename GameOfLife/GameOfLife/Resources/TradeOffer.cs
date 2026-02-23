using System;

namespace GameOfLife.Resources
{
    /// <summary>
    /// Предложение торговли между ботами.
    /// 
    /// Инкапсулирует условия торговой сделки:
    /// - Какие ресурсы предлагает продавец
    /// - Какие ресурсы требует взамен
    /// - Срок действия предложения
    /// - Минимальное количество для сделки
    /// 
    /// Архитектурные особенности:
    /// - Иммутабельность после создания (безопасность)
    /// - Поддержка частичного выполнения сделки
    /// - Валидация условий перед принятием
    /// - События для уведомления об изменениях
    /// 
    /// Жизненный цикл предложения:
    /// 1. Создание: бот создаёт предложение
    /// 2. Публикация: предложение становится доступным
    /// 3. Принятие: другой бот принимает предложение
    /// 4. Выполнение: ресурсы обмениваются
    /// 5. Завершение: предложение удаляется
    /// 
    /// Использование:
    /// var offer = new TradeOffer(sellerBot, ResourceType.Food, 10, ResourceType.Water, 5);
    /// if (buyerBot.CanAccept(offer)) { offer.Execute(); }
    /// </summary>
    public class TradeOffer : IEquatable<TradeOffer>
    {
        #region Приватные поля

        /// <summary>
        /// Уникальный идентификатор предложения.
        /// </summary>
        private readonly Guid _id;

        /// <summary>
        /// Бот который создал предложение (продавец).
        /// </summary>
        private readonly Entities.Bot _seller;

        /// <summary>
        /// Время создания предложения.
        /// </summary>
        private readonly DateTime _createdTime;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Уникальный идентификатор предложения.
        /// 
        /// Используется для:
        /// - Отслеживания предложения в системе торговли
        /// - Отмены конкретного предложения
        /// - Логирования торговых операций
        /// </summary>
        public Guid Id => _id;

        /// <summary>
        /// Бот который создал предложение.
        /// 
        /// Только для чтения. Не может быть изменено после создания.
        /// </summary>
        public Entities.Bot Seller => _seller;

        /// <summary>
        /// Тип ресурса который предлагается к продаже.
        /// </summary>
        public ResourceType OfferedResource { get; }

        /// <summary>
        /// Количество предлагаемого ресурса.
        /// </summary>
        public float OfferedAmount { get; private set; }

        /// <summary>
        /// Тип ресурса который требуется взамен.
        /// </summary>
        public ResourceType RequiredResource { get; }

        /// <summary>
        /// Количество требуемого ресурса.
        /// </summary>
        public float RequiredAmount { get; private set; }

        /// <summary>
        /// Время создания предложения.
        /// 
        /// Используется для расчёта срока действия
        /// и сортировки предложений по времени.
        /// </summary>
        public DateTime CreatedTime => _createdTime;

        /// <summary>
        /// Срок действия предложения в тиках.
        /// 
        /// 0 = бессрочное предложение
        /// >0 = предложение истекает через N тиков
        /// 
        /// По умолчанию 100 тиков (чтобы боты не держали
        /// неактуальные предложения бесконечно).
        /// </summary>
        public int ExpirationTicks { get; set; } = 100;

        /// <summary>
        /// Количество тиков прошедших с создания.
        /// 
        /// Увеличивается каждый тик симуляции.
        /// Когда превышает ExpirationTicks, предложение
        /// считается истёкшим.
        /// </summary>
        public int ElapsedTicks { get; private set; } = 0;

        /// <summary>
        /// Индикатор активности предложения.
        /// 
        /// false если:
        /// - Предложение принято и выполнено
        /// - Истёк срок действия
        /// - Отменено продавцом
        /// - Продавец умер
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Курс обмена (требуемое / предлагаемое).
        /// 
        /// Значение > 1.0 = продавец в плюсе (требует больше чем даёт)
        /// Значение < 1.0 = продавец в минусе (даёт больше чем требует)
        /// Значение = 1.0 = честный обмен
        /// 
        /// Используется ботами для оценки выгодности сделки.
        /// </summary>
        public float ExchangeRate
        {
            get
            {
                if (OfferedAmount <= 0)
                    return 0;
                return RequiredAmount / OfferedAmount;
            }
        }

        /// <summary>
        /// Минимальное количество для частичной сделки.
        /// 
        /// Если другой бот не может выполнить сделку полностью,
        /// он может выполнить часть если количество >= MinAmount.
        /// 
        /// 0 = только полная сделка
        /// >0 = минимальное количество для частичной сделки
        /// </summary>
        public float MinAmount { get; set; } = 0;

        #endregion

        #region Конструкторы

        /// <summary>
        /// Создаёт новое предложение торговли.
        /// </summary>
        /// <param name="seller">
        /// Бот который создаёт предложение (продавец).
        /// </param>
        /// <param name="offeredResource">
        /// Тип ресурса который предлагается.
        /// </param>
        /// <param name="offeredAmount">
        /// Количество предлагаемого ресурса.
        /// </param>
        /// <param name="requiredResource">
        /// Тип ресурса который требуется взамен.
        /// </param>
        /// <param name="requiredAmount">
        /// Количество требуемого ресурса.
        /// </param>
        /// <param name="expirationTicks">
        /// Срок действия в тиках (0 = бессрочно).
        /// </param>
        public TradeOffer(
            Entities.Bot seller,
            ResourceType offeredResource,
            float offeredAmount,
            ResourceType requiredResource,
            float requiredAmount,
            int expirationTicks = 100)
        {
            _id = Guid.NewGuid();
            _seller = seller ?? throw new ArgumentNullException(nameof(seller));
            _createdTime = DateTime.Now;

            if (offeredResource == null)
                throw new ArgumentNullException(nameof(offeredResource));
            if (requiredResource == null)
                throw new ArgumentNullException(nameof(requiredResource));
            if (offeredAmount <= 0)
                throw new ArgumentException("Количество должно быть положительным", nameof(offeredAmount));
            if (requiredAmount <= 0)
                throw new ArgumentException("Количество должно быть положительным", nameof(requiredAmount));

            OfferedResource = offeredResource;
            OfferedAmount = offeredAmount;
            RequiredResource = requiredResource;
            RequiredAmount = requiredAmount;
            ExpirationTicks = expirationTicks;
            IsActive = true;
            MinAmount = offeredAmount * 0.1f; // 10% от суммы по умолчанию
        }

        #endregion

        #region Методы проверки

        /// <summary>
        /// Проверяет может ли бот принять это предложение.
        /// 
        /// Выполняет следующие проверки:
        /// 1. Предложение активно
        /// 2. Срок действия не истёк
        /// 3. Бот не является продавцом
        /// 4. У бота достаточно требуемых ресурсов
        /// 5. У бота есть место в инвентаре
        /// </summary>
        /// <param name="buyer">
        /// Бот который хочет принять предложение.
        /// </param>
        /// <returns>
        /// True если бот может принять предложение, иначе false.
        /// </returns>
        public bool CanAccept(Entities.Bot buyer)
        {
            if (!IsActive)
                return false;

            if (IsExpired)
                return false;

            if (buyer == _seller)
                return false;

            if (buyer.Inventory == null)
                return false;

            // Проверка наличия требуемых ресурсов
            if (!buyer.Inventory.HasEnough(RequiredResource, RequiredAmount))
                return false;

            // Проверка места в инвентаре для получаемого ресурса
            float netChange = OfferedAmount - RequiredAmount;
            if (netChange > 0)
            {
                float availableSpace = buyer.Inventory.MaxCapacity - buyer.Inventory.TotalWeight;
                if (buyer.Inventory.MaxCapacity >= 0 && availableSpace < netChange)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет истёк ли срок действия предложения.
        /// </summary>
        public bool IsExpired
        {
            get
            {
                if (ExpirationTicks <= 0)
                    return false;
                return ElapsedTicks >= ExpirationTicks;
            }
        }

        /// <summary>
        /// Обновляет счётчик тиков предложения.
        /// 
        /// Вызывается каждый тик симуляции для всех
        /// активных предложений.
        /// </summary>
        public void UpdateTick()
        {
            if (!IsActive)
                return;

            ElapsedTicks++;

            if (IsExpired)
            {
                IsActive = false;
            }
        }

        #endregion

        #region Выполнение сделки

        /// <summary>
        /// Выполняет торговую сделку между продавцом и покупателем.
        /// 
        /// Модифицирует инвентари обоих ботов:
        /// - Продавец теряет OfferedResource, получает RequiredResource
        /// - Покупатель теряет RequiredResource, получает OfferedResource
        /// 
        /// Важно: метод не проверяет CanAccept().
        /// Вызывающий код должен проверить возможность сделки.
        /// </summary>
        /// <param name="buyer">
        /// Бот который принимает предложение (покупатель).
        /// </param>
        /// <returns>
        /// True если сделка успешна, false если ошибка.
        /// </returns>
        public bool Execute(Entities.Bot buyer)
        {
            if (!CanAccept(buyer))
                return false;

            if (_seller.Inventory == null || buyer.Inventory == null)
                return false;

            // Продавец отдаёт ресурс
            _seller.Inventory.Remove(OfferedResource, OfferedAmount);

            // Продавец получает требуемый ресурс
            _seller.Inventory.Add(RequiredResource, RequiredAmount);

            // Покупатель отдаёт требуемый ресурс
            buyer.Inventory.Remove(RequiredResource, RequiredAmount);

            // Покупатель получает предлагаемый ресурс
            buyer.Inventory.Add(OfferedResource, OfferedAmount);

            // Помечаем предложение как выполненное
            IsActive = false;

            // Уведомляем о successful trade
            OnTradeCompleted?.Invoke(this, buyer);

            return true;
        }

        /// <summary>
        /// Выполняет частичную сделку.
        /// 
        /// Используется когда у покупателя недостаточно ресурсов
        /// для полной сделки но достаточно для частичной.
        /// </summary>
        /// <param name="buyer">
        /// Бот который принимает предложение.
        /// </param>
        /// <param name="partialAmount">
        /// Количество для частичной сделки.
        /// </param>
        /// <returns>
        /// True если сделка успешна, false если ошибка.
        /// </returns>
        public bool ExecutePartial(Entities.Bot buyer, float partialAmount)
        {
            if (partialAmount < MinAmount || partialAmount >= OfferedAmount)
                return false;

            if (!IsActive || IsExpired)
                return false;

            // Расчитываем пропорции
            float ratio = partialAmount / OfferedAmount;
            float requiredPartial = RequiredAmount * ratio;

            if (!buyer.Inventory.HasEnough(RequiredResource, requiredPartial))
                return false;

            // Продавец отдаёт часть ресурса
            _seller.Inventory.Remove(OfferedResource, partialAmount);
            _seller.Inventory.Add(RequiredResource, requiredPartial);

            // Покупатель отдаёт часть требуемого
            buyer.Inventory.Remove(RequiredResource, requiredPartial);
            buyer.Inventory.Add(OfferedResource, partialAmount);

            // Обновляем предложение
            OfferedAmount -= partialAmount;
            RequiredAmount -= requiredPartial;

            if (OfferedAmount <= MinAmount)
            {
                IsActive = false;
            }

            OnTradeCompleted?.Invoke(this, buyer);
            return true;
        }

        #endregion

        #region События

        /// <summary>
        /// Событие завершения торговой сделки.
        /// 
        /// Вызывается после успешного выполнения Execute().
        /// Аргументы: предложение, покупатель.
        /// </summary>
        public event Action<TradeOffer, Entities.Bot> OnTradeCompleted;

        #endregion

        #region Переопределения

        /// <summary>
        /// Сравнивает предложение с другим объектом.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as TradeOffer);
        }

        /// <summary>
        /// Сравнивает предложение с другим по ID.
        /// </summary>
        public bool Equals(TradeOffer other)
        {
            return other != null && _id == other._id;
        }

        /// <summary>
        /// Возвращает хеш-код предложения.
        /// </summary>
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        /// <summary>
        /// Возвращает строковое представление предложения.
        /// </summary>
        public override string ToString()
        {
            return $"{OfferedAmount} {OfferedResource?.Name} → {RequiredAmount} {RequiredResource?.Name} " +
                   $"(Rate: {ExchangeRate:F2}, Active: {IsActive})";
        }

        #endregion

        #region Управление состоянием

        /// <summary>
        /// Деактивирует предложение (отменяет его).
        /// 
        /// Вызывается когда продавец отменяет предложение
        /// или когда предложение истекает.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Проверяет, может ли продавец отменить это предложение.
        /// </summary>
        /// <param name="bot">Бот который пытается отменить.</param>
        /// <returns>True если бот является продавцом и предложение активно.</returns>
        public bool CanCancel(Entities.Bot bot)
        {
            return IsActive && _seller == bot;
        }

        #endregion
    }
}