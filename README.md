# Hangman.Server

Servidor del juego del Ahorcado desarrollado con WCF y SQL Server, como proyecto final de la materia *Tecnologías para la Construcción de Software* — Ingeniería de Software, UV FEI, FEB–JUL 2026.

Expone los servicios WCF que consume la aplicación cliente `Hangman.Client`.

## Tecnologías

- C# / .NET Framework 4.7.2
- WCF (Windows Communication Foundation)
- Entity Framework 6 (Database First)
- SQL Server
- Visual Studio 2022
- Git / GitHub

## Estructura de la solución

```text
Hangman.Server
├── Hangman.Contracts\       <- Interfaces de servicio, DTOs, Requests, Responses, Enums
├── Hangman.DataAccess\      <- Contexto EF, modelos generados, repositorios
├── Hangman.Services\        <- Implementación de los servicios WCF
└── Hangman.ConsoleHost\     <- Host de consola para levantar los servicios en desarrollo
```

### Hangman.Contracts

Biblioteca de clases compartida. Define los contratos del sistema.

```text
IAuthService.cs
IPlayerService.cs
IMatchService.cs
IScoreService.cs
ICatalogService.cs
DTOs\
Enums\
    MatchStatus.cs
    MovementType.cs
    AccountStatus.cs
```

Este proyecto no debe depender de ningún otro proyecto de la solución.

### Hangman.DataAccess

Acceso a datos mediante Entity Framework Database First.

```text
Model\               <- Clases generadas por EF (PLAYER, ACCOUNT, MATCH, etc.)
Repositories\        <- Consultas y operaciones sobre la BD
App.config           <- Cadena de conexión (no subir credenciales reales)
```

### Hangman.Services

Implementación de los contratos definidos en `Hangman.Contracts`.

```text
AuthService.cs       <- CU-01 Registro, CU-02 Login
PlayerService.cs     <- CU-03 Ver perfil, CU-04 Editar, CU-14 Idioma
CatalogService.cs    <- CU-15 Categorías y palabras
MatchService.cs      <- CU-05 Listar, CU-06 Crear, CU-07 Unirse, CU-08 Jugar, CU-09 Abandonar
ScoreService.cs      <- CU-10 Puntaje global, CU-11 Penalizaciones
```

### Hangman.ConsoleHost

Proyecto de consola que hospeda los servicios WCF durante desarrollo y pruebas.

```text
Program.cs           <- Levanta y cierra los ServiceHost
App.config           <- Configuración de endpoints WCF
```

## Base de datos

El sistema gestor de base de datos es SQL Server. La base se llama `HangmanDB`.

El script oficial de creación se encuentra en:

```text
HangmanDB_base_original.sql
```

Tablas principales:

```text
LANGUAGE
PLAYER
ACCOUNT
EMAIL_VERIFICATION
CATEGORY
WORD
MATCH
MATCH_GUESS
SCORE_MOVEMENT
```

### Ejecutar el script

1. Abrir SQL Server Management Studio
2. Conectarse al servidor local
3. Abrir `HangmanDB_base_original.sql`
4. Ejecutar el script completo

> El script elimina y recrea `HangmanDB` desde cero. No ejecutar sobre datos importantes.

## Configuración de cadena de conexión

Cada desarrollador configura su conexión local. No subir credenciales al repositorio.

Ejemplo con autenticación integrada de Windows:

```text
Server=TU_SERVIDOR;Database=HangmanDB;Integrated Security=True;TrustServerCertificate=True;
```

Copiar `ConnectionStrings.example.config` como `ConnectionStrings.config` y reemplazar `TU_SERVIDOR` con el nombre real de la instancia.

## Convención de ramas

| Rama | Uso |
|---|---|
| `main` | Versiones estables entregadas |
| `develop` | Integración continua del equipo |
| `feature/nombre-servicio` | Desarrollo de un servicio específico |
| `fix/descripcion` | Corrección de errores |

Ejemplo: `feature/auth-service`, `feature/match-service`

## Relación con el cliente

Este repositorio es exclusivamente el servidor. El cliente WPF vive en:

> [https://github.com/MZSM98/Hangman.Client](https://github.com/MZSM98/Hangman.Client)

Ambos proyectos deben estar corriendo simultáneamente para que el juego funcione.

## Equipo

- Jorge Manuel Cobos Castro
- Marcos Zenón Sánchez Mendizábal
- Guillermo Velázquez Rosiles
- Claudio Trujillo Zepeda

**Docente:** Mtro. Ramón Gómez Romero  
**Materia:** Tecnologías para la Construcción de Software  
**Periodo:** FEB – JUL 2026
