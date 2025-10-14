using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json"); // Load cấu hình Ocelot
builder.Services.AddOcelot();


var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});



var app = builder.Build();

app.UseRouting();
app.UseOcelot().Wait(); // Sử dụng Ocelot Middleware

app.Run();