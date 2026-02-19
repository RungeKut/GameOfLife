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
        public int EnergyPool { get; set; }          // Запас энергии в клетке почвы

        // === Константы ===
        private const byte MAX_TOXICITY = 10;
        private const byte MAX_NUTRIENT = 10;
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
            EnergyPool = NutrientLevel * 10;
        }

        // === ВЛИЯНИЕ КЛЕТОК НА СРЕДУ ===

        // Клетка родилась - потребляет питательные вещества
        public void OnCellBirth()
        {
            NutrientLevel = (byte)Math.Max(0, NutrientLevel - 1);
            EnergyPool = Math.Max(0, EnergyPool - 5);
            Moisture = (byte)Math.Max(0, Moisture - 1);
        }

        // Клетка выжила - потребляет энергию
        public void OnCellSurvival(byte metabolism)
        {
            EnergyPool = Math.Max(0, EnergyPool - metabolism);
            NutrientLevel = (byte)Math.Max(0, NutrientLevel - (metabolism / 3));
        }

        // Клетка умерла - повышает токсичность (разложение)
        public void OnCellDeath()
        {
            Toxicity = (byte)Math.Min(MAX_TOXICITY, Toxicity + 2);
            NutrientLevel = (byte)Math.Min(MAX_NUTRIENT, NutrientLevel + 1);
        }

        // Клетка с высоким метаболизмом - повышает температуру
        public void OnHighMetabolism(byte metabolism)
        {
            if (metabolism > 7)
            {
                Temperature = (byte)Math.Min(MAX_TOXICITY, Temperature + 1);
            }
        }

        // === ВЛИЯНИЕ СРЕДЫ НА КЛЕТКИ ===

        public float GetSurvivalModifier(CellGenome genome)
        {
            float modifier = 1.0f;

            // Питательные вещества увеличивают выживаемость
            modifier += NutrientLevel * 0.08f;

            // Токсичность уменьшает выживаемость
            modifier -= Toxicity * 0.15f;

            // Температура должна соответствовать метаболизму
            if (genome != null)
            {
                float tempDiff = Math.Abs(Temperature - genome.Metabolism);
                modifier -= tempDiff * 0.08f;

                // Влажность влияет на размножение
                if (Moisture > 5 && genome.ReproductionRate > 5)
                    modifier += 0.15f;

                // Клетки с высоким метаболизмом более чувствительны к токсинам
                if (genome.Metabolism > 7 && Toxicity > 5)
                    modifier -= 0.2f;
            }

            return Math.Max(0.1f, Math.Min(2.0f, modifier));
        }

        // Потребление энергии клеткой
        public void ConsumeEnergy(int amount)
        {
            EnergyPool -= amount;
            if (EnergyPool < 0) EnergyPool = 0;
        }

        // Регенерация среды
        public void Regenerate()
        {
            // Токсичность постепенно снижается
            if (Toxicity > 0)
                Toxicity = (byte)(Toxicity - TOXICITY_DECAY_RATE);

            // Питательные вещества восстанавливаются
            if (NutrientLevel < MAX_NUTRIENT && EnergyPool > 0)
            {
                NutrientLevel = (byte)(NutrientLevel + NUTRIENT_REGEN_RATE);
                EnergyPool -= 2;
            }

            // Температура нормализуется
            if (Temperature > 5)
                Temperature = (byte)(Temperature - 1);

            // Влажность восстанавливается
            if (Moisture < 8)
                Moisture = (byte)(Moisture + 1);

            // Энергия восстанавливается от питательных веществ
            if (EnergyPool < NutrientLevel * 10)
                EnergyPool += 1;
        }

        // Получить цвет среды для визуализации (отладка)
        public System.Drawing.Color GetEnvironmentColor()
        {
            int r = Toxicity * 25;
            int g = NutrientLevel * 25;
            int b = Moisture * 25;
            return System.Drawing.Color.FromArgb(50, r, g, b);
        }
    }
}