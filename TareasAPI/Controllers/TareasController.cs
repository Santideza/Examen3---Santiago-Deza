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

    public TareasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TareaDto>>> GetTareas()
    {
        var tareas = await _context.Tareas.ToListAsync();
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
    public async Task<ActionResult<TareaDto>> CreateTarea(CreateTareaRequest request)
    {
        var tarea = new Tarea
        {
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            Completada = false,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Tareas.Add(tarea);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTarea), new { id = tarea.Id }, MapToDto(tarea));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTarea(int id, UpdateTareaRequest request)
    {
        var tarea = await _context.Tareas.FindAsync(id);
        if (tarea == null)
            return NotFound();

        tarea.Titulo = request.Titulo ?? tarea.Titulo;
        tarea.Descripcion = request.Descripcion ?? tarea.Descripcion;
        tarea.Completada = request.Completada ?? tarea.Completada;

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
        Completada = tarea.Completada,
        FechaCreacion = tarea.FechaCreacion
    };
}

public class CreateTareaRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class UpdateTareaRequest
{
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public bool? Completada { get; set; }
}
