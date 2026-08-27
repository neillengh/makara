using Makara.Core.Interfaces;
using Makara.Core.Models;
using Makara.Infrastructure;
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

// Services
builder.Services.AddScoped<IDataSourceService, DataSourceService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

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
