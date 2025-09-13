using System;
using System.ComponentModel;

namespace Domain.Core.Helper;

public static class EnumHelper
{
    public static string GetEnumDescription(this Enum enumValue)
    {
        var field = enumValue?.GetType().GetField(enumValue.ToString());
        if (field == null)
            return string.Empty;

        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
        {
            return attribute.Description;
        }

        return string.Empty;
    }
}
