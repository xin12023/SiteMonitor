using SiteMonitor.Helpers;
using SiteMonitor.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ӳ��������Ϣ�� AppConfig ����
builder.Services.Configure<MonitorConfig>(builder.Configuration.GetSection("MonitorConfig"));

//ע����־����
builder.Services.AddSingleton<LogHelper>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logPath = config.GetValue<string>("MonitorConfig:LogPath");
    var copiesCount = config.GetValue<int>("MonitorConfig:LogCopies");
    var logLevel = Enum.Parse<LoggerType>(config.GetValue<string>("MonitorConfig:LogLevel"), ignoreCase: true);
    var logFileNameFormat = config.GetValue<string>("MonitorConfig:LogNameFormat");
    return new LogHelper(logPath, copiesCount, logLevel, logFileNameFormat);
});


//ע���̨����
builder.Services.AddHostedService<SiteBackgroundService>();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
