using Npgsql;
using Respawn;
using System.Data.Common;

namespace WebApp.Tests.Integration;

internal class DbRespawner : IDisposable
{
	private readonly DbConnection _connection;
	private readonly Respawner _respawner;

	protected DbRespawner(Respawner respawner, DbConnection connection)
	{
		_respawner = respawner;
		_connection = connection;
	}

	public void Dispose()
	{
		_connection.Dispose();
		GC.SuppressFinalize(this);
	}

	public static async Task<DbRespawner> CreateAsync(string connectionString)
	{
		DbConnection connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		Respawner respawner = await Respawner.CreateAsync(connection,
			new RespawnerOptions { SchemasToInclude = ["public"], DbAdapter = DbAdapter.Postgres });
		return new DbRespawner(respawner, connection);
	}

	public async Task ResetDatabaseAsync()
	{
		await _respawner.ResetAsync(_connection);
	}
}