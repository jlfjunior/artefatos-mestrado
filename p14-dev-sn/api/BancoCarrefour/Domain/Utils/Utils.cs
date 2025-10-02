using System.Linq.Expressions;

namespace Domain.Utils
{
    public static class Utils
    {
        public static Expression<Func<T, object>> OrderbyToLambda<T>(string orderBy) where T : class
        {
            var spiltedOrder = orderBy.Split(" ");
            var propertyName = String.IsNullOrEmpty(spiltedOrder[0]) ? "Id" : spiltedOrder[0];

            var parameter = Expression.Parameter(typeof(T), "ob");
            var property = Expression.Property(parameter, propertyName);

            var convert = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(convert, parameter);
        }

        public static bool OrderbyToDescending(string orderBy)
        {
            var spiltedOrder = orderBy.Split(" ");
            if (String.IsNullOrEmpty(spiltedOrder[0]))
                return false;
            if (spiltedOrder[1] == "desc")
                return true;

            return false;
        }
    }
}
