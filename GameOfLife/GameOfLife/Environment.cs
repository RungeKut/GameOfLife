using System;

namespace GameOfLife
{
    public class Environment
    {
        public byte NutrientLevel { get; set; }
        public byte Toxicity { get; set; }
        public byte Temperature { get; set; }
        public byte Moisture { get; set; }
        public int EnergyPool { get; set; }

        public Environment()
        {
            Randomize();
        }

        public void Randomize()
        {
            Random rand = new Random();
            NutrientLevel = (byte)rand.Next(1, 11);
            Toxicity = (byte)rand.Next(0, 5);
            Temperature = (byte)rand.Next(3, 8);
            Moisture = (byte)rand.Next(1, 11);
            EnergyPool = NutrientLevel * 10;
        }

        public float GetSurvivalModifier(CellGenome genome)
        {
            float modifier = 1.0f;
            modifier += NutrientLevel * 0.05f;
            modifier -= Toxicity * 0.1f;

            if (genome != null)
            {
                float tempDiff = Math.Abs(Temperature - genome.Metabolism);
                modifier -= tempDiff * 0.05f;

                if (Moisture > 5 && genome.ReproductionRate > 5)
                    modifier += 0.1f;
            }

            return Math.Max(0.1f, Math.Min(2.0f, modifier));
        }

        public void ConsumeEnergy(int amount)
        {
            EnergyPool -= amount;
            if (EnergyPool < 0) EnergyPool = 0;
        }

        public void Regenerate()
        {
            if (EnergyPool < NutrientLevel * 10)
                EnergyPool++;
        }
    }
}