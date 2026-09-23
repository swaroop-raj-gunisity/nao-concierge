using NaoConcierge.Agent;
using NaoConcierge.Application.UseCases;
using NaoConcierge.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAgentServices(builder.Configuration);

builder.Services.AddScoped<StartIntakeUseCase>();
builder.Services.AddScoped<ProcessMessageUseCase>();
builder.Services.AddScoped<ConfirmAttributeUseCase>();
builder.Services.AddScoped<GetCaseStatusUseCase>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
