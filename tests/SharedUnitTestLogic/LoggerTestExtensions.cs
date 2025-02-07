using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog.Core;

namespace SharedUnitTestLogic;

public static partial class LoggerTestExtensions
{
	[MessageTemplateFormatMethod(nameof(message))]
	public static void ReceivedLog(this ILogger logger, LogLevel logLevel, [ConstantExpected] string message,
		params object?[] parameters)
	{
		var matches = LoggingParametersRegex().Matches(message);

		if (matches.Count != parameters.Length)
			throw new InvalidOperationException(
				"Found different number of log message parameters than object parameters");

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

	[MessageTemplateFormatMethod(nameof(message))]
	public static void ReceivedLog<TException>(this ILogger logger, LogLevel logLevel,
		[ConstantExpected] string message,
		params object?[] parameters)
		where TException : Exception
	{
		var matches = LoggingParametersRegex().Matches(message);

		if (matches.Count != parameters.Length)
			throw new InvalidOperationException(
				"Found different number of log message parameters than object parameters");

		for (var i = 0; i < matches.Count; i++)
		{
			var match = matches[i];
			message = message.Replace(match.Value, parameters[i]?.ToString());
		}

		logger.Received().Log(
			logLevel,
			Arg.Any<EventId>(),
			Arg.Is<object>(x => x.ToString() == message),
			Arg.Any<TException>(),
			Arg.Any<Func<object, Exception?, string>>());
	}

	[GeneratedRegex(@"\{([\w\d]+)\}")]
	private static partial Regex LoggingParametersRegex();
}
