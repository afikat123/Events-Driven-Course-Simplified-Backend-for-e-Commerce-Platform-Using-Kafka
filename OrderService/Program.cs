using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<KafkaConsumer>();
builder.Services.AddControllers(); 
builder.Logging.SetMinimumLevel(LogLevel.Warning);

var app = builder.Build();

app.Urls.Add("http://0.0.0.0:5001");


var kafkaConsumer = app.Services.GetRequiredService<KafkaConsumer>();
Task.Run(kafkaConsumer.startConsume);
// _ = kafkaConsumer.startConsume();
app.MapControllers();
app.Run();