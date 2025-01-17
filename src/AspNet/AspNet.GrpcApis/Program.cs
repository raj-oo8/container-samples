using AspNet.GrpcApis.Services;

namespace AspNet.GrpcApis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc();

            var app = builder.Build();

            app.UseGrpcWeb();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<WeatherServiceV1>();
            app.MapGet("/", () => "This gRPC service is gRPC-Web enabled and is callable from browser apps using the gRPC-Web protocol");

            app.Run();
        }
    }
}