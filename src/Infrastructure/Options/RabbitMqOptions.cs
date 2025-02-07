namespace Kundenportal.AdminUi.Infrastructure.Options;

/// <summary>
/// The options for the RabbitMq connection.
/// </summary>
public class RabbitMqOptions
{
    /// <summary>
    /// The configuration section name for the RabbitMq options.
    /// </summary>
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// The host of the RabbitMq instance.
    /// </summary>
    public string Host { get; set; } = "localhost";
    
    /// <summary>
    /// The port of the RabbitMq instance.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// The virtual host to use in the RabbitMq instance.
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// The username for the RabbitMq instance.
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// The password for the RabbitMq instance.
    /// </summary>
    public string Password { get; set; } = "guest";
}
