using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Infrastructure;
using Makara.Server.NodeHandlers;
using Makara.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Web API
builder.Services.AddControllers();
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

// Services
builder.Services.AddScoped<IDataSourceService, DataSourceService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IEtlService, EtlService>();
builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine>();

// Scheduler (singleton + hosted service)
builder.Services.AddSingleton<WorkflowSchedulerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WorkflowSchedulerService>());

var app = builder.Build();

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
