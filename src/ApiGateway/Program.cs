using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Bật Endpoint API Explorer để hỗ trợ MapGet
builder.Services.AddEndpointsApiExplorer();

// Đăng ký Health Checks
builder.Services.AddHealthChecks(); 

// Đăng ký Ocelot, Swagger, SwaggerForOcelot
builder.Services.AddOcelot();
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
        policy.WithOrigins("https://e00d56ad.bookbridge-5ju.pages.dev",
            "http://localhost:5173")
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

// Sử dụng Development Exception Page
app.UseDeveloperExceptionPage();

// 1. Cấu hình Health check endpoint (Render dùng để kiểm tra /healthz)
// Điều này sẽ xử lý các yêu cầu tới /healthz mà KHÔNG cần qua Ocelot
app.UseRouting();
app.UseCors("AllowFrontEnd");

// Map Health Check và Root URL endpoint
app.UseEndpoints(endpoints =>
{
    // Cấu hình để xử lý /healthz (match với cấu hình Render và route trong ocelot.json)
    endpoints.MapHealthChecks("/healthz"); 
    // Xử lý root URL
    endpoints.MapGet("/", () => Results.Ok("API Gateway is running. Use /swagger/docs to see available documentation."));
});


// 2. SwaggerForOcelot UI
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// 3. Cuối cùng — Ocelot gateway middleware
await app.UseOcelot();

app.Run();