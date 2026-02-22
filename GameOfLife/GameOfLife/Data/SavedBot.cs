using System.Collections.Generic;

namespace GameOfLife.Data
{
    /// <summary>
    /// Сохранённое состояние бота.
    /// 
    /// Содержит все данные необходимые для восстановления
    /// бота после загрузки сохранения.
    /// </summary>
    public class SavedBot
    {
        /// <summary>
        /// Уникальный идентификатор бота.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Горизонтальная координата.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Вертикальная координата.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Текущее здоровье.
        /// </summary>
        public float Health { get; set; }

        /// <summary>
        /// Текущая энергия.
        /// </summary>
        public float Energy { get; set; }

        /// <summary>
        /// Текущий возраст.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Сохранённый геном.
        /// </summary>
        public SavedGenome Genome { get; set; }

        /// <summary>
        /// Инвентарь ресурсов.
        /// </summary>
        public List<SavedResource> Inventory { get; set; } = new List<SavedResource>();

        /// <summary>
        /// Статистика бота.
        /// </summary>
        public SavedBotStatistics Statistics { get; set; }
    }
}
