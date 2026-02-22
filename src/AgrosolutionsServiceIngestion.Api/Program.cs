using AgrosolutionsServiceIngestion.Application.Interfaces;
using AgrosolutionsServiceIngestion.Application.UseCase;
using AgrosolutionsServiceIngestion.Application.Validators;
using AgrosolutionsServiceIngestion.Infrastructure.Messaging;
using Amazon.SimpleNotificationService;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<SensorRawRequestValidator>();
builder.Services.AddScoped<PublishSensorRawUseCase>();

//builder.Services.AddScoped<ISensorRawPublisher, RabbitMqSensorRawPublisher>();
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
builder.Services.AddSingleton<ISensorRawPublisher, SnsSensorRawPublisher>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
