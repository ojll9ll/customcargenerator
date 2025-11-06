using System;

namespace CustomCarExporter.Core.Converters
{
    /// <summary>
    /// Конвертер углов и схождения для интерфейса (веб/бот/консоль).
    /// - Углы принимаем/возвращаем в десятичных градусах.
    /// - Схождение конвертируем между миллиметрами (по диску) и градусами.
    ///
    /// Формулы:
    ///   rimMm = rimInches * 25.4
    ///   perWheelAngleDeg = atan(mm / rimMm) * 180 / PI
    ///   totalToeAngleDeg = 2 * atan((totalMm / 2) / rimMm) * 180 / PI
    ///
    /// Примечания:
    /// - perWheel = угол одного колеса; total = суммарное схождение оси.
    /// - mm подразумевается по ободу (диаметру диска), как в большинстве стендов.
    /// </summary>
    public static class AngleConverter
    {
        private const double DegPerRad = 180.0 / Math.PI;
        private const double MmPerInch = 25.4;

        /// <summary>
        /// Возвращает то же значение (если угол уже в десятичных градусах).
        /// Включено для единообразия API.
        /// </summary>
        public static double DegreesDecimal(double degrees) => degrees;

        /// <summary>
        /// Конвертация схождения одного колеса из мм в градусы при известном диаметре диска (в дюймах).
        /// mm — линейное схождение по ободу для одного колеса.
        /// </summary>
        public static double ToeMmToDegreesPerWheel(double mm, double rimDiameterInches)
        {
            double rimMm = rimDiameterInches * MmPerInch;
            if (rimMm <= 0) return 0;
            return Math.Atan(mm / rimMm) * DegPerRad;
        }

        /// <summary>
        /// Конвертация суммарного схождения оси из мм в градусы (total) при известном диаметре диска (в дюймах).
        /// totalMm — суммарная разница по краю диска слева/справа.
        /// </summary>
        public static double ToeMmToTotalDegrees(double totalMm, double rimDiameterInches)
        {
            double rimMm = rimDiameterInches * MmPerInch;
            if (rimMm <= 0) return 0;
            // total = 2 * atan((мм/2) / R)
            return 2.0 * Math.Atan((totalMm * 0.5) / rimMm) * DegPerRad;
        }

        /// <summary>
        /// Конвертация угла одного колеса в мм по ободу, при известном диаметре диска (в дюймах).
        /// </summary>
        public static double DegreesToToeMmPerWheel(double angleDeg, double rimDiameterInches)
        {
            double rimMm = rimDiameterInches * MmPerInch;
            double angleRad = angleDeg / DegPerRad;
            return Math.Tan(angleRad) * rimMm;
        }

        /// <summary>
        /// Конвертация суммарного угла оси (total) в мм по ободу, при известном диаметре диска (в дюймах).
        /// </summary>
        public static double TotalDegreesToToeMm(double totalAngleDeg, double rimDiameterInches)
        {
            double rimMm = rimDiameterInches * MmPerInch;
            double halfRad = (totalAngleDeg * 0.5) / DegPerRad;
            return 2.0 * Math.Tan(halfRad) * rimMm;
        }
    }
}

