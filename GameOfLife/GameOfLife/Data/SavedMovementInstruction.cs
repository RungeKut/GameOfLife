namespace GameOfLife.Data
{
    /// <summary>
    /// Сохранённая инструкция движения.
    /// </summary>
    public class SavedMovementInstruction
    {
        public string Direction { get; set; }
        public int Duration { get; set; }
    }
}
