using DataForge.BuildingBlocks.Infrastructure.Persistence.Interfaces;
using Npgsql;

namespace DataForge.BuildingBlocks.Infrastructure.Persistence.Connections;

public sealed class NpgsqlConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken) =>
        dataSource.OpenConnectionAsync(cancellationToken);
}
