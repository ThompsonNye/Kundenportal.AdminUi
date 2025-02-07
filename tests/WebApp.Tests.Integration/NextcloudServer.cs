using System.Net;
using System.Text;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WebApp.Tests.Integration;

public class NextcloudServer
{
	private readonly WireMockServer _server;

	public const string Username = "testuser";
	public const string Password = "testpassword";

	public string Url => _server.Url!;

	public NextcloudServer()
	{
		_server = WireMockServer.Start();
	}

	/// <summary>
	/// Sets up Nextcloud to not find any resources at the given path.
	/// </summary>
	/// <param name="username">The user for the request</param>
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
			.WithHeader("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}")
			.WithStatusCode(HttpStatusCode.NotFound);

		_server
			.Given(request)
			.RespondWith(response);
	}

	/// <summary>
	/// Sets up Nextcloud to create a folder at the given path.
	/// </summary>
	/// <param name="username"></param>
	/// <param name="path"></param>
	public void SetupCreateFolder(string username, string password, string path)
	{
		_server
		.Given(Request.Create()
			// The WebDav client makes requests with a leading slash!
			.WithPath($"/remote.php/dav/files/{username}{path}")
			.UsingMethod("MKCOL")
			.WithHeader("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}"))
		.RespondWith(Response.Create()
			.WithStatusCode(HttpStatusCode.Created));
	}

}
