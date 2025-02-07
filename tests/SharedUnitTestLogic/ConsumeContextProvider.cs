using MassTransit;
using NSubstitute;

namespace SharedUnitTestLogic;

public static class ConsumeContextProvider
{
	public static ConsumeContext<T> GetMockedContext<T>(T message)
		where T : class
	{
		ConsumeContext<T>? context = Substitute.For<ConsumeContext<T>>();
		context.Message.Returns(message);
		context.CancellationToken.Returns(CancellationToken.None);
		return context;
	}
}
