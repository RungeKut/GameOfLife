namespace GameOfLife.Data
{
    /// <summary>
    /// Сохранённая статистика бота.
    /// </summary>
    public class SavedBotStatistics
    {
        public string DeathReason { get; set; }
        public int? DeathTick { get; set; }
        public int OffspringCount { get; set; }
        public float MaxEnergyReached { get; set; }
        public float TotalResourcesGathered { get; set; }
    }
}
