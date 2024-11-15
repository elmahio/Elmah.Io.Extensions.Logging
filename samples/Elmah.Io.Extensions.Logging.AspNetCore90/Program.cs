using Elmah.Io.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Use the following to configure API key and log ID in appsettings.json
//builder.Logging.Services.Configure<ElmahIoProviderOptions>(builder.Configuration.GetSection("ElmahIo"));

// Use the following line to configure elmah.io provider settings like log level from appsettings.json
//builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// If everything is configured in appsettings.json, call the AddElmahIo overload without an options action
//builder.Logging.AddElmahIo();

builder.Logging.AddElmahIo(options =>
{
    options.ApiKey = "API_KEY";
    options.LogId = new Guid("LOG_ID");

    // Optional application name
    options.Application = "ASP.NET Core 9.0 Application";

    // Additional options can be configured like this:
    options.OnMessage = msg =>
    {
        msg.Version = "9.0.0";
    };

    // Remove comment on the following line to log through a proxy (in this case Fiddler).
    //options.WebProxy = new WebProxy("localhost", 8888);
});

// The elmah.io provider can log any log level, but we recommend only to log warning and up
builder.Logging.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
