using Azure.Storage.Blobs;
using Confluent.Kafka;
using Dss.application.Interfaces;
using Dss.Application.Interfaces;
using Dss.Application.Services;
using Dss.Infrastructure.Persistence;
using Kafka.Interfaces;
using Kafka.Producer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Connect to PostgreSQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseNpgsql(connectionString));

// Register Azure Blob Container Client
builder.Services.AddSingleton(x => new BlobContainerClient(builder.Configuration["BlobConnectionString"], builder.Configuration["BlobContainerName"]));

// Add services to the container.
builder.Services.AddTransient<IAzureStorage, AzureStorage>();
builder.Services.AddTransient<IAddDocumentService, AddDocumentService>();


// Configure the producer
builder.Services.Configure<ProducerConfig>(options =>
{
    options.BootstrapServers = builder.Configuration.GetSection("KafkaProducerConfig:bootstrapservers").Value;    
});
var clientConfig = new ClientConfig()
{
    BootstrapServers = builder.Configuration["KafkaProducerConfig:bootstrapservers"],
    SaslUsername = builder.Configuration["KafkaProducerConfig:SaslUsername"],
    SaslPassword = builder.Configuration["KafkaProducerConfig:SaslPassword"],
    SecurityProtocol = SecurityProtocol.SaslSsl,
    SaslMechanism = SaslMechanism.Plain,
    Acks = Acks.All
};
var producerConfig = new ProducerConfig(clientConfig);
builder.Services.AddSingleton(producerConfig);
builder.Services.AddSingleton(typeof(IKafkaProducer<,>), typeof(KafkaProducer<,>));


// Add Serilog
builder.Host.UseSerilog((_, config) =>
{
    config.WriteTo.Console()
    .WriteTo.File("logs/dsslogs.txt", rollingInterval: RollingInterval.Day)
    .ReadFrom.Configuration(builder.Configuration);
});

//set uploadsize large files
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

//set uploadsize large files
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(120);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(120);   
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Shows UseCors with CorsPolicyBuilder.
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

app.UseAuthorization();
app.MapControllers();

app.Run();

