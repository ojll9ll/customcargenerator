using System;

namespace CustomCarExporter.Core
{
    /// <summary>
    /// Единица спецификации (тип/значения). TypeKey может быть Guid либо строковый ключ, который позже маппится на Guid.
    /// </summary>
    public class SpecificationEntry
    {
        public string TypeKeyOrAlias { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Best { get; set; }
        public int Axle { get; set; } = 1; // по умолчанию передняя ось
    }
}

