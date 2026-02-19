using System;
using System.Drawing;

namespace GameOfLife
{
    public class CellGenome
    {
        // === Базовые гены выживания ===
        public byte BirthThreshold { get; set; }
        public byte SurvivalMin { get; set; }
        public byte SurvivalMax { get; set; }

        // === Энергия и метаболизм ===
        public byte Metabolism { get; set; }
        public byte MaxEnergy { get; set; }
        public byte EnergyEfficiency { get; set; }

        // === Движение ===
        public byte MovementTendency { get; set; }
        public byte MovementSpeed { get; set; }
        public byte MovementCost { get; set; }

        // === Зрение ===
        public byte VisionRange { get; set; }
        public byte VisionAngle { get; set; }

        // === Охота ===
        public byte HuntStrength { get; set; }
        public byte HuntRange { get; set; }
        public byte HuntCooldown { get; set; }

        // === Защита ===
        public byte DefenseStrength { get; set; }
        public byte ProjectileSpawnRate { get; set; }
        public byte ProjectileDamage { get; set; }

        // === Размножение ===
        public byte ReproductionRate { get; set; }
        public byte OffspringCount { get; set; }

        // === Цвет и идентификаторы ===
        public Color GenomeColor { get; private set; }
        public int GenomeHash { get; private set; }
        public CellType CellType { get; private set; }

        public CellGenome()
        {
            Randomize();
        }

        public CellGenome(CellGenome parent)
        {
            Random rand = new Random();

            BirthThreshold = Mutate(parent.BirthThreshold, 1, rand);
            SurvivalMin = Mutate(parent.SurvivalMin, 1, rand);
            SurvivalMax = Mutate(parent.SurvivalMax, 1, rand);
            Metabolism = Mutate(parent.Metabolism, 2, rand);
            MaxEnergy = Mutate(parent.MaxEnergy, 1, rand);
            EnergyEfficiency = Mutate(parent.EnergyEfficiency, 2, rand);
            MovementTendency = Mutate(parent.MovementTendency, 2, rand);
            MovementSpeed = Mutate(parent.MovementSpeed, 1, rand);
            MovementCost = Mutate(parent.MovementCost, 1, rand);
            VisionRange = Mutate(parent.VisionRange, 2, rand);
            VisionAngle = Mutate(parent.VisionAngle, 1, rand);
            HuntStrength = Mutate(parent.HuntStrength, 2, rand);
            HuntRange = Mutate(parent.HuntRange, 1, rand);
            HuntCooldown = Mutate(parent.HuntCooldown, 2, rand);
            DefenseStrength = Mutate(parent.DefenseStrength, 2, rand);
            ProjectileSpawnRate = Mutate(parent.ProjectileSpawnRate, 2, rand);
            ProjectileDamage = Mutate(parent.ProjectileDamage, 1, rand);
            ReproductionRate = Mutate(parent.ReproductionRate, 2, rand);
            OffspringCount = Mutate(parent.OffspringCount, 1, rand);

            CalculateType();
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
            MaxEnergy = (byte)rand.Next(5, 11);
            EnergyEfficiency = (byte)rand.Next(1, 11);
            MovementTendency = (byte)rand.Next(0, 11);
            MovementSpeed = (byte)rand.Next(1, 6);
            MovementCost = (byte)rand.Next(1, 11);
            VisionRange = (byte)rand.Next(1, 11);
            VisionAngle = (byte)rand.Next(1, 9);
            HuntStrength = (byte)rand.Next(0, 11);
            HuntRange = (byte)rand.Next(1, 6);
            HuntCooldown = (byte)rand.Next(1, 11);
            DefenseStrength = (byte)rand.Next(0, 11);
            ProjectileSpawnRate = (byte)rand.Next(0, 11);
            ProjectileDamage = (byte)rand.Next(1, 11);
            ReproductionRate = (byte)rand.Next(1, 11);
            OffspringCount = (byte)rand.Next(1, 6);

            CalculateType();
            CalculateColor();
        }

        // ✅ ИЗМЕНЕНО: internal вместо private для доступа из GameEngine
        internal void CalculateType()
        {
            if (HuntStrength > 7 && MovementTendency > 5)
                CellType = CellType.Predator;
            else if (DefenseStrength > 7 || ProjectileSpawnRate > 5)
                CellType = CellType.Defender;
            else if (MovementTendency > 7 && HuntStrength < 3)
                CellType = CellType.Gatherer;
            else if (ProjectileSpawnRate > 7)
                CellType = CellType.Projectile;
            else
                CellType = CellType.Normal;
        }

        // ✅ ИЗМЕНЕНО: internal вместо private для доступа из GameEngine
        internal void CalculateColor()
        {
            GenomeHash = GetHashCode();

            int r, g, b;

            switch (CellType)
            {
                case CellType.Predator:
                    r = 200 + (HuntStrength * 5);
                    g = 50 + (MovementSpeed * 5);
                    b = 50;
                    break;
                case CellType.Defender:
                    r = 100;
                    g = 150 + (DefenseStrength * 10);
                    b = 100 + (ProjectileSpawnRate * 10);
                    break;
                case CellType.Gatherer:
                    r = 100 + (VisionRange * 10);
                    g = 200 + (EnergyEfficiency * 5);
                    b = 100;
                    break;
                case CellType.Projectile:
                    r = 255;
                    g = 100;
                    b = 255;
                    break;
                default:
                    r = (BirthThreshold * 25 + SurvivalMin * 15) % 256;
                    g = (Metabolism * 20 + ReproductionRate * 10) % 256;
                    b = (MovementTendency * 30 + SurvivalMax * 20) % 256;
                    break;
            }

            GenomeColor = Color.FromArgb(200, Math.Min(255, r), Math.Min(255, g), Math.Min(255, b));
        }

        // ✅ ДОБАВЛЕНО: Публичный метод для установки типа клетки из GameEngine
        public void SetCellType(CellType type)
        {
            CellType = type;
            CalculateColor();
        }

        public override int GetHashCode()
        {
            return BirthThreshold ^ (SurvivalMin << 2) ^ (SurvivalMax << 4) ^
                   (Metabolism << 6) ^ (ReproductionRate << 8) ^ (MovementTendency << 10) ^
                   (HuntStrength << 12) ^ (DefenseStrength << 14) ^ (VisionRange << 16);
        }

        public override bool Equals(object obj)
        {
            if (obj is CellGenome other)
            {
                return CellType == other.CellType &&
                       HuntStrength == other.HuntStrength &&
                       DefenseStrength == other.DefenseStrength &&
                       MovementTendency == other.MovementTendency;
            }
            return false;
        }
    }

    public enum CellType
    {
        Normal,
        Predator,
        Defender,
        Gatherer,
        Projectile
    }
}