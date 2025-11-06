using System;
using System.Collections.Generic;

namespace CustomCarExporter.Core
{
    /// <summary>
    /// Параметры для генерации пользовательского пакета (.customcar).
    /// </summary>
    public class ExportModelParams
    {
        public string ManufacturerName { get; set; }
        public string FolderName { get; set; }
        public string ModelName { get; set; }
        public DateTime? ProductionStart { get; set; }
        public DateTime? ProductionEnd { get; set; }
        public List<SpecificationEntry> Specifications { get; set; } = new List<SpecificationEntry>();

        /// <summary>
        /// Если true — использовать ручную запись XML (как в старых программах), иначе стандартная сериализация.
        /// </summary>
        public bool UseManualXml { get; set; }

        /// <summary>
        /// Включать дополнительные узлы (RimSize, Caster10Degrees и др.) при ручной записи.
        /// </summary>
        public bool IncludeExtras { get; set; } = true;
    }
}

