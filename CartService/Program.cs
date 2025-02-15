using CartService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddControllers(); 
builder.Logging.SetMinimumLevel(LogLevel.Warning);

var app = builder.Build();

app.Urls.Add("http://0.0.0.0:5002");

var kafkaProducer = app.Services.GetRequiredService<KafkaProducer>();
kafkaProducer.produceTestMessageAndCreateTopic();

app.MapControllers();
app.Run();
Console.WriteLine("Producer Start!");

