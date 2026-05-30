using Microsoft.AspNetCore.Mvc;
using TareasAPI.Data;
using TareasAPI.DTOs;
using TareasAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TareasAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TareasController : ControllerBase
{
    private readonly AppDbContext _context;
    private static readonly string[] EstadosValidos = ["Pendiente", "EnProceso", "Completada"];
    private static readonly string[] PrioridadesValidas = ["Baja", "Media", "Alta"];

    public TareasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TareaDto>>> GetTareas(
        string? estado,
        string? prioridad,
        DateTime? fechaInicio,
        DateTime? fechaFin)
    {
        if (!string.IsNullOrWhiteSpace(estado) && !EstadosValidos.Contains(estado))
            return BadRequest("Estado invalido. Use: Pendiente, EnProceso o Completada");

        if (!string.IsNullOrWhiteSpace(prioridad) && !PrioridadesValidas.Contains(prioridad))
            return BadRequest("Prioridad invalida. Use: Baja, Media o Alta");

        if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
            return BadRequest("fechaInicio no puede ser mayor que fechaFin");

        var consulta = _context.Tareas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(t => t.Estado == estado);

        if (!string.IsNullOrWhiteSpace(prioridad))
            consulta = consulta.Where(t => t.Prioridad == prioridad);

        if (fechaInicio.HasValue)
            consulta = consulta.Where(t => t.FechaVencimiento.Date >= fechaInicio.Value.Date);

        if (fechaFin.HasValue)
            consulta = consulta.Where(t => t.FechaVencimiento.Date <= fechaFin.Value.Date);

        var tareas = await consulta.ToListAsync();
        return Ok(tareas.Select(t => MapToDto(t)).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TareaDto>> GetTarea(int id)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea == null)
            return NotFound();
        return Ok(MapToDto(tarea));
    }

    [HttpPost]
    public async Task<ActionResult<TareaDto>> CreateTarea(TareaRequest request)
    {
        var error = Validar(request);
        if (error != null)
            return BadRequest(error);

        var tarea = new Tarea
        {
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            Estado = request.Estado,
            Prioridad = request.Prioridad,
            FechaCreacion = DateTime.UtcNow,
            FechaVencimiento = request.FechaVencimiento
        };

        _context.Tareas.Add(tarea);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTarea), new { id = tarea.Id }, MapToDto(tarea));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTarea(int id, TareaRequest request)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea == null)
            return NotFound();

        var error = Validar(request);
        if (error != null)
            return BadRequest(error);

        tarea.Titulo = request.Titulo;
        tarea.Descripcion = request.Descripcion;
        tarea.Estado = request.Estado;
        tarea.Prioridad = request.Prioridad;
        tarea.FechaVencimiento = request.FechaVencimiento;

        _context.Tareas.Update(tarea);
        await _context.SaveChangesAsync();

        return Ok(MapToDto(tarea));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTarea(int id)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea == null)
            return NotFound();

        _context.Tareas.Remove(tarea);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private TareaDto MapToDto(Tarea tarea) => new()
    {
        Id = tarea.Id,
        Titulo = tarea.Titulo,
        Descripcion = tarea.Descripcion,
        Estado = tarea.Estado,
        Prioridad = tarea.Prioridad,
        FechaCreacion = tarea.FechaCreacion,
        FechaVencimiento = tarea.FechaVencimiento
    };

    private static string? Validar(TareaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return "El titulo es obligatorio";

        if (!EstadosValidos.Contains(request.Estado))
            return "Estado invalido. Use: Pendiente, EnProceso o Completada";

        if (!PrioridadesValidas.Contains(request.Prioridad))
            return "Prioridad invalida. Use: Baja, Media o Alta";

        if (request.FechaVencimiento.Date < DateTime.Today)
            return "La fecha de vencimiento no puede ser pasada";

        return null;
    }
}

public class TareaRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
}
