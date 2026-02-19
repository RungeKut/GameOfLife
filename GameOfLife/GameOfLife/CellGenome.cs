using System;
using System.Drawing;

namespace GameOfLife
{
    public class CellGenome
    {
        // Гены, влияющие на поведение
        public byte BirthThreshold { get; set; }      // Сколько соседей нужно для рождения (0-8)
        public byte SurvivalMin { get; set; }         // Минимум соседей для выживания (0-8)
        public byte SurvivalMax { get; set; }         // Максимум соседей для выживания (0-8)
        public byte Metabolism { get; set; }          // Скорость потребления энергии (1-10)
        public byte ReproductionRate { get; set; }    // Шанс размножения (1-10)
        public byte MovementTendency { get; set; }    // Склонность к движению (0-10)

        // Цвет на основе генома
        public Color GenomeColor { get; private set; }

        // Уникальный ID генома
        public int GenomeHash { get; private set; }

        public CellGenome()
        {
            Randomize();
        }

        public CellGenome(CellGenome parent)
        {
            // Наследование с возможными мутациями
            Random rand = new Random();
            BirthThreshold = Mutate(parent.BirthThreshold, 1, rand);
            SurvivalMin = Mutate(parent.SurvivalMin, 1, rand);
            SurvivalMax = Mutate(parent.SurvivalMax, 1, rand);
            Metabolism = Mutate(parent.Metabolism, 2, rand);
            ReproductionRate = Mutate(parent.ReproductionRate, 2, rand);
            MovementTendency = Mutate(parent.MovementTendency, 2, rand);

            CalculateColor();
        }

        private byte Mutate(byte value, int maxChange, Random rand)
        {
            int change = rand.Next(-maxChange, maxChange + 1);
            int newValue = value + change;

            if (newValue < 1) newValue = 1;
            if (newValue > 10) newValue = 10;
            if (value == 0) newValue = 0;

            return (byte)newValue;
        }

        public void Randomize()
        {
            Random rand = new Random();
            BirthThreshold = (byte)rand.Next(2, 4);
            SurvivalMin = (byte)rand.Next(2, 3);
            SurvivalMax = (byte)rand.Next(3, 4);
            Metabolism = (byte)rand.Next(1, 11);
            ReproductionRate = (byte)rand.Next(1, 11);
            MovementTendency = (byte)rand.Next(0, 11);

            CalculateColor();
        }

        private void CalculateColor()
        {
            // Генерируем цвет на основе генома
            GenomeHash = GetHashCode();

            int r = (BirthThreshold * 25 + SurvivalMin * 15) % 256;
            int g = (Metabolism * 20 + ReproductionRate * 10) % 256;
            int b = (MovementTendency * 30 + SurvivalMax * 20) % 256;

            GenomeColor = Color.FromArgb(200, r, g, b);
        }

        public override int GetHashCode()
        {
            return BirthThreshold ^ (SurvivalMin << 2) ^ (SurvivalMax << 4) ^
                   (Metabolism << 6) ^ (ReproductionRate << 8) ^ (MovementTendency << 10);
        }

        public override bool Equals(object obj)
        {
            if (obj is CellGenome other)
            {
                return BirthThreshold == other.BirthThreshold &&
                       SurvivalMin == other.SurvivalMin &&
                       SurvivalMax == other.SurvivalMax &&
                       Metabolism == other.Metabolism &&
                       ReproductionRate == other.ReproductionRate &&
                       MovementTendency == other.MovementTendency;
            }
            return false;
        }
    }
}