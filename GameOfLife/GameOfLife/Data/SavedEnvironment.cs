using System.Collections.Generic;

namespace GameOfLife.Data
{
    /// <summary>
    /// Сохранённое состояние окружающей среды.
    /// </summary>
    public class SavedEnvironment
    {
        public float DayProgress { get; set; }
        public float Temperature { get; set; }
        public float WindStrength { get; set; }
        public float WindDirection { get; set; }
        public float Humidity { get; set; }
        public List<SavedRadiationZone> RadiationZones { get; set; }
        public List<SavedMagneticField> MagneticFields { get; set; }
    }
}
