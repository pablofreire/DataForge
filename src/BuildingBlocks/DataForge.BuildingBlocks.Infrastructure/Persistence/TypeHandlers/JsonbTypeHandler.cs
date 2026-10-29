using System.Data;
using System.Text.Json;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace DataForge.BuildingBlocks.Infrastructure.Persistence.TypeHandlers;

/// <summary>
/// Maps a value object to a PostgreSQL <c>jsonb</c> column with System.Text.Json (camelCase, nulls omitted).
/// </summary>
public sealed class JsonbTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        parameter.Value = value is null ? DBNull.Value : JsonSerializer.Serialize(value, SerializerOptions);
        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
    }

    public override T? Parse(object value) => value switch
    {
        string json => JsonSerializer.Deserialize<T>(json, SerializerOptions),
        _ => throw new DataException($"Cannot convert {value.GetType().Name} to {typeof(T).Name}."),
    };
}
