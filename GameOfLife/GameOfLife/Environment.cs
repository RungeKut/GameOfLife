using System;

namespace GameOfLife
{
    public class Environment
    {
        // === Характеристики почвы ===
        public byte NutrientLevel { get; set; }      // Уровень питательных веществ (0-10)
        public byte Toxicity { get; set; }           // Токсичность (0-10)
        public byte Temperature { get; set; }        // Температура (0-10)
        public byte Moisture { get; set; }           // Влажность (0-10)
        public byte OxygenLevel { get; set; }        // Уровень кислорода (0-10)
        public byte RadiationLevel { get; set; }     // Уровень радиации (0-10)

        // === Энергия и ресурсы ===
        public int EnergyPool { get; set; }          // Запас энергии в клетке почвы
        public int Biomass { get; set; }             // Биомасса от мёртвых клеток

        // === Константы ===
        private const byte MAX_VALUE = 10;
        private const byte TOXICITY_DECAY_RATE = 1;
        private const byte NUTRIENT_REGEN_RATE = 1;

        public Environment()
        {
            Randomize();
        }

        public void Randomize()
        {
            Random rand = new Random();
            NutrientLevel = (byte)rand.Next(3, 11);
            Toxicity = (byte)rand.Next(0, 4);
            Temperature = (byte)rand.Next(3, 8);
            Moisture = (byte)rand.Next(3, 11);
            OxygenLevel = (byte)rand.Next(5, 11);
            RadiationLevel = (byte)rand.Next(0, 3);
            EnergyPool = NutrientLevel * 10;
            Biomass = 0;
        }

        // === ВЛИЯНИЕ КЛЕТОК НА СРЕДУ ===

        public void OnCellBirth()
        {
            NutrientLevel = (byte)Math.Max(0, NutrientLevel - 1);
            EnergyPool = Math.Max(0, EnergyPool - 5);
            Moisture = (byte)Math.Max(0, Moisture - 1);
            OxygenLevel = (byte)Math.Max(0, OxygenLevel - 1);
        }

        public void OnCellSurvival(byte metabolism)
        {
            EnergyPool = Math.Max(0, EnergyPool - metabolism);
            NutrientLevel = (byte)Math.Max(0, NutrientLevel - (metabolism / 3));
            OxygenLevel = (byte)Math.Max(0, OxygenLevel - 1);
        }

        public void OnCellDeath(byte biomass)
        {
            Toxicity = (byte)Math.Min(MAX_VALUE, Toxicity + 2);
            NutrientLevel = (byte)Math.Min(MAX_VALUE, NutrientLevel + 1);
            Biomass += biomass;
            RadiationLevel = (byte)Math.Min(MAX_VALUE, RadiationLevel + 1);
        }

        public void OnCellMove(byte movementCost)
        {
            // Движение уплотняет почву
            Toxicity = (byte)Math.Min(MAX_VALUE, Toxicity + 1);
            OxygenLevel = (byte)Math.Max(0, OxygenLevel - 1);
            Temperature = (byte)Math.Min(MAX_VALUE, Temperature + 1);
        }

        public void OnHunt(byte huntStrength)
        {
            // Охота повышает токсичность (кровь)
            Toxicity = (byte)Math.Min(MAX_VALUE, Toxicity + huntStrength / 2);
            Biomass += huntStrength;
        }

        public void OnProjectileImpact(byte damage)
        {
            // Снаряды создают радиацию
            RadiationLevel = (byte)Math.Min(MAX_VALUE, RadiationLevel + damage / 2);
            Toxicity = (byte)Math.Min(MAX_VALUE, Toxicity + damage / 3);
        }

        // === ВЛИЯНИЕ СРЕДЫ НА КЛЕТКИ ===

        public float GetSurvivalModifier(CellGenome genome)
        {
            float modifier = 1.0f;

            modifier += NutrientLevel * 0.08f;
            modifier -= Toxicity * 0.15f;
            modifier += OxygenLevel * 0.05f;
            modifier -= RadiationLevel * 0.2f;

            if (genome != null)
            {
                float tempDiff = Math.Abs(Temperature - genome.Metabolism);
                modifier -= tempDiff * 0.08f;

                if (Moisture > 5 && genome.ReproductionRate > 5)
                    modifier += 0.15f;

                if (genome.Metabolism > 7 && Toxicity > 5)
                    modifier -= 0.2f;

                // Клетки с высоким зрением лучше находят ресурсы
                if (genome.VisionRange > 7 && NutrientLevel > 5)
                    modifier += 0.1f;
            }

            return Math.Max(0.1f, Math.Min(2.0f, modifier));
        }

        public float GetVisionModifier(CellGenome genome)
        {
            // Туман, радиация и токсичность уменьшают видимость
            float visibility = 1.0f;
            visibility -= RadiationLevel * 0.1f;
            visibility -= Toxicity * 0.05f;
            visibility += Moisture * 0.02f;

            return Math.Max(0.1f, Math.Min(1.0f, visibility));
        }

        public void ConsumeEnergy(int amount)
        {
            EnergyPool -= amount;
            if (EnergyPool < 0) EnergyPool = 0;
        }

        public void Regenerate()
        {
            if (Toxicity > 0)
                Toxicity = (byte)(Toxicity - TOXICITY_DECAY_RATE);

            if (NutrientLevel < MAX_VALUE && EnergyPool > 0)
            {
                NutrientLevel = (byte)(NutrientLevel + NUTRIENT_REGEN_RATE);
                EnergyPool -= 2;
            }

            if (Temperature > 5)
                Temperature = (byte)(Temperature - 1);

            if (Moisture < 8)
                Moisture = (byte)(Moisture + 1);

            if (OxygenLevel < 10)
                OxygenLevel = (byte)(OxygenLevel + 1);

            if (RadiationLevel > 0)
                RadiationLevel = (byte)(RadiationLevel - 1);

            if (Biomass > 0)
            {
                Biomass -= 1;
                NutrientLevel = (byte)Math.Min(MAX_VALUE, NutrientLevel + 1);
            }

            if (EnergyPool < NutrientLevel * 10)
                EnergyPool += 1;
        }
    }
}