using FluentValidation;

namespace Kundenportal.AdminUi.Infrastructure.Options;

/// <summary>
///     The options for the RabbitMq connection.
/// </summary>
public class RabbitMqOptions
{
	/// <summary>
	///     The configuration section name for the RabbitMq options.
	/// </summary>
	public const string SectionName = "RabbitMq";

	/// <summary>
	///     The host of the RabbitMq instance.
	/// </summary>
	public string Host { get; set; } = "localhost";

	/// <summary>
	///     The port of the RabbitMq instance.
	/// </summary>
	public int Port { get; set; } = 5672;

	/// <summary>
	///     The virtual host to use in the RabbitMq instance.
	/// </summary>
	public string VirtualHost { get; set; } = "/";

	/// <summary>
	///     The username for the RabbitMq instance.
	/// </summary>
	public string Username { get; set; } = "guest";

	/// <summary>
	///     The password for the RabbitMq instance.
	/// </summary>
	public string Password { get; set; } = "guest";

	/// <summary>
	///     Constructs a RabbitMq uri for connection to the configured RabbitMq instance.
	/// </summary>
	/// <returns></returns>
	public Uri GetUri()
	{
		return new Uri($"rabbitmq://{Host}:{Port}");
	}
}

public sealed class RabbitMqOptionsValidator : AbstractValidator<RabbitMqOptions>
{
	public const string ErrorCodeMissingLeadingSlash = "MissingLeadingSlash";
	public const string ErrorCodeInvalidUri = "ErrorCodeInvalidUri";

	public RabbitMqOptionsValidator()
	{
		RuleFor(x => x.Host)
			.NotEmpty();

		RuleFor(x => x.Port)
			.GreaterThan(0)
			.LessThan((int)Math.Pow(2, 16));

		RuleFor(x => x.VirtualHost)
			.NotEmpty()
			.Must(virtualHost => virtualHost.StartsWith('/'))
			.WithErrorCode(ErrorCodeMissingLeadingSlash)
			.WithMessage("'{PropertyName}' has to start with a slash");

		RuleFor(x => x.Username)
			.NotEmpty();

		RuleFor(x => x.Password)
			.NotEmpty();

		RuleFor(x => x)
			.Must(ConstructAValidUri)
			.WithErrorCode(ErrorCodeInvalidUri)
			.WithMessage("The configured options have to result in a valid uri");
	}

	private static bool ConstructAValidUri(RabbitMqOptions options)
	{
		try
		{
			options.GetUri();
			return true;
		}
		catch
		{
			return false;
		}
	}
}
