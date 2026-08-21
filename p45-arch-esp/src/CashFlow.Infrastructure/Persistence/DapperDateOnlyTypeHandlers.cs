using System.Data;
using Dapper;

namespace CashFlow.Infrastructure.Persistence;

internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) =>
        value switch
        {
            DateOnly date => date,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => DateOnly.Parse(value.ToString()!)
        };

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }
}

internal sealed class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value) =>
        value is null or DBNull
            ? null
            : value switch
            {
                DateOnly date => date,
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                _ => DateOnly.Parse(value.ToString()!)
            };

    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.HasValue ? value.Value : DBNull.Value;
    }
}
