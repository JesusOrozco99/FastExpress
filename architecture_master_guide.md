# 🏛️ Guía Unificada de Arquitectura y Desarrollo — FinanceControl & CoreApiRest

> **INSTRUCCIÓN DIRECTA PARA ASISTENTES DE IA (Cursor, Windsurf, GitHub Copilot):**
> Este documento contiene reglas estrictas e inviolables de arquitectura para todo el ecosistema de la solución (**Backend CoreApiRest** y **Frontend FinanceControl_V2**).
> Cualquier propuesta, generación o modificación de código DEBE cumplir rigurosamente con esta guía.
> NUNCA modifiques carpetas del Frontend fuera de `FinanceControl_V2` ni rompas la separación modular o de capas en ninguno de los dos proyectos.

---

## 🚨 PROTOCOLO OBLIGATORIO: PLAN DE ACCIÓN PREVIO (MANDATORIO)

**ANTES de escribir, modificar o refactorizar cualquier línea de código**, la IA o el desarrollador **DEBE obligatoriamente crear y presentar un Plan de Acción estructurado** para aprobación o revisión.

### Estructura Requerida del Plan de Acción
Cualquier propuesta de cambio debe iniciar con una sección clara que contenga:

1. **Objetivo y Alcance:** Qué se va a construir o modificar y por qué.
2. **Análisis de Impacto Capa por Capa:**
   - **Backend (`CoreApiRest`):** Lista de Entidades, DTOs, Requests, Responses, Puertos (Input/Output), Casos de Uso, Controladores Granulares, Repositorios y API Controllers afectados.
   - **Frontend (`FinanceControl_V2`):** Lista de Modelos, Contratos, Servicios HTTP, Helper local (si aplica), Páginas Blazor y Diálogos Radzen afectados.
3. **Secuencia de Ejecución Ordenada (Paso a Paso):**
   - Orden lógico de creación (Ej: Backend Domain $\rightarrow$ Ports/UseCases $\rightarrow$ Repositories/Controllers $\rightarrow$ Frontend Contracts $\rightarrow$ Services $\rightarrow$ Blazor UI $\rightarrow$ Registros DI).
4. **Registros de Inyección de Dependencias:** Especificación exacta de los archivos `ServicesRegistration.cs` (Backend) y `DependencyInjection.cs` (Frontend) que requerirán modificaciones.
5. **Riesgos y Validaciones:** Posibles puntos de fallo, reglas de negocio críticas a cuidar y cómo se verificará la funcionalidad.

> ⚠️ **REGLA DE ORO:** No se generará código definitivo hasta que el Plan de Acción esté definido y claro.

---

## 1. Visión y Estructura General de la Solución Full Stack

La solución está dividida en dos grandes componentes independientes pero alineados en diseño modular por capas:

```text
Ecosistema Financiero/
├── CoreApiRest/                        <-- [BACKEND - Clean Architecture + Ports & Adapters]
│   ├── CoreApiRest.BusinessRules/      <-- Capa 1: Reglas de Negocio del Dominio
│   ├── CoreApiRest.UsesCases/          <-- Capa 2: Casos de Uso
│   ├── CoreApiRest.Controllers/        <-- Capa 3A: Controladores Granulares
│   ├── CoreApiRest.OutputPorts/        <-- Capa 3B: Puertos de Salida (Result<T>)
│   ├── CoreApiRest.Repositories/       <-- Capa 3C: Acceso a Datos (EF Core)
│   ├── CoreApiRest.IOC/                <-- Capa 3D: Inversión de Control
│   └── CoreApiRest/                    <-- Capa 4: API REST ASP.NET Core
│
└── Frontend/                           <-- [FRONTEND - Blazor WASM + Radzen]
    ├── Business.Objects/               <-- [PROHIBIDO MODIFICAR - CÓDIGO LEGADO]
    ├── Functions/                      <-- [PROHIBIDO MODIFICAR - CÓDIGO LEGADO]
    ├── Interfaces/                     <-- [PROHIBIDO MODIFICAR - CÓDIGO LEGADO]
    ├── Services/                       <-- [PROHIBIDO MODIFICAR - CÓDIGO LEGADO]
    ├── Web/                            <-- [PROHIBIDO MODIFICAR - CÓDIGO LEGADO]
    └── FinanceControl_V2/              <-- [ÁREA DE TRABAJO EXCLUSIVA FRONTEND]
        ├── FinanceControl.BusinessRules/
        ├── FinanceControl.Contracts/
        ├── FinanceControl.Services/
        ├── FinanceControl.Helpers/
        └── FinanceControl.Client/
```

---

## 2. Arquitectura del Backend (`CoreApiRest`)

### 2.1 Principios y Estructura de Capas
El Backend implementa **Clean Architecture + Hexagonal (Ports and Adapters)** con **Controladores Granulares**:

```
+-----------------------------------------------------------------------+
| 4. Frameworks & Drivers (CoreApiRest ASP.NET Core API)               |
|   +-----------------------------------------------------------------+ |
|   | 3. Interface Adapters (Controllers, OutputPorts, Repositories)  | |
|   |   +-----------------------------------------------------------+ | |
|   |   | 2. Application Business Rules (CoreApiRest.UsesCases)     | | |
|   |   |   +-----------------------------------------------------+ | | |
|   |   |   | 1. Enterprise Business Rules (BusinessRules)        | | | |
|   |   |   +-----------------------------------------------------+ | | |
|   |   +-----------------------------------------------------------+ | |
|   +-----------------------------------------------------------------+ |
+-----------------------------------------------------------------------+
```

1. **Capa 1: Enterprise Business Rules (`CoreApiRest.BusinessRules`)**
   - **Sin dependencias externas.**
   - Agrupa: `Entities/`, `DTOS/`, `Request/<Modulo>/`, `Responses/`, `Interfaces/Controllers/<Modulo>/`, `Interfaces/Ports/<Modulo>/`, `Interfaces/Repositories/<Modulo>/`.
2. **Capa 2: Application Business Rules (`CoreApiRest.UsesCases`)**
   - Implementa `I<Action>InputPort`.
   - Inyecta el repositorio y el puerto de salida (`_outputPort.Handle(Result...)`).
   - Registro en `UsesCases/ServicesRegistration.cs`.
3. **Capa 3: Interface Adapters**
   - **`CoreApiRest.Controllers`**: Controladores granulares (`ICategoria<Action>Controller`). Ejecutan el caso de uso y retornan `_outputPort.Result`.
   - **`CoreApiRest.OutputPorts`**: Almacena y da formato al resultado (`Result<T>`).
   - **`CoreApiRest.Repositories`**: Implementaciones de EF Core. **OBLIGATORIO:** Colocar siempre en subcarpetas modulares (`Categoria/CategoriaRepository.cs`). Usar `AsNoTracking()` en consultas y `SoftDeleteAsync` para borrados lógicos.
   - **`CoreApiRest.IOC`**: Registra e integra todas las capas.
4. **Capa 4: Frameworks & Drivers (`CoreApiRest`)**
   - API Controllers de ASP.NET Core (`Controllers/<Modulo>/<Modulo>Controller.cs`).
   - Mapea llamadas HTTP a los Controladores Granulares.
   - Extrae Claims de usuario: `string? usuarioId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.Actor);`.
   - Mapea errores de `Result.Error.Type`:
     - `ErrorType.Conflict` $\rightarrow$ `Conflict()` (HTTP 409)
     - `ErrorType.NotFound` $\rightarrow$ `NotFound()` (HTTP 404)
     - `ErrorType.Validation` $\rightarrow$ `BadRequest()` (HTTP 400)
     - Default $\rightarrow$ `StatusCode(500)`

---

## 3. Arquitectura del Frontend (`FinanceControl_V2`)

### 3.1 Matriz de Responsabilidades por Capa

| Proyecto | Responsabilidad Técnica | Referencias Permitidas | Contenido Permitido |
| :--- | :--- | :--- | :--- |
| `FinanceControl.BusinessRules` | DTOs, Requests, Responses y Form Models. | Ninguna | Clases POCO C# (`DTOs`, `Models`, `Request`, `Responses`). |
| `FinanceControl.Contracts` | Interfaces de servicios para consumo de API REST. | `FinanceControl.BusinessRules` | Interfaces C# (`I[Modulo]Service.cs`). |
| `FinanceControl.Services` | Consumo de la API mediante `HttpClient` y configuración DI. | `FinanceControl.Contracts`, `FinanceControl.BusinessRules` | Clases HTTP, Handlers, `DependencyInjection.cs`. |
| `FinanceControl.Helpers` | Procesamiento local puro, cálculos y formateadores. | `FinanceControl.BusinessRules` | Clases estáticas **sin I/O ni HttpClient**. |
| `FinanceControl.Client` | Aplicación Blazor WASM, Páginas y Componentes. | `FinanceControl.Contracts`, `FinanceControl.BusinessRules`, `FinanceControl.Helpers` | `.razor`, `.razor.cs`, Layouts, Páginas, Diálogos Radzen (`Dialogs/`). |

### 3.2 Reglas Inviolables de UI y Servicios
1. ❌ **NUNCA** inyectar ni usar `HttpClient` directamente dentro de archivos `.razor` o componentes UI.
2. ❌ **NUNCA** realizar llamadas de API o acceso a base de datos dentro de `FinanceControl.Helpers`.
3. ❌ **SIEMPRE** inyectar las interfaces de `FinanceControl.Contracts` en los componentes UI (`@inject IUsuarioService UsuarioService`).
4. ❌ **SIEMPRE** usar componentes visuales de **Radzen** (`RadzenStack`, `RadzenCard`, `RadzenRow`, `RadzenColumn`, `RadzenButton`, `RadzenTextBox`, `RadzenDropDown`, `RadzenSwitch`, `RadzenTemplateForm`, `RadzenFormField`, `RadzenBadge`, `RadzenProgressBarCircular`).
5. ❌ **SIEMPRE** gestionar modales/formularios mediante `DialogService` (`DialogService.OpenAsync<TDialog>(...)`) y alertas mediante `NotificationService`.
6. ❌ **SIEMPRE** registrar los nuevos servicios en `FinanceControl.Services/DependencyInjection.cs`.

---

## 4. Convención de Organización Modular Transversal

Toda funcionalidad (ejemplo `Categorias` o `Usuarios`) debe mantener el mismo nombre de módulo transversalmente en Backend y Frontend.

```text
Backend (CoreApiRest)/                   Frontend (FinanceControl_V2)/
├── BusinessRules/                       ├── BusinessRules/
│   ├── Entities/Categoria.cs            │   ├── DTOS/CategoriaDto.cs
│   ├── DTOS/CategoriaDto.cs             │   └── Models/CategoriaFormModel.cs
│   ├── Request/Categoria/               ├── Contracts/
│   │   └── CreateCategoriaRequest.cs    │   └── Categoria/ICategoriaService.cs
│   └── Responses/CategoriaResponse.cs   ├── Services/
├── UsesCases/Categoria/                 │   └── Categoria/CategoriaService.cs
├── Controllers/Categoria/               ├── Helpers/
├── OutputPorts/Categoria/               │   └── CategoriaHelper.cs
├── Repositories/Categoria/              └── Client/
└── CoreApiRest/Controllers/Categoria/       └── Pages/Categoria/
                                                 ├── Categoria.razor
                                                 └── Dialogs/
                                                     └── CategoriaFormDialog.razor
```

---

## 5. Código de Referencia de Extremo a Extremo

### 5.1 Backend: Caso de Uso y API Controller (`CoreApiRest`)

#### Caso de Uso (`CoreApiRest.UsesCases/Categoria/CreateCategoriaUseCase.cs`)
```csharp
namespace CoreApiRest.UsesCases.Categoria;

using CoreApiRest.BusinessRules.DTOS;
using CoreApiRest.BusinessRules.Interfaces.Ports.Categoria;
using CoreApiRest.BusinessRules.Interfaces.Repositories.Categoria;
using CoreApiRest.BusinessRules.Request.Categorias;
using CoreApiRest.BusinessRules.Responses;

public class CreateCategoriaUseCase : ICategoriaCreateInputPort
{
    private readonly ICategoriaRepository _repository;
    private readonly ICategoriaOutputPort _outputPort;

    public CreateCategoriaUseCase(ICategoriaRepository repository, ICategoriaOutputPort outputPort)
    {
        _repository = repository;
        _outputPort = outputPort;
    }

    public async Task Handle(CreateCategoriaRequest request)
    {
        var nombre = request.Nombre?.Trim() ?? string.Empty;
        if (await _repository.ExisteNombreAsync(nombre))
        {
            _outputPort.Handle(Result.Failure<CategoriaResponse>(CategoriaErrors.NombreDuplicado(nombre)));
            return;
        }

        var categoria = new BusinessRules.Entities.Categoria
        {
            IdTipoMovimiento = request.IdTipoMovimiento,
            Nombre = nombre,
            EsCostoFijo = request.EsCostoFijo,
            Activo = request.Activo,
            IdUsuario = request.IdUsuario,
            Valor = request.Valor,
            FechaCreacion = DateTime.UtcNow
        };

        await _repository.CreateAsync(categoria);

        var dto = new CategoriaResponse
        {
            Categoria = new CategoriaDto
            {
                IdCategoria = categoria.IdCategoria,
                Nombre = categoria.Nombre,
                Activo = categoria.Activo
            }
        };

        _outputPort.Handle(Result.Success(dto));
    }
}
```

#### API Controller (`CoreApiRest/Controllers/Categoria/CategoriaController.cs`)
```csharp
namespace CoreApiRest.Controllers.Categoria;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoreApiRest.BusinessRules.Interfaces.Controllers.Categoria;
using CoreApiRest.BusinessRules.Request.Categorias;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriaController : ControllerBase
{
    private readonly ICategoriaCreateController _createController;

    public CategoriaController(ICategoriaCreateController createController)
    {
        _createController = createController;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaRequest request)
    {
        string? usuarioId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.Actor);
        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized("No se pudo obtener la identidad del usuario.");

        request.IdUsuario = Guid.Parse(usuarioId);

        var result = await _createController.Create(request);
        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.Conflict => Conflict(new { result.Error.Code, result.Error.Message }),
                ErrorType.Validation => BadRequest(new { result.Error.Code, result.Error.Message }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { result.Error.Code, result.Error.Message })
            };
        }

        return CreatedAtAction("GetById", new { id = result.Value.Categoria.IdCategoria }, result.Value);
    }
}
```

---

### 5.2 Frontend: Servicio HTTP y Vista Blazor WASM (`FinanceControl_V2`)

#### Servicio HTTP (`FinanceControl.Services/Users/UsuarioService.cs`)
```csharp
namespace FinanceControl.Services.Users;

using System.Net.Http.Json;
using Business.Objects.Models;
using Business.Objects.Responses;
using FinanceControl.Contracts.Users;

public class UsuarioService(HttpClient http) : IUsuarioService
{
    private const string BaseRoute = "api/Usuarios";

    public async Task<List<UsersResponse>> GetAllAsync(bool? soloActivos = null, CancellationToken ct = default)
    {
        var url = soloActivos.HasValue ? $"{BaseRoute}?soloActivos={soloActivos.Value}" : BaseRoute;
        var result = await http.GetFromJsonAsync<List<UsersResponse>>(url, ct);
        return result ?? new List<UsersResponse>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(UsuarioFormModel model, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(BaseRoute, model, ct);
        if (response.IsSuccessStatusCode) return (true, null);

        var error = await response.Content.ReadAsStringAsync(ct);
        return (false, error);
    }
}
```

#### Vista Blazor Radzen (`FinanceControl.Client/Pages/Users/Users.razor`)
```razor
@page "/usuarios"
@using Business.Objects.DTOS
@using Business.Objects.Responses
@using FinanceControl.Contracts.Users
@attribute [Authorize]
@inject IUsuarioService UsuarioService
@inject DialogService DialogService
@inject NotificationService NotificationService

<PageTitle>Usuarios - Finance Control V2</PageTitle>

<RadzenStack Gap="1.5rem">
    <RadzenStack Orientation="Orientation.Horizontal" JustifyContent="JustifyContent.SpaceBetween" AlignItems="AlignItems.Center">
        <RadzenText TextStyle="TextStyle.H3" Class="rz-mb-0">Usuarios</RadzenText>
        <RadzenButton Icon="add" Text="Nuevo usuario" ButtonStyle="ButtonStyle.Primary" Click="@AbrirCrear" />
    </RadzenStack>

    @if (Cargando)
    {
        <RadzenStack AlignItems="AlignItems.Center" Class="rz-p-8">
            <RadzenProgressBarCircular ProgressBarStyle="ProgressBarStyle.Primary" Value="100" ShowValue="false" Mode="ProgressBarMode.Indeterminate" />
        </RadzenStack>
    }
    else
    {
        <RadzenRow Gap="1rem">
            @foreach (var usuario in ListUsers)
            {
                <RadzenColumn Size="12" SizeSM="6" SizeMD="4">
                    <RadzenCard Class="rz-p-4">
                        <RadzenText TextStyle="TextStyle.Body1">@usuario.Usuario.Nombre @usuario.Usuario.Apellidos</RadzenText>
                        <RadzenBadge BadgeStyle="@(usuario.Usuario.Activo ? BadgeStyle.Success : BadgeStyle.Light)"
                                     Text="@(usuario.Usuario.Activo ? "Activo" : "Inactivo")" />
                    </RadzenCard>
                </RadzenColumn>
            }
        </RadzenRow>
    }
</RadzenStack>

@code {
    private List<UsersResponse> ListUsers { get; set; } = new();
    private bool Cargando { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        ListUsers = await UsuarioService.GetAllAsync();
        Cargando = false;
    }

    private async Task AbrirCrear()
    {
        // Lógica de apertura de diálogo Radzen
        await Task.CompletedTask;
    }
}
```

---

## 6. Algoritmo Paso a Paso para Solicitudes de la IA

Cuando el usuario pida implementar una nueva funcionalidad o modificar código existente:

```
[ PASO 0: CREAR Y PRESENTAR PLAN DE ACCIÓN ]
                   │
                   ▼
       ¿Aprobado / Confirmado?
             ├── NO ──> Ajustar Plan de Acción
             └── SÍ ──> Continuar con la ejecución
                   │
                   ▼
[ PASO 1: MÓDULO BACKEND (CoreApiRest) ]
  1.1 Entidades, DTOs, Request, Response en BusinessRules.
  1.2 Interfaces de Controller, Puertos y Repository en BusinessRules.
  1.3 Casos de Uso en UsesCases.
  1.4 Granular Controllers & Output Ports.
  1.5 Repositorio en Repositories/<Modulo>/.
  1.6 API Controller en CoreApiRest/Controllers/<Modulo>/.
  1.7 Registro DI en ServicesRegistration.cs de cada capa.
                   │
                   ▼
[ PASO 2: MÓDULO FRONTEND (FinanceControl_V2) ]
  2.1 DTOs y FormModels en BusinessRules.
  2.2 Contrato de Servicio en Contracts/<Modulo>/.
  2.3 Servicio HTTP en Services/<Modulo>/.
  2.4 Helpers locales si aplica en Helpers/<Modulo>/.
  2.5 Componentes/Páginas/Diálogos Radzen en Client/Pages/<Modulo>/.
  2.6 Registro DI en FinanceControl.Services/DependencyInjection.cs.
```

---

## 7. Formato de Entrega Obligatorio para Respuestas de la IA

Toda respuesta producida por la IA para tareas de desarrollo DEBE seguir esta estructura fija:

1. **Plan de Acción Previo** (o confirmación de seguimiento del plan previamente validado).
2. **Resumen General de la Solución.**
3. **Lista Completa de Archivos Afectados con Rutas Relativas Exactas** (ej. `CoreApiRest/CoreApiRest.Repositories/Categoria/CategoriaRepository.cs`).
4. **Código Fuente Completo de Cada Archivo** (sin código omitido, sustituciones `// TODO` o recortes).
5. **Registros de Inyección de Dependencias Requeridos** (detallando los métodos exactos a modificar en `ServicesRegistration.cs` y `DependencyInjection.cs`).
6. **Validaciones, Supuestos y Criterios de Aceptación.**