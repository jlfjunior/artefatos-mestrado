using System.ComponentModel;

public class EnumHelper
{
    public static string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute))!;

        return attribute == null ? value.ToString() : attribute.Description;
    }
}