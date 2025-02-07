using Kundenportal.AdminUi.Application.Options;
using Microsoft.Extensions.Options;
using WebDav;

namespace Kundenportal.AdminUi.Application.Services;

public interface INextcloudApi
{
    Task<NextcloudFolder> GetFolderDetailsAsync(string path, CancellationToken cancellationToken = default);

    Task CreateFolderAsync(string path, CancellationToken cancellationToken = default);
}

public sealed class NextcloudApi(
    IWebDavClient webDavClient,
    IOptions<NextcloudOptions> nextcloudOptions) : INextcloudApi
{
    private readonly IWebDavClient _webDavClient = webDavClient;
    private readonly IOptions<NextcloudOptions> _nextcloudOptions = nextcloudOptions;

    public async Task<NextcloudFolder> GetFolderDetailsAsync(string path, CancellationToken cancellationToken = default)
    {
        var parameters = new PropfindParameters
        {
            CancellationToken = cancellationToken,
            Headers = new Dictionary<string, string>()
            {
                ["Depth"] = "0"
            }
        };

        PropfindResponse response = await _webDavClient.Propfind($"remote.php/dav/files/admin{path}", parameters);

        if (!response.IsSuccessful)
        {
            // TODO Handle gracefully
            throw new Exception();
        }

        var nextcloudFolder = response
            .Resources
            // TODO Improve
            // .Where(x => x.Uri.TrimEnd().TrimEnd('/').EndsWith($"{path.TrimEnd().TrimEnd('/')}"))
            .Select(resource => new NextcloudFolder(
                resource.Uri,
                // TODO Improve
                resource.Uri.TrimStart().TrimStart('/').Replace("remote.php/dav/files/admin", ""),
                resource.CreationDate,
                resource.LastModifiedDate,
                resource.IsCollection))
            .FirstOrDefault();

        if (nextcloudFolder is null)
        {
            // TODO Handle gracefully
            throw new Exception();
        }

        return nextcloudFolder;
    }

    public async Task CreateFolderAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        MkColParameters parameters = new()
        {
            CancellationToken = cancellationToken
        };
        WebDavResponse response = await _webDavClient.Mkcol($"remote.php/dav/files/{_nextcloudOptions.Value.Username}{path}", parameters);

        if (response.IsSuccessful)
        {
            return;
        }
        
        // TODO Handle better
        throw new Exception();
    }
}

public record NextcloudFolder(
    string Uri,
    string RelativePath,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModifiedDate,
    bool IsFolder);
