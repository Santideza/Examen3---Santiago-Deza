using Microsoft.EntityFrameworkCore;
using TareasAPI.Data;
using TareasAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5200");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tareas.db"));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<SentimientoService>();
builder.Services.AddControllers();

var app = builder.Build();

app.Services.GetRequiredService<SentimientoService>();

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
    <title>API Tareas</title>
    <style>
        * { box-sizing: border-box; }
        body { font-family: Arial, sans-serif; max-width: 980px; margin: 32px auto; padding: 0 16px; background: #f6f7f9; color: #222; }
        section { margin-top: 20px; }
        form, .panel, li { background: white; border: 1px solid #ddd; border-radius: 8px; padding: 14px; }
        input, textarea, select, button { width: 100%; margin-top: 8px; padding: 10px; font: inherit; }
        button { cursor: pointer; border: 0; border-radius: 6px; background: #1769e0; color: white; }
        button.secundario { background: #555; }
        button.peligro { background: #b42318; }
        ul { list-style: none; padding: 0; display: grid; gap: 10px; }
        .fila { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
        .fila button { width: auto; }
        .grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; }
        .mensaje { margin-top: 10px; color: #b42318; min-height: 20px; }
        small { color: #666; }
        @media (max-width: 760px) { .grid { grid-template-columns: 1fr; } }
    </style>
</head>
<body>
    <h1>Gestion de tareas</h1>

    <form id="form">
        <input id="id" type="hidden">
        <input id="titulo" placeholder="Titulo" required>
        <textarea id="descripcion" placeholder="Descripcion"></textarea>
        <select id="estado">
            <option value="">Estado</option>
            <option>Pendiente</option>
            <option>EnProceso</option>
            <option>Completada</option>
        </select>
        <select id="prioridad">
            <option value="">Prioridad</option>
            <option>Baja</option>
            <option>Media</option>
            <option>Alta</option>
        </select>
        <input id="fechaVencimiento" type="date" required>
        <div class="fila">
            <button id="guardar">Guardar</button>
            <button type="button" class="secundario" onclick="limpiarFormulario()">Nuevo</button>
        </div>
    </form>

    <section class="panel">
        <h2>Filtros</h2>
        <div class="grid">
            <select id="filtroEstado">
                <option value="">Todos los estados</option>
                <option>Pendiente</option>
                <option>EnProceso</option>
                <option>Completada</option>
            </select>
            <select id="filtroPrioridad">
                <option value="">Todas las prioridades</option>
                <option>Baja</option>
                <option>Media</option>
                <option>Alta</option>
            </select>
            <input id="fechaInicio" type="date">
            <input id="fechaFin" type="date">
        </div>
        <div class="fila">
            <button onclick="cargarTareas()">Buscar</button>
            <button class="secundario" onclick="limpiarFiltros()">Limpiar filtros</button>
        </div>
    </section>

    <p id="mensaje" class="mensaje"></p>

    <section>
        <h2>Tareas locales</h2>
        <ul id="lista"></ul>
    </section>

    <section>
        <div class="fila">
            <h2>Tareas externas</h2>
            <button onclick="cargarExternas()">Cargar externas</button>
        </div>
        <ul id="externas"></ul>
    </section>

    <script>
        const api = "/api/tareas";
        const apiExternas = "/api/tareas-externas";
        const form = document.querySelector("#form");
        const lista = document.querySelector("#lista");
        const externas = document.querySelector("#externas");
        const mensaje = document.querySelector("#mensaje");

        function mostrarError(texto) {
            mensaje.textContent = texto || "";
        }

        function filtrosUrl() {
            const params = new URLSearchParams();
            if (filtroEstado.value) params.append("estado", filtroEstado.value);
            if (filtroPrioridad.value) params.append("prioridad", filtroPrioridad.value);
            if (fechaInicio.value) params.append("fechaInicio", fechaInicio.value);
            if (fechaFin.value) params.append("fechaFin", fechaFin.value);
            return params.toString() ? `${api}?${params}` : api;
        }

        async function cargarTareas() {
            mostrarError("");
            const respuesta = await fetch(filtrosUrl());
            if (!respuesta.ok) {
                mostrarError(await respuesta.text());
                return;
            }
            const tareas = await respuesta.json();
            lista.innerHTML = tareas.map(t => `
                <li>
                    <div class="fila">
                        <strong>${t.titulo}</strong>
                        <button onclick="editar(${t.id})">Editar</button>
                        <button onclick="eliminar(${t.id})">Eliminar</button>
                    </div>
                    <p>${t.descripcion || ""}</p>
                    <small>${t.estado} | ${t.prioridad} | vence: ${t.fechaVencimiento.substring(0, 10)}</small>
                </li>
            `).join("");
        }

        form.addEventListener("submit", async e => {
            e.preventDefault();
            mostrarError("");
            const tarea = {
                titulo: titulo.value,
                descripcion: descripcion.value,
                estado: estado.value,
                prioridad: prioridad.value,
                fechaVencimiento: fechaVencimiento.value
            };
            const url = id.value ? `${api}/${id.value}` : api;
            const metodo = id.value ? "PUT" : "POST";
            const respuesta = await fetch(url, {
                method: metodo,
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(tarea)
            });
            if (!respuesta.ok) {
                mostrarError(await respuesta.text());
                return;
            }
            limpiarFormulario();
            cargarTareas();
        });

        async function editar(tareaId) {
            const respuesta = await fetch(`${api}/${tareaId}`);
            if (!respuesta.ok) {
                mostrarError("No se encontro la tarea");
                return;
            }
            const tarea = await respuesta.json();
            id.value = tarea.id;
            titulo.value = tarea.titulo;
            descripcion.value = tarea.descripcion || "";
            estado.value = tarea.estado;
            prioridad.value = tarea.prioridad;
            fechaVencimiento.value = tarea.fechaVencimiento.substring(0, 10);
            guardar.textContent = "Actualizar";
        }

        function limpiarFormulario() {
            form.reset();
            id.value = "";
            guardar.textContent = "Guardar";
        }

        function limpiarFiltros() {
            filtroEstado.value = "";
            filtroPrioridad.value = "";
            fechaInicio.value = "";
            fechaFin.value = "";
            cargarTareas();
        }

        async function eliminar(id) {
            await fetch(`${api}/${id}`, { method: "DELETE" });
            cargarTareas();
        }

        async function cargarExternas() {
            mostrarError("");
            const respuesta = await fetch(apiExternas);
            if (!respuesta.ok) {
                mostrarError(await respuesta.text());
                return;
            }
            const tareas = await respuesta.json();
            externas.innerHTML = tareas.slice(0, 20).map(t => `
                <li>
                    <strong>${t.externalId}. ${t.titulo}</strong>
                    <br>
                    <small>${t.completado ? "Completado" : "Pendiente"}</small>
                </li>
            `).join("");
        }

        cargarTareas();
    </script>
</body>
</html>
""", "text/html"));

app.MapControllers();

Console.WriteLine("Pagina lista: http://localhost:5200");

app.Run();
