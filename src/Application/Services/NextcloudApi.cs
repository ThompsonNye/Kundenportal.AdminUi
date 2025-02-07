using System.Net.Sockets;
using Kundenportal.AdminUi.Application.Models.Exceptions;
using Kundenportal.AdminUi.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using WebDav;

namespace Kundenportal.AdminUi.Application.Services;

public interface INextcloudApi
{
	Task<bool> DoesFolderExistAsync(string path, CancellationToken cancellationToken = default);

	Task CreateFolderAsync(string path, CancellationToken cancellationToken = default);

	Task DeleteFolderAsync(string path, CancellationToken cancellationToken = default);
}

public sealed class NextcloudApi(
	IWebDavClient webDavClient,
	IOptions<NextcloudOptions> nextcloudOptions,
	ILogger<NextcloudApi> logger) : INextcloudApi
{
	private readonly ILogger<NextcloudApi> _logger = logger;
	private readonly IOptions<NextcloudOptions> _nextcloudOptions = nextcloudOptions;
	private readonly IWebDavClient _webDavClient = webDavClient;

	public async Task<bool> DoesFolderExistAsync(string path, CancellationToken cancellationToken = default)
	{
		PropfindParameters parameters = new()
		{
			CancellationToken = cancellationToken,
			Headers = new Dictionary<string, string>
			{
				["Depth"] = "0"
			}
		};

		PropfindResponse response;

		try
		{
			response = await _webDavClient.Propfind($"remote.php/dav/files/admin{path}", parameters);
		}
		catch (Exception ex)
			when (ex is TimeoutRejectedException or HttpRequestException or SocketException)
		{
			_logger.LogError(ex, "Failed to reach Nextcloud");
			throw new NextcloudRequestException(ex);
		}

		if (!response.IsSuccessful && response.StatusCode != 404)
			throw new NextcloudRequestException(response.StatusCode);

		return response.Resources
			.Any(x => x.IsCollection);
	}

	public async Task CreateFolderAsync(string path, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(path);

		MkColParameters parameters = new()
		{
			CancellationToken = cancellationToken
		};
		var response = await _webDavClient.Mkcol($"remote.php/dav/files/{_nextcloudOptions.Value.Username}{path}",
			parameters);

		if (response.IsSuccessful) return;

		if (response.StatusCode == 405) throw new NextcloudFolderExistsException(path);

		throw new NextcloudRequestException(response.StatusCode);
	}

	public async Task DeleteFolderAsync(string path, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(path);

		DeleteParameters parameters = new()
		{
			CancellationToken = cancellationToken
		};
		var response = await _webDavClient.Delete($"remote.php/dav/files/{_nextcloudOptions.Value.Username}{path}",
			parameters);

		if (response.IsSuccessful || response.StatusCode == 404) return;

		throw new NextcloudRequestException(response.StatusCode);
	}
}
