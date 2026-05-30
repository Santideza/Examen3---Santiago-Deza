namespace TareasAPI.Models;

public class Tarea
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente";
    public string Prioridad { get; set; } = "Media";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaVencimiento { get; set; }
}
