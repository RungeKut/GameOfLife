using System;

namespace GameOfLife.Data
{
    /// <summary>
    /// Информация о файле сохранения.
    /// 
    /// Содержит метаданные которые можно прочитать
    /// без полной загрузки сохранения.
    /// </summary>
    public class SaveInfo
    {
        /// <summary>
        /// Путь к файлу сохранения.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Дата создания сохранения.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Дата последнего изменения.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Размер файла в байтах.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Версия формата сохранения.
        /// </summary>
        public int SaveVersion { get; set; }

        /// <summary>
        /// Название сохранения (если есть).
        /// </summary>
        public string SaveName { get; set; }

        /// <summary>
        /// Режим симуляции (Conway, Ecosystem).
        /// </summary>
        public string SimulationMode { get; set; }

        /// <summary>
        /// Номер последнего сохранённого поколения.
        /// </summary>
        public long LastGeneration { get; set; }

        /// <summary>
        /// Количество живых клеток на момент сохранения.
        /// </summary>
        public int LiveCellCount { get; set; }

        /// <summary>
        /// Количество ботов (для режима экосистемы).
        /// </summary>
        public int BotCount { get; set; }
    }
}
