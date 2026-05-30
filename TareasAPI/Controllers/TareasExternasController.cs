using Microsoft.AspNetCore.Mvc;
using TareasAPI.DTOs;

namespace TareasAPI.Controllers;

[ApiController]
[Route("api/tareas-externas")]
public class TareasExternasController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TareasExternasController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<TareaExternaDto>>> GetTareasExternas()
    {
        var tareas = await ObtenerTareasExternas();
        if (tareas == null)
            return StatusCode(502, "No se pudo obtener la informacion de la API externa");

        return Ok(tareas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TareaExternaDto>> GetTareaExterna(int id)
    {
        var tareas = await ObtenerTareasExternas();
        if (tareas == null)
            return StatusCode(502, "No se pudo obtener la informacion de la API externa");

        var tarea = tareas.FirstOrDefault(t => t.ExternalId == id);
        if (tarea == null)
            return NotFound();

        return Ok(tarea);
    }

    private async Task<List<TareaExternaDto>?> ObtenerTareasExternas()
    {
        try
        {
            var cliente = _httpClientFactory.CreateClient();
            var tareas = await cliente.GetFromJsonAsync<List<JsonPlaceholderTodo>>(
                "https://jsonplaceholder.typicode.com/todos");

            return tareas?.Select(t => new TareaExternaDto
            {
                ExternalId = t.Id,
                Titulo = t.Title,
                Completado = t.Completed
            }).ToList();
        }
        catch
        {
            return null;
        }
    }
}

public class JsonPlaceholderTodo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Completed { get; set; }
}
