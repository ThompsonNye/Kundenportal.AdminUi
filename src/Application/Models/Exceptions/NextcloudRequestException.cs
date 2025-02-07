namespace Kundenportal.AdminUi.Application.Models.Exceptions;

public sealed class NextcloudRequestException
	: ApplicationException
{
	public NextcloudRequestException(Exception innerException)
		: base($"Nextcloud request failed due to a {innerException.GetType().Name}", innerException)
	{
	}

	public NextcloudRequestException(int statusCode)
		: base($"Nextcloud request failed with status code {statusCode}")
	{
		StatusCode = statusCode;
	}

	public int? StatusCode { get; }
}
