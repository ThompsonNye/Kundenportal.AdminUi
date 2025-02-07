using System.Net;
using System.Text;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WebApp.Tests.Integration;

public class NextcloudServer
{
	public const string Username = "testuser";
	public const string Password = "testpassword";
	private readonly WireMockServer _server;

	public NextcloudServer()
	{
		_server = WireMockServer.Start();
	}

	public string Url => _server.Url!;

	/// <summary>
	///     Sets up Nextcloud to not find any resources at the given path.
	/// </summary>
	/// <param name="username">The user for the request</param>
	/// <param name="password">The password for the user</param>
	/// <param name="path">The path to look at for resources</param>
	/// <returns></returns>
	public void SetupGetEmptyResources(string username, string password, string path)
	{
		IRequestBuilder request = Request.Create()
			// The WebDav client makes requests with a leading slash!
			.WithPath($"/remote.php/dav/files/{username}{path}")
			.UsingMethod("PROPFIND")
			.WithHeader("Depth", "0");

		IResponseBuilder response = Response.Create()
			.WithBody($"""
			           		<?xml version="1.0" encoding="utf-8"?>
			           		<d:error
			           			xmlns:d="DAV:"
			           			xmlns:s="http://sabredav.org/ns">
			           			<s:exception>Sabre\DAV\Exception\NotFound</s:exception>
			           			<s:message>File with name {path} could not be located</s:message>
			           		</d:error>
			           """)
			.WithHeader("Content-Type", "application/xml; charset=utf-8")
			.WithHeader("Authorization",
				$"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}")
			.WithStatusCode(HttpStatusCode.NotFound);

		_server
			.Given(request)
			.RespondWith(response);
	}

	/// <summary>
	///     Sets up Nextcloud to not find any resources at the given path.
	/// </summary>
	/// <param name="username">The user for the request</param>
	/// <param name="password">The password for the user</param>
	/// <param name="path">The path to look at for resources</param>
	/// <returns></returns>
	public void SetupGetSingleResource(string username, string password, string path)
	{
		IRequestBuilder request = Request.Create()
			// The WebDav client makes requests with a leading slash!
			.WithPath($"/remote.php/dav/files/{username}{path}")
			.UsingMethod("PROPFIND")
			.WithHeader("Depth", "0");

		IResponseBuilder response = Response.Create()
			.WithBody($"""
			           <?xml version="1.0"?>
			           <d:multistatus
			           	xmlns:d="DAV:"
			           	xmlns:s="http://sabredav.org/ns"
			           	xmlns:oc="http://owncloud.org/ns"
			           	xmlns:nc="http://nextcloud.org/ns">
			           	<d:response>
			           		<d:href>/remote.php/dav/files/{username}{path}/</d:href>
			           		<d:propstat>
			           			<d:prop>
			           				<d:getlastmodified>Sun, 14 Apr 2024 14:47:42 GMT</d:getlastmodified>
			           				<d:resourcetype>
			           					<d:collection/>
			           				</d:resourcetype>
			           				<d:quota-used-bytes>0</d:quota-used-bytes>
			           				<d:quota-available-bytes>-3</d:quota-available-bytes>
			           				<d:getetag>&quot;661bec8edc999&quot;</d:getetag>
			           			</d:prop>
			           			<d:status>HTTP/1.1 200 OK</d:status>
			           		</d:propstat>
			           	</d:response>
			           </d:multistatus>

			           """)
			.WithHeader("Content-Type", "application/xml; charset=utf-8")
			.WithHeader("Authorization",
				$"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}")
			.WithStatusCode(HttpStatusCode.MultiStatus);

		_server
			.Given(request)
			.RespondWith(response);
	}

	/// <summary>
	///     Sets up Nextcloud to create a folder at the given path.
	/// </summary>
	/// <param name="username"></param>
	/// <param name="password">The password for the user</param>
	/// <param name="path"></param>
	public void SetupCreateFolder(string username, string password, string path)
	{
		_server
			.Given(Request.Create()
				// The WebDav client makes requests with a leading slash!
				.WithPath($"/remote.php/dav/files/{username}{path}")
				.UsingMethod("MKCOL")
				.WithHeader("Authorization",
					$"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}"))
			.RespondWith(Response.Create()
				.WithStatusCode(HttpStatusCode.Created));
	}
}