using AspNet.Library.Protos;
using AspNet.GrpcApis.Services;
using Grpc.Core;
using NSubstitute;

namespace AspNet.GrpcApis.Tests
{
    public class WeatherServiceV1UnitTests
    {
        [Fact]
        public async Task GetWeatherForecastReturnsWeatherForecastResponse()
        {
            // Arrange
            var service = new WeatherServiceV1();
            var serverCallContext = Substitute.For<ServerCallContext>();

            // Act
            var result = await service.GetWeatherForecast(new WeatherForecastRequest(), serverCallContext);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Forecasts.Count > 0, "The collection count should be greater than 0.");
        }
    }
}
