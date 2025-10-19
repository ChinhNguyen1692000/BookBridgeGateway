using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Đăng ký Ocelot, Swagger, SwaggerForOcelot
builder.Services.AddOcelot();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// CORS đăng ký trước khi Build
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd", policy =>
    {
        policy.WithOrigins("https://e00d56ad.bookbridge-5ju.pages.dev")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseRouting();

// ✅ Dùng CORS trước khi Ocelot
app.UseCors("AllowFrontEnd");

// Swagger UI
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// Ocelot middleware
await app.UseOcelot();

app.Run();
