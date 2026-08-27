using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Infrastructure;
using Makara.Server.NodeHandlers;
using Makara.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Web API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 兼容 Enum 按字符串返回（对前端更友好）
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Infrastructure (FreeSql + SQLite)
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=makara.db";
builder.Services.AddMakaraInfrastructure(connectionString);

// SSE Progress Hub (singleton: shared across requests)
builder.Services.AddSingleton<IProgressHub, ProgressHub>();

// Node Handlers
builder.Services.AddScoped<DataSourceNodeHandler>();
builder.Services.AddScoped<DataCleanNodeHandler>();
builder.Services.AddScoped<FieldMapNodeHandler>();
builder.Services.AddScoped<DatasetBuildNodeHandler>();
builder.Services.AddScoped<WorkflowNodeHandlerFactory>();

// Services (original)
builder.Services.AddScoped<IDataSourceService, DataSourceService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IEtlService, EtlService>();
builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine>();

// Services (new)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDatasetService, DatasetService>();
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();

// Scheduler (singleton + hosted service)
builder.Services.AddSingleton<WorkflowSchedulerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WorkflowSchedulerService>());

var app = builder.Build();

// ===== Seed demo data on first start (幂等) =====
using (var scope = app.Services.CreateScope())
{
    var fsql = scope.ServiceProvider.GetRequiredService<IFreeSql>();
    await SeedDataService.EnsureSeedAsync(fsql);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
