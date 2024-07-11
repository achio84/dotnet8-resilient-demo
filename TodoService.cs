using System.Text.Json;

namespace ResilienceDemoApp
{
    public class TodoService
    {
        public TodoService() { }

        public async Task<Todo> GetTodoById(int id)
        {
            var client = new HttpClient();

            var result = await client.GetAsync($"https://jsonplaceholder.typicode.com/todos/{id}");

            var content = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Todo>(content);
        }
    }
}
