using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Environment
{
    /// <summary>
    /// Эффект воздействия магнитной аномалии.
    /// </summary>
    public class MagneticEffect
    {
        /// <summary>
        /// Уровень дезориентации (0.0 - 1.0).
        /// </summary>
        public float DistortionLevel { get; set; }

        /// <summary>
        /// Бот дезориентирован в этом тике.
        /// </summary>
        public bool IsDisoriented { get; set; }
    }
}
