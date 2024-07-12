using Polly;
using Polly.Registry;
using Polly.Retry;
using System.Text.Json;

namespace ResilienceDemoApp
{
    public class TodoService
    {
        private readonly ResiliencePipelineProvider<string> _provider;
        private readonly HttpClient _client;

        public TodoService(ResiliencePipelineProvider<string> provider, HttpClient client)
        {
            _provider = provider;
            _client = client;
        }

        public async Task<Todo> GetTodoById(int id)
        {
            var client = new HttpClient();

            var pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    MaxRetryAttempts = 2,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    OnRetry = (arg) =>
                    {
                        Console.WriteLine($"Retry # {arg.AttemptNumber}");
                        return ValueTask.CompletedTask;
                    }
                }).AddTimeout(TimeSpan.FromSeconds(10))
                .Build();

            var result = await pipeline.ExecuteAsync(async ct =>
            {
                return await client.GetAsync($"https://jsonplaceholder.typicode.com/todos/{id}");
            });

            var content = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Todo>(content);
        }
        public async Task<Todo> GetTodoByIdV2(int id)
        {
            var client = new HttpClient();

            var pipeline = _provider.GetPipeline("default");

            var result = await pipeline.ExecuteAsync(async ct =>
            {
                return await client.GetAsync($"https://jsonplaceholder.typicode.com/todos/{id}");
            });

            var content = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Todo>(content);
        }

        public async Task<Todo> GetTodoByIdV3(int id)
        {

            var result = await _client.GetAsync($"https://jsonplaceholder.typicode.com/todos/{id}");

            var content = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Todo>(content);
        }
    }
}
