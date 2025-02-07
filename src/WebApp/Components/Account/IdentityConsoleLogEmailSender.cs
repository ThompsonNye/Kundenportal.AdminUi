using Kundenportal.AdminUi.Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Kundenportal.AdminUi.WebApp.Components.Account;

internal sealed class IdentityConsoleLogEmailSender(IEmailSender emailSender) : IEmailSender<ApplicationUser>
{
	public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
	{
		return emailSender.SendEmailAsync(email, "Confirm your email",
			$"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
	}

	public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
	{
		return emailSender.SendEmailAsync(email, "Reset your password",
			$"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
	}

	public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
	{
		return emailSender.SendEmailAsync(email, "Reset your password",
			$"Please reset your password using the following code: {resetCode}");
	}
}
