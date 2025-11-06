using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Techno.Vector.Database;
using CustomCarExporter.Core.Aliases;

namespace CustomCarExporter.Core
{
    /// <summary>
    /// Ядро экспорта пользовательских моделей в формат ArrayOfModel (.customcar).
    /// </summary>
    public static class CustomCarExporter
    {
        /// <summary>
        /// Сгенерировать .customcar и вернуть как массив байт.
        /// </summary>
        public static byte[] GeneratePackage(ExportModelParams input)
        {
            var temp = Path.Combine(Path.GetTempPath(), $"custom_{Guid.NewGuid():N}.customcar");
            GenerateToFile(input, temp);
            var bytes = File.ReadAllBytes(temp);
            File.Delete(temp);
            return bytes;
        }

        /// <summary>
        /// Сгенерировать .customcar и сохранить в файл.
        /// </summary>
        public static void GenerateToFile(ExportModelParams input, string outputPath)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (string.IsNullOrWhiteSpace(input.ModelName)) throw new ArgumentException("ModelName is required");

            var manufacturerName = string.IsNullOrWhiteSpace(input.ManufacturerName) ? "Unknown" : input.ManufacturerName.Trim();
            var folderName = input.FolderName;

            // Построение основной модели
            var model = BuildVectorModel(input, manufacturerName, folderName);

            // Узел-производитель
            var manufacturerModel = new Model
            {
                ID = Guid.NewGuid(),
                Name = manufacturerName,
                IsCustomModel = true,
                ParentKey = Guid.Empty,
                MinProductionDate = DateTime.MinValue,
                MaxProductionDate = DateTime.MinValue,
                DocumentKeys = null
            };

            // Присваиваем ParentKey
            model.ParentKey = manufacturerModel.ID;

            var package = new List<Model> { model, manufacturerModel };

            if (input.UseManualXml)
            {
                ManualWriteArrayOfModel(package, outputPath, input.IncludeExtras);
                return;
            }

            // Сохранение в файл (ArrayOfModel) через XmlSerializer в UTF-16
            var serializer = new XmlSerializer(typeof(Model[]));
            var settings = new XmlWriterSettings
            {
                Encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: true), // UTF-16 LE with BOM
                Indent = true
            };
            using (var fs = File.Create(outputPath))
            using (var xw = XmlWriter.Create(fs, settings))
            {
                serializer.Serialize(xw, package.ToArray());
            }
        }

        private static Model BuildVectorModel(ExportModelParams input, string manufacturerName, string folderName)
        {
            var modelName = input.ModelName?.Trim() ?? "Unknown";
            if (!string.IsNullOrWhiteSpace(folderName) && !modelName.StartsWith(folderName + " "))
            {
                modelName = folderName + " " + modelName;
            }

            var m = new Model
            {
                ID = Guid.NewGuid(),
                Name = modelName,
                IsCustomModel = true,
                ParentKey = Guid.Empty,
                MinProductionDate = input.ProductionStart ?? DateTime.MinValue,
                MaxProductionDate = input.ProductionEnd ?? DateTime.MinValue,
                DocumentKeys = null
            };

            // SearchName
            if (!string.IsNullOrWhiteSpace(manufacturerName))
            {
                m.SearchName = manufacturerName + ">" + (m.Name ?? "Unknown");
            }

            // Спецификации (только разрешённые типы)
            if (input.Specifications != null && input.Specifications.Any())
            {
                var allowed = new HashSet<Guid>(SpecTypeAliases.AllowedGuids);
                var list = new List<Specification>();
                foreach (var s in input.Specifications)
                {
                    var typeKey = ResolveSpecTypeKey(s.TypeKeyOrAlias);
                    if (typeKey == Guid.Empty || !allowed.Contains(typeKey))
                    {
                        continue; // пропускаем неразрешённые
                    }

                    var best = s.Best ?? 0.0;
                    var minTol = s.Min ?? 0.0; // трактуем как нижний допуск
                    var maxTol = s.Max ?? 0.0; // трактуем как верхний допуск

                    var spec = new Specification
                    {
                        ID = Guid.Empty,
                        TypeKey = typeKey,
                        BestValue = ToVectorAngle(best),
                        MinValue = ToVectorAngle(best - minTol),
                        MaxValue = ToVectorAngle(best + maxTol),
                        Axle = s.Axle
                    };
                    list.Add(spec);
                }

                if (list.Count > 0)
                {
                    m.Specifications = list;
                    m.SaveSpecification2Data();
                }
            }

            return m;
        }

        private static Guid ResolveSpecTypeKey(string keyOrAlias)
        {
            if (string.IsNullOrWhiteSpace(keyOrAlias)) return Guid.Empty;
            if (SpecTypeAliases.TryResolve(keyOrAlias, out var g)) return g;
            return Guid.Empty;
        }

        private static double ToVectorAngle(double value)
        {
            // Предполагаем, что вход уже в десятичных градусах.
            return value;
        }

        private static void ManualWriteArrayOfModel(List<Model> package, string outputPath, bool includeExtras)
        {
            using (var writer = new StreamWriter(outputPath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<ArrayOfModel>");

                foreach (var model in package)
                {
                    writer.WriteLine("  <Model>");
                    writer.WriteLine($"    <Name>{System.Security.SecurityElement.Escape(model.Name ?? string.Empty)}</Name>");

                    if (includeExtras)
                    {
                        if (model.ParentKey == Guid.Empty)
                            writer.WriteLine("    <RimSize xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />");
                        else
                            writer.WriteLine("    <RimSize>14</RimSize>");
                    }

                    // Pictures пустой список (как в примере)
                    writer.WriteLine("    <Pictures />");

                    if (!string.IsNullOrEmpty(model.SearchName))
                        writer.WriteLine($"    <SearchName>{System.Security.SecurityElement.Escape(model.SearchName)}</SearchName>");

                    writer.WriteLine($"    <MinProductionDate>{model.MinProductionDate:yyyy-MM-ddTHH:mm:ss}</MinProductionDate>");
                    writer.WriteLine($"    <MaxProductionDate>{model.MaxProductionDate:yyyy-MM-ddTHH:mm:ss}</MaxProductionDate>");

                    // VINMasks пустой список
                    writer.WriteLine("    <VINMasks />");

                    writer.WriteLine("    <LegacyID xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />");
                    writer.WriteLine("    <FirebirdID xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />");
                    writer.WriteLine($"    <LegacyType>{model.LegacyType}</LegacyType>");
                    writer.WriteLine($"    <ID>{model.ID}</ID>");

                    // Properties/specifications/specdata — только у модели (не у папки)
                    if (model.ParentKey != Guid.Empty)
                    {
                        // Properties (пустые)
                        writer.WriteLine("    <Properties />");

                        // Specifications
                        if (model.Specifications != null && model.Specifications.Any())
                        {
                            writer.WriteLine("    <Specifications>");
                            foreach (var spec in model.Specifications)
                            {
                                writer.WriteLine("      <Specification>");
                                writer.WriteLine($"        <TypeKey>{spec.TypeKey}</TypeKey>");
                                writer.WriteLine($"        <BaseSpecTypeID>{spec.BaseSpecTypeID}</BaseSpecTypeID>");
                                writer.WriteLine($"        <MinValue>{spec.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}</MinValue>");
                                writer.WriteLine($"        <MaxValue>{spec.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}</MaxValue>");
                                writer.WriteLine($"        <BestValue>{spec.BestValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}</BestValue>");
                                writer.WriteLine("        <IsAdjustable>false</IsAdjustable>");
                                writer.WriteLine($"        <ID>{spec.ID}</ID>");
                                writer.WriteLine("      </Specification>");
                            }
                            writer.WriteLine("    </Specifications>");
                        }
                        else
                        {
                            writer.WriteLine("    <Specifications />");
                        }

                        // SpecificationData
                        if (model.SpecificationData != null && model.SpecificationData.Length > 0)
                        {
                            writer.WriteLine($"    <SpecificationData>{System.Convert.ToBase64String(model.SpecificationData)}</SpecificationData>");
                        }
                        else
                        {
                            writer.WriteLine("    <SpecificationData />");
                        }

                        // DocumentKeys (пусто)
                        writer.WriteLine("    <DocumentKeys />");
                    }

                    writer.WriteLine($"    <IsCustomModel>{(model.IsCustomModel ? "true" : "false")}</IsCustomModel>");
                    writer.WriteLine($"    <ParentKey>{model.ParentKey}</ParentKey>");
                    writer.WriteLine($"    <Deprecated>{(model.Deprecated ? "true" : "false")}</Deprecated>");
                    writer.WriteLine("    <LastUpdated xsi:nil=\"true\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />");

                    if (includeExtras)
                    {
                        writer.WriteLine("    <Caster10Degrees>false</Caster10Degrees>");
                        writer.WriteLine("    <Caster20Degrees>false</Caster20Degrees>");
                        writer.WriteLine("    <Caster_Adjustment>false</Caster_Adjustment>");
                        writer.WriteLine("    <SteeringLock>false</SteeringLock>");
                        writer.WriteLine("    <UseTargetToMinMaxMeasurement>false</UseTargetToMinMaxMeasurement>");
                    }

                    writer.WriteLine("  </Model>");
                }

                writer.WriteLine("</ArrayOfModel>");
            }
        }
    }
}

