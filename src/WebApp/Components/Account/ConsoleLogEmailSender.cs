using Microsoft.AspNetCore.Identity.UI.Services;

namespace Kundenportal.AdminUi.WebApp.Components.Account;

internal sealed class ConsoleLogEmailSender(ILogger<ConsoleLogEmailSender> Logger) : IEmailSender
{
	public Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		var body = System.Net.WebUtility.HtmlDecode(htmlMessage);

		Logger.LogInformation("To: {Email}\nSubject: {Subject}\nBody: {Body}", email, subject, body);
		return Task.CompletedTask;
	}
}
