using Microsoft.Extensions.Hosting;
using SiteMonitor.Models;
using SiteMonitor.Serve;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
//ע���̨����
builder.Services.AddHostedService<SiteBackgroundService>();

// ӳ��������Ϣ�� AppConfig ����
builder.Services.Configure<MonitorConfig>(builder.Configuration.GetSection("MonitorConfig"));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
