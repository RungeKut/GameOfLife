using System;

namespace GameOfLife.Data
{
    /// <summary>
    /// Статистика симуляции.
    /// </summary>
    public class SimulationStatistics
    {
        public long MaxGeneration { get; set; }
        public int PeakLiveCells { get; set; }
        public int TotalBotsCreated { get; set; }
        public TimeSpan TotalSimulationTime { get; set; }
        public DateTime FirstSaveDate { get; set; }
        public DateTime LastSaveDate { get; set; }
    }
}
