using AspNet.GrpcApis.Services;
using Microsoft.OpenApi.Models;

namespace AspNet.GrpcApis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc().AddJsonTranscoding();
            builder.Services.AddGrpcSwagger();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo { Title = "gRPC transcoding", Version = "v1" });
            });
            builder.Services.AddGrpcReflection();

            builder.Services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            }));

            var app = builder.Build();

            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
            app.UseCors();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<WeatherServiceV1>().EnableGrpcWeb().RequireCors("AllowAll");

            if (app.Environment.IsDevelopment())
            {
                app.MapGrpcReflectionService();
            }

            app.MapGet("/", () => "This gRPC service is gRPC-Web enabled and is callable from browser apps using the gRPC-Web protocol");

            app.Run();
        }
    }
}