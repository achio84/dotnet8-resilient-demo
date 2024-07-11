using Microsoft.AspNetCore.Mvc;

namespace ResilienceDemoApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoService _todoService;
        private readonly ILogger<TodoController> _logger;

        public TodoController(ILogger<TodoController> logger, TodoService todoService)
        {
            _logger = logger;
            _todoService = todoService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var todo = await _todoService.GetTodoById(id);
            return Ok(todo);
        }
    }
}
