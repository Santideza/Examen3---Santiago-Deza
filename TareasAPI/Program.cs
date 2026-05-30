using Microsoft.EntityFrameworkCore;
using TareasAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tareas.db"));

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => Results.Content("""
<!doctype html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Tareas</title>
    <style>
        * { box-sizing: border-box; }
        body { font-family: Arial, sans-serif; max-width: 720px; margin: 40px auto; padding: 0 16px; background: #f6f7f9; color: #222; }
        h1 { margin-bottom: 16px; }
        form, li { background: white; border: 1px solid #ddd; border-radius: 8px; padding: 14px; }
        input, textarea, select, button { width: 100%; margin-top: 8px; padding: 10px; font: inherit; }
        button { cursor: pointer; border: 0; border-radius: 6px; background: #1769e0; color: white; }
        ul { list-style: none; padding: 0; display: grid; gap: 10px; }
        .fila { display: flex; gap: 8px; align-items: center; }
        .fila button { width: auto; }
    </style>
</head>
<body>
    <h1>Mis tareas</h1>
    <form id="form">
        <input id="titulo" placeholder="Titulo" required>
        <textarea id="descripcion" placeholder="Descripcion"></textarea>
        <select id="estado">
            <option>Pendiente</option>
            <option>EnProceso</option>
            <option>Completada</option>
        </select>
        <select id="prioridad">
            <option>Baja</option>
            <option selected>Media</option>
            <option>Alta</option>
        </select>
        <input id="fechaVencimiento" type="date" required>
        <button>Agregar tarea</button>
    </form>
    <ul id="lista"></ul>

    <script>
        const api = "/api/tareas";
        const form = document.querySelector("#form");
        const lista = document.querySelector("#lista");

        async function cargar() {
            const tareas = await fetch(api).then(r => r.json());
            lista.innerHTML = tareas.map(t => `
                <li>
                    <div class="fila">
                        <strong>${t.titulo}</strong>
                        <button onclick="eliminar(${t.id})">Eliminar</button>
                    </div>
                    <p>${t.descripcion || ""}</p>
                    <small>${t.estado} | ${t.prioridad} | vence: ${t.fechaVencimiento.substring(0, 10)}</small>
                </li>
            `).join("");
        }

        form.addEventListener("submit", async e => {
            e.preventDefault();
            await fetch(api, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    titulo: titulo.value,
                    descripcion: descripcion.value,
                    estado: estado.value,
                    prioridad: prioridad.value,
                    fechaVencimiento: fechaVencimiento.value
                })
            });
            form.reset();
            cargar();
        });

        async function eliminar(id) {
            await fetch(`${api}/${id}`, { method: "DELETE" });
            cargar();
        }

        cargar();
    </script>
</body>
</html>
""", "text/html"));

app.UseAuthorization();
app.MapControllers();

app.Run();
