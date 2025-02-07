using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Core;

namespace Application.Tests.Unit;

public static partial class LoggerTestExtensions
{
    public static void ReceivedLog<T>(this ILogger<T> logger, LogLevel logLevel, string message,
        params object?[] parameters)
    {
        MatchCollection matches = LoggingParametersRegex().Matches(message);

        if (matches.Count != parameters.Length)
        {
            throw new InvalidOperationException(
                "Found different number of log message parameters than object parameters");
        }

        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            message = message.Replace(match.Value, parameters[i]?.ToString());
        }
        
        logger.Received().Log(
            logLevel,
            Arg.Any<EventId>(),
            Arg.Is<object>(x => x.ToString() == message),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [GeneratedRegex(@"\{([\w\d]+)\}")]
    private static partial Regex LoggingParametersRegex();
}