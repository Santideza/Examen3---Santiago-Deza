using Microsoft.AspNetCore.Mvc;
using TareasAPI.Services;

namespace TareasAPI.Controllers;

[ApiController]
[Route("api/ml")]
public class MlController : ControllerBase
{
    private readonly SentimientoService _sentimientoService;

    public MlController(SentimientoService sentimientoService)
    {
        _sentimientoService = sentimientoService;
    }

    [HttpPost("sentimiento")]
    public ActionResult<SentimientoResponse> AnalizarSentimiento(SentimientoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Comentario))
            return BadRequest("El comentario es obligatorio");

        return Ok(new SentimientoResponse
        {
            Comentario = request.Comentario,
            Sentimiento = _sentimientoService.Analizar(request.Comentario)
        });
    }
}

public class SentimientoRequest
{
    public string Comentario { get; set; } = string.Empty;
}

public class SentimientoResponse
{
    public string Comentario { get; set; } = string.Empty;
    public string Sentimiento { get; set; } = string.Empty;
}
