using System;
using System.Collections.Generic;
using System.Globalization;

namespace CustomCarExporter.Core.Aliases
{
    /// <summary>
    /// Централизованный словарь алиасов типов спецификаций → GUID.
    /// Ключи нечувствительны к регистру, пробелам, дефисам и подчёркиваниям.
    /// </summary>
    public static class SpecTypeAliases
    {
        // Источник GUIDов предоставлен пользователем из базы Vector.
        private static readonly Dictionary<string, Guid> aliasToGuid = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
        {
            // Передняя ось: Camber L/R
            { Normalize("camber_front_left"),   Guid.Parse("918580AB-6531-435A-B63B-9929F4E06173") }, // LeftCamber_Control
            { Normalize("camber_front_right"),  Guid.Parse("12A0C794-2052-42CF-9308-6A3229900B8C") }, // RightCamber_Contrl

            // Передняя ось: Caster L/R
            { Normalize("caster_front_left"),   Guid.Parse("29301B8E-EE21-4F0E-A515-10A52B736DE5") }, // LeftCaster
            { Normalize("caster_front_right"),  Guid.Parse("7440FE93-1D1A-4770-9E0C-1D8C5A7BA939") }, // RightCaster

            // Передняя ось: Toe total/L/R (controlled)
            { Normalize("toe_front_total"),     Guid.Parse("2BC86976-B40D-4659-84D0-18986E7AF9D2") }, // Toe_Cntrl
            { Normalize("toe_front_left"),      Guid.Parse("2BC86976-1AAD-1451-84D0-18986E7AF902") }, // Toe_Left_Cntrl
            { Normalize("toe_front_right"),     Guid.Parse("DDDD6916-0AAD-1451-84D0-18986E7AF902") }, // Toe_Right_Cntrl

            // Передняя ось: SAI L/R
            { Normalize("sai_front_left"),      Guid.Parse("C55DF4A3-6889-4C68-BB35-C2C8C4CC0979") }, // LeftSai
            { Normalize("sai_front_right"),     Guid.Parse("B2A5CF5D-23B2-449A-A9B7-7EB6C85E5C7F") }, // RightSAI

            // Задняя ось: camber L/R → общий Rear Camber; toe total (Uncontrolled)
            { Normalize("camber_rear_left"),    Guid.Parse("C6241905-C753-48C9-9DE0-5E58D4DBB9D9") },
            { Normalize("camber_rear_right"),   Guid.Parse("C6241905-C753-48C9-9DE0-5E58D4DBB9D9") },
            { Normalize("toe_rear_total"),      Guid.Parse("AB546991-BE46-4CB9-BDF0-1A2EEF649AE1") },

            // Дополнительные алиасы (синонимы) для удобства ввода
            { Normalize("front_camber_left"),   Guid.Parse("918580AB-6531-435A-B63B-9929F4E06173") },
            { Normalize("front_camber_right"),  Guid.Parse("12A0C794-2052-42CF-9308-6A3229900B8C") },
            { Normalize("front_caster_left"),   Guid.Parse("29301B8E-EE21-4F0E-A515-10A52B736DE5") },
            { Normalize("front_caster_right"),  Guid.Parse("7440FE93-1D1A-4770-9E0C-1D8C5A7BA939") },
            { Normalize("front_toe_total"),     Guid.Parse("2BC86976-B40D-4659-84D0-18986E7AF9D2") },
            { Normalize("front_toe_left"),      Guid.Parse("2BC86976-1AAD-1451-84D0-18986E7AF902") },
            { Normalize("front_toe_right"),     Guid.Parse("DDDD6916-0AAD-1451-84D0-18986E7AF902") },
            { Normalize("front_sai_left"),      Guid.Parse("C55DF4A3-6889-4C68-BB35-C2C8C4CC0979") },
            { Normalize("front_sai_right"),     Guid.Parse("B2A5CF5D-23B2-449A-A9B7-7EB6C85E5C7F") },
            { Normalize("rear_camber_left"),    Guid.Parse("C6241905-C753-48C9-9DE0-5E58D4DBB9D9") },
            { Normalize("rear_camber_right"),   Guid.Parse("C6241905-C753-48C9-9DE0-5E58D4DBB9D9") },
            { Normalize("rear_toe_total"),      Guid.Parse("AB546991-BE46-4CB9-BDF0-1A2EEF649AE1") },
        };

        /// <summary>
        /// Зарегистрировать/переопределить алиас.
        /// </summary>
        public static void Register(string alias, Guid guid)
        {
            if (guid == Guid.Empty) return;
            aliasToGuid[Normalize(alias)] = guid;
        }

        /// <summary>
        /// Попробовать разрешить алиас в Guid. Возвращает true при успехе.
        /// </summary>
        public static bool TryResolve(string aliasOrKey, out Guid guid)
        {
            if (Guid.TryParse(aliasOrKey, out guid) && guid != Guid.Empty)
                return true;

            var norm = Normalize(aliasOrKey);
            return aliasToGuid.TryGetValue(norm, out guid);
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var s = value.Trim().ToLowerInvariant();
            s = s.Replace(" ", string.Empty)
                 .Replace("-", string.Empty)
                 .Replace("_", string.Empty)
                 .Replace(".", string.Empty);
            return s;
        }

        /// <summary>
        /// Набор разрешённых GUIDов для фильтрации (чтобы не брать лишние типы).
        /// </summary>
        public static IReadOnlyCollection<Guid> AllowedGuids => new[]
        {
            // Front Camber L/R
            Guid.Parse("918580AB-6531-435A-B63B-9929F4E06173"),
            Guid.Parse("12A0C794-2052-42CF-9308-6A3229900B8C"),
            // Front Caster L/R
            Guid.Parse("29301B8E-EE21-4F0E-A515-10A52B736DE5"),
            Guid.Parse("7440FE93-1D1A-4770-9E0C-1D8C5A7BA939"),
            // Front Toe total/L/R
            Guid.Parse("2BC86976-B40D-4659-84D0-18986E7AF9D2"),
            Guid.Parse("2BC86976-1AAD-1451-84D0-18986E7AF902"),
            Guid.Parse("DDDD6916-0AAD-1451-84D0-18986E7AF902"),
            // Front SAI L/R
            Guid.Parse("C55DF4A3-6889-4C68-BB35-C2C8C4CC0979"),
            Guid.Parse("B2A5CF5D-23B2-449A-A9B7-7EB6C85E5C7F"),
            // Rear: camber (общий GUID для L/R) и toe total (Uncontrolled)
            Guid.Parse("C6241905-C753-48C9-9DE0-5E58D4DBB9D9"),
            Guid.Parse("AB546991-BE46-4CB9-BDF0-1A2EEF649AE1"),
        };
    }
}

