using Polly;
using Polly.Retry;
using ResilienceDemoApp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<TodoService>();
builder.Services.AddHttpClient<TodoService>().AddResilienceHandler("jong", x =>
{
    x.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        ShouldHandle = (arg) =>
        {
            var isError = arg.Outcome.Exception is Exception ex;
            return ValueTask.FromResult(isError);
        },
        MaxRetryAttempts = 2,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        OnRetry = (arg) =>
        {
            Console.WriteLine($"Retry from http client # {arg.AttemptNumber}");
            return ValueTask.CompletedTask;
        }
    }).AddTimeout(TimeSpan.FromSeconds(10));
});

builder.Services.AddResiliencePipeline("default", x =>
{
    x.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        MaxRetryAttempts = 2,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        OnRetry = (arg) =>
        {
            Console.WriteLine($"Retry from Program file # {arg.AttemptNumber}");
            return ValueTask.CompletedTask;
        }
    }).AddTimeout(TimeSpan.FromSeconds(10));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
