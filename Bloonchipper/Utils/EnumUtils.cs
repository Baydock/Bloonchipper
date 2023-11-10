using System;

namespace Bloonchipper.Utils {
    internal static class EnumUtils {
        /// <summary>
        /// Determines if any of the given flags are in the given enum
        /// </summary>
        /// <param name="from">The enum being tested</param>
        /// <param name="flags">The flags being tested</param>
        /// <returns>True if any of the flags are found, false otherwise</returns>
        public static bool HasAnyFlag(this Enum from, params Enum[] flags) {
            foreach(Enum flag in flags)
                if (from.HasFlag(flag))
                    return true;
            return false;
        }
    }
}
