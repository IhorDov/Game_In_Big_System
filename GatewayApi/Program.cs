//using GatewayApi.Extensions;
//using Ocelot.DependencyInjection;
//using Ocelot.Middleware;


//var builder = WebApplication.CreateBuilder(args);

//builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
//builder.Services.AddOcelot(builder.Configuration);

//builder.Services.AddControllers();

//// Add CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsPolicy", builder =>
//        builder.AllowAnyOrigin()
//               .AllowAnyMethod()
//               .AllowAnyHeader());
//});

//var app = builder.Build();

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();

//await app.UseOcelot();

//// Use CORS
//app.UseCors("CorsPolicy");

//app.Run();

////builder.AddAppAuthentication();

////// Configure Kestrel to listen on specific ports
////builder.WebHost.ConfigureKestrel(options =>
////{
////    options.ListenAnyIP(80); // HTTP port
////    options.ListenAnyIP(443, listenOptions =>
////    {
////        listenOptions.UseHttps("/app/certificates/aspnetapp.pfx", "1979password"); // Correct path in container
////    }); // HTTPS port
////});

////if (builder.Environment.EnvironmentName.ToString().ToLower().Equals("production"))
////{
////    builder.Configuration.AddJsonFile("ocelot.Production.json", optional: false, reloadOnChange: true);
////}
////else
////{
////    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
////}

////builder.Services.AddOcelot(builder.Configuration);

////var app = builder.Build();

////app.MapGet("/", () => "Hello World!");

////app.UseOcelot().GetAwaiter().GetResult();

////app.Run();
