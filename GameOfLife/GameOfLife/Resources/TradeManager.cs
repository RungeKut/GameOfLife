using System;
using System.Collections.Generic;
using System.Linq;
using GameOfLife.Entities;

namespace GameOfLife.Resources
{
    /// <summary>
    /// Менеджер торговой системы.
    /// 
    /// Управляет всеми торговыми предложениями в мире:
    /// - Публикация новых предложений
    /// - Поиск подходящих предложений для ботов
    /// - Выполнение торговых сделок
    /// - Очистка истёкших предложений
    /// 
    /// Архитектурные особенности:
    /// - Централизованное управление торговлей
    /// - Поддержка множественных валют (ресурсов)
    /// - Оптимизированный поиск предложений
    /// - События для уведомления об изменениях
    /// 
    /// Использование:
    /// var manager = TradeManager.Instance;
    /// manager.PublishOffer(bot, ResourceType.Food, 10, ResourceType.Water, 5);
    /// var offers = manager.GetOffersForBot(bot);
    /// </summary>
    public class TradeManager
    {
        #region Приватные поля

        /// <summary>
        /// Единственный экземпляр менеджера (синглтон).
        /// </summary>
        private static TradeManager _instance;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Все активные торговые предложения.
        /// Ключ: ID предложения, Значение: предложение.
        /// </summary>
        private readonly Dictionary<Guid, TradeOffer> _offers;

        /// <summary>
        /// Предложения сгруппированные по предлагаемому ресурсу.
        /// Для быстрого поиска по типу ресурса.
        /// </summary>
        private readonly Dictionary<ResourceType, List<TradeOffer>> _offersByResource;

        /// <summary>
        /// Генератор случайных чисел.
        /// </summary>
        private readonly Random _random;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Единственный экземпляр менеджера.
        /// </summary>
        public static TradeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(TradeManager))
                    {
                        if (_instance == null)
                        {
                            _instance = new TradeManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Количество активных предложений.
        /// </summary>
        public int ActiveOfferCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _offers.Count(o => o.Value.IsActive);
                }
            }
        }

        #endregion

        #region События

        /// <summary>
        /// Событие публикации нового предложения.
        /// </summary>
        public event Action<TradeOffer> OnOfferPublished;

        /// <summary>
        /// Событие выполнения торговой сделки.
        /// </summary>
        public event Action<TradeOffer, Bot, Bot> OnTradeExecuted;

        /// <summary>
        /// Событие удаления истёкшего предложения.
        /// </summary>
        public event Action<TradeOffer> OnOfferExpired;

        #endregion

        #region Конструктор

        private TradeManager()
        {
            _offers = new Dictionary<Guid, TradeOffer>();
            _offersByResource = new Dictionary<ResourceType, List<TradeOffer>>();
            _random = new Random();
        }

        #endregion

        #region Публикация предложений

        /// <summary>
        /// Публикует новое торговое предложение.
        /// </summary>
        /// <param name="seller">
        /// Бот который создаёт предложение.
        /// </param>
        /// <param name="offeredResource">
        /// Тип предлагаемого ресурса.
        /// </param>
        /// <param name="offeredAmount">
        /// Количество предлагаемого ресурса.
        /// </param>
        /// <param name="requiredResource">
        /// Тип требуемого ресурса.
        /// </param>
        /// <param name="requiredAmount">
        /// Количество требуемого ресурса.
        /// </param>
        /// <param name="expirationTicks">
        /// Срок действия в тиках.
        /// </param>
        /// <returns>
        /// Созданное предложение или null если ошибка.
        /// </returns>
        public TradeOffer PublishOffer(
            Bot seller,
            ResourceType offeredResource,
            float offeredAmount,
            ResourceType requiredResource,
            float requiredAmount,
            int expirationTicks = 100)
        {
            if (seller == null || !seller.IsActive)
                return null;

            if (seller.Inventory == null)
                return null;

            // Проверка наличия ресурса у продавца
            if (!seller.Inventory.HasEnough(offeredResource, offeredAmount))
                return null;

            var offer = new TradeOffer(
                seller,
                offeredResource,
                offeredAmount,
                requiredResource,
                requiredAmount,
                expirationTicks
            );

            offer.OnTradeCompleted += OnTradeCompleted;

            lock (_lockObject)
            {
                _offers[offer.Id] = offer;

                // Добавляем в индекс по ресурсу
                if (!_offersByResource.ContainsKey(offeredResource))
                {
                    _offersByResource[offeredResource] = new List<TradeOffer>();
                }
                _offersByResource[offeredResource].Add(offer);

                OnOfferPublished?.Invoke(offer);
            }

            return offer;
        }

        /// <summary>
        /// Отменяет предложение по ID.
        /// </summary>
        public bool CancelOffer(Guid offerId)
        {
            lock (_lockObject)
            {
                if (_offers.TryGetValue(offerId, out var offer))
                {
                    // ✅ Используем публичный метод вместо прямого присваивания
                    offer.Deactivate();

                    _offers.Remove(offerId);

                    // Удаляем из индекса
                    if (_offersByResource.ContainsKey(offer.OfferedResource))
                    {
                        _offersByResource[offer.OfferedResource].RemoveAll(o => o.Id == offerId);
                    }

                    return true;
                }
                return false;
            }
        }

        #endregion

        #region Поиск предложений

        /// <summary>
        /// Получает все активные предложения.
        /// </summary>
        public List<TradeOffer> GetAllOffers()
        {
            lock (_lockObject)
            {
                return _offers.Values.Where(o => o.IsActive).ToList();
            }
        }

        /// <summary>
        /// Получает предложения для конкретного бота.
        /// 
        /// Фильтрует предложения которые:
        /// - Активны
        /// - Не созданы этим ботом
        /// - Бот может принять
        /// </summary>
        public List<TradeOffer> GetOffersForBot(Bot bot)
        {
            var result = new List<TradeOffer>();

            lock (_lockObject)
            {
                foreach (var offer in _offers.Values)
                {
                    if (offer.IsActive && offer.Seller != bot && offer.CanAccept(bot))
                    {
                        result.Add(offer);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Получает предложения по типу ресурса.
        /// </summary>
        public List<TradeOffer> GetOffersByResource(ResourceType resource)
        {
            lock (_lockObject)
            {
                if (_offersByResource.TryGetValue(resource, out var offers))
                {
                    return offers.Where(o => o.IsActive).ToList();
                }
                return new List<TradeOffer>();
            }
        }

        /// <summary>
        /// Находит лучшее предложение для бота.
        /// 
        /// Критерии:
        /// 1. Бот может принять
        /// 2. Минимальный курс обмена
        /// 3. Максимальное количество
        /// </summary>
        public TradeOffer GetBestOfferForBot(Bot bot)
        {
            var offers = GetOffersForBot(bot);

            if (offers.Count == 0)
                return null;

            return offers
                .OrderBy(o => o.ExchangeRate)
                .ThenByDescending(o => o.OfferedAmount)
                .FirstOrDefault();
        }

        #endregion

        #region Обновление

        /// <summary>
        /// Обновляет все предложения (тик симуляции).
        /// 
        /// Вызывается каждый тик движком.
        /// - Увеличивает счётчик тиков
        /// - Удаляет истёкшие предложения
        /// </summary>
        public void Update()
        {
            lock (_lockObject)
            {
                var expired = new List<TradeOffer>();

                foreach (var offer in _offers.Values)
                {
                    if (offer.IsActive)
                    {
                        offer.UpdateTick();

                        if (offer.IsExpired)
                        {
                            expired.Add(offer);
                        }
                    }
                    else
                    {
                        expired.Add(offer);
                    }
                }

                // Удаляем истёкшие
                foreach (var offer in expired)
                {
                    _offers.Remove(offer.Id);
                    if (_offersByResource.ContainsKey(offer.OfferedResource))
                    {
                        _offersByResource[offer.OfferedResource].RemoveAll(o => o.Id == offer.Id);
                    }
                    OnOfferExpired?.Invoke(offer);
                }
            }
        }

        #endregion

        #region Обработчики событий

        private void OnTradeCompleted(TradeOffer offer, Bot buyer)
        {
            OnTradeExecuted?.Invoke(offer, offer.Seller, buyer);
        }

        #endregion

        #region Утилиты

        /// <summary>
        /// Очищает все предложения.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _offers.Clear();
                _offersByResource.Clear();
            }
        }

        /// <summary>
        /// Удаляет все предложения бота (при смерти).
        /// </summary>
        public void RemoveBotOffers(Bot bot)
        {
            if (bot == null)
                return;

            lock (_lockObject)
            {
                var botOffers = _offers.Values.Where(o => o.Seller == bot).ToList();

                foreach (var offer in botOffers)
                {
                    _offers.Remove(offer.Id);
                    if (_offersByResource.ContainsKey(offer.OfferedResource))
                    {
                        _offersByResource[offer.OfferedResource].RemoveAll(o => o.Id == offer.Id);
                    }
                }
            }
        }

        #endregion
    }
}