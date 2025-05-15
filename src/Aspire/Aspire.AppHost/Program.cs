var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Aspire_AspNet_Web_Api>("webapi");

builder.AddProject<Projects.Aspire_AspNet_Mvc>("webapp");

builder.Build().Run();
