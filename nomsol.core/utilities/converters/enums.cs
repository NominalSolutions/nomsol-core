using System;
using System.ComponentModel;
using System.Reflection;

namespace nomsol.core.utilities.converters
{
    public static class enums
    {
        /// <summary>
        /// Retrieves the description attribute of an enum value, if available.
        /// </summary>
        /// <param name="value">The enum value to inspect.</param>
        /// <returns>The description string, or null if not found.</returns>
        public static string? GetDescription(this Enum value)
        {
            if (value == null)
                return null;

            string? name = Enum.GetName(value.GetType(), value);
            if (name == null)
                return null;

            FieldInfo? field = value.GetType().GetField(name);
            return field?.GetCustomAttribute<DescriptionAttribute>(false)?.Description;
        }
    }
}
