using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Đăng ký dịch vụ
builder.Services.AddOcelot();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

app.UseRouting();

// Swagger UI cho Ocelot
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";

    // Nếu muốn Swagger nằm ở root (http://localhost:8080/)
    // opt.RoutePrefix = string.Empty;

    // Cấu hình lại upstream Swagger JSON cho bản MMLib v9.x
    opt.ReConfigureUpstreamSwaggerJson = (ctx, swaggerJson) =>
    {
        var swagger = JObject.Parse(swaggerJson);

        // Lấy base URL trong GlobalConfiguration (ocelot.json)
        var baseUrl = app.Configuration["GlobalConfiguration:BaseUrl"] ?? "";

        // Xác định prefix route (gateway/auth, gateway/books, ...)
        // Ở bản 9 không còn ApiEndPoint, nên có thể xác định dựa vào đường dẫn request.
        var path = ctx.Request.Path.ToString().ToLower();
        var upstreamPathPrefix = "/gateway";

        if (path.Contains("auth"))
            upstreamPathPrefix = "/gateway/auth";
        else if (path.Contains("books"))
            upstreamPathPrefix = "/gateway/books";
        else if (path.Contains("bookstores"))
            upstreamPathPrefix = "/gateway/bookstores";
        else if (path.Contains("orders"))
            upstreamPathPrefix = "/gateway/orders";

        var fullUrl = $"{baseUrl}{upstreamPathPrefix}";

        swagger.Remove("servers");
        swagger.Add("servers", new JArray
        {
            new JObject { { "url", fullUrl } }
        });

        return swagger.ToString();
    };
})
.UseOcelot()
.Wait();

app.Run();
