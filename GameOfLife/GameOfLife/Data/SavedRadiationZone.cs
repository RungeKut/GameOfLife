namespace GameOfLife.Data
{
    /// <summary>
    /// Сохранённая радиоактивная зона.
    /// </summary>
    public class SavedRadiationZone
    {
        public string Id { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int Radius { get; set; }
        public float RadiationLevel { get; set; }
        public string ZoneType { get; set; }
    }
}
