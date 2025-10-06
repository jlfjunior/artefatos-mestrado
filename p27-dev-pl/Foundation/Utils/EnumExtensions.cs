namespace Foundation.Utils
{
    public static class EnumExtensions
    {
        public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            if (!Enum.TryParse(value, true, out TEnum enumValue))
                throw new ArgumentException($"Falha ao converter a string '{value}' para o enum {typeof(TEnum).Name}");

            return enumValue;
        }
    }
}
