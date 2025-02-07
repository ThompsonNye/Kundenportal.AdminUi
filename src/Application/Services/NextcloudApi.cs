using WebDav;

namespace Kundenportal.AdminUi.Application.Services;

public interface INextcloudApi
{
    Task<NextcloudFolder> GetFolderDetailsAsync(string path, CancellationToken cancellationToken);
}

public sealed class NextcloudApi : INextcloudApi
{
    public async Task<NextcloudFolder> GetFolderDetailsAsync(string path, CancellationToken cancellationToken)
    {
        using var client = new WebDavClient(new WebDavClientParams()
        {
            BaseAddress = new Uri("http://localhost:50000"),
            DefaultRequestHeaders = new Dictionary<string, string>()
            {
                ["Authorization"] = "Basic YWRtaW46YWRtaW4xKy4="
            }
        });

        var parameters = new PropfindParameters
        {
            CancellationToken = cancellationToken,
            Headers = new Dictionary<string, string>()
            {
                ["Depth"] = "0"
            }
        };

        PropfindResponse response = await client.Propfind($"remote.php/dav/files/admin/{path}", parameters);

        if (!response.IsSuccessful)
        {
            throw new Exception();
        }

        var nextcloudFolder = response
            .Resources
            .Where(x => x.Uri.TrimEnd().TrimEnd('/').EndsWith($"/{path.TrimEnd().TrimEnd('/')}"))
            .Select(resource => new NextcloudFolder(
                resource.Uri,
                resource.Uri.TrimStart().TrimStart('/').Replace("remote.php/dav/files/admin", ""),
                resource.CreationDate,
                resource.LastModifiedDate,
                resource.IsCollection))
            .FirstOrDefault();

        if (nextcloudFolder is null)
        {
            throw new Exception();
        }

        return nextcloudFolder;
    }
}

public record NextcloudFolder(
    string Uri,
    string RelativePath,
    DateTimeOffset? CreationDate,
    DateTimeOffset? LastModifiedDate,
    bool IsFolder);
