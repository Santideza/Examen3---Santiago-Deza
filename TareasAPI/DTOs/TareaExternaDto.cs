namespace TareasAPI.DTOs;

public class TareaExternaDto
{
    public int ExternalId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public bool Completado { get; set; }
}
