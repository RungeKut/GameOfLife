namespace GameOfLife.Data
{
    /// <summary>
    /// Сохранённая магнитная аномалия.
    /// </summary>
    public class SavedMagneticField
    {
        public string Id { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int Radius { get; set; }
        public float DistortionStrength { get; set; }
    }
}
