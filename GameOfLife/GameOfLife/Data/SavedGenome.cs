using System.Collections.Generic;

namespace GameOfLife.Data
{
    /// <summary>
    /// Сохранённый геном бота.
    /// </summary>
    public class SavedGenome
    {
        public float MetabolismRate { get; set; }
        public float MaxHealth { get; set; }
        public float MaxEnergy { get; set; }
        public int MaxAge { get; set; }
        public float Aggression { get; set; }
        public float Sociability { get; set; }
        public int MovementSpeed { get; set; }
        public float DefenseFactor { get; set; }
        public float ReproductionThreshold { get; set; }
        public float MutationRate { get; set; }
        public List<SavedMovementInstruction> MovementProgram { get; set; }
        public Dictionary<string, float> ResourcePreferences { get; set; }
    }
}
