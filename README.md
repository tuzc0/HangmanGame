# HangmanGame

Proyecto del juego del ahorcado desarrollado como una solución cliente-servidor usando tecnologías de Microsoft.

Esta solución contiene la base del backend del sistema, organizada en proyectos separados para mantener una estructura limpia entre contratos, servicios, acceso a datos y hospedaje del servicio WCF.

## Tecnologías utilizadas

- C#
- .NET Framework
- WCF
- SQL Server
- Visual Studio
- Git / GitHub

## Estructura de la solución

```text
HangmanGame
├── Hangman.ConsoleHost
├── Hangman.Contracts
├── Hangman.DataAccess
└── HangmanGame.Services
```

### Hangman.ConsoleHost

Proyecto de consola encargado de hospedar los servicios WCF durante el desarrollo y las pruebas.

Contiene:

```text
App.config
Program.cs
```

### Hangman.Contracts

Biblioteca de clases donde se definen los contratos compartidos del sistema.

Aquí deben colocarse:

```text
Interfaces de servicios WCF
DTOs
Requests
Responses
Enums compartidos
```

Este proyecto no debe depender de los demás proyectos de la solución.

### Hangman.DataAccess

Biblioteca de clases encargada del acceso a datos.

Aquí deben colocarse:

```text
Conexión a SQL Server
Repositorios
Consultas SQL
Mapeo de datos
```

La lógica de validación principal no debe estar en la base de datos, sino en el programa principal o en los servicios correspondientes.

### HangmanGame.Services

Proyecto donde se implementan los servicios WCF.

Aquí deben colocarse las clases que implementan los contratos definidos en `Hangman.Contracts`.

Ejemplos:

```text
AuthService
PlayerService
MatchService
ScoreService
```

## Base de datos

El proyecto utiliza SQL Server como sistema gestor de base de datos.

La base de datos principal se llama:

```text
HangmanDB
```

El script oficial para crear la base de datos se encuentra en:

```text
HangmanDB_base_original.sql
```

Este script crea las tablas principales del sistema:

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

## Consideraciones del diseño de base de datos

La base de datos se mantiene enfocada en la persistencia de datos.

La base se encarga principalmente de:

```text
PRIMARY KEY
FOREIGN KEY
NOT NULL
Tamaños máximos de campos
UNIQUE
Índices
DEFAULT simples
```

Las validaciones principales se realizan desde el programa principal o desde los servicios WCF.

Ejemplos de validaciones que deben manejarse desde la aplicación:

```text
Formato de correo electrónico
Nombre vacío
Fecha de nacimiento válida
Teléfono correcto
Estados permitidos
Reglas del juego
Tiempo de expiración del token de verificación
```

## Verificación de correo electrónico

La tabla encargada de la verificación de correo es:

```text
EMAIL_VERIFICATION
```

Esta tabla almacena:

```text
verification_code_hash
expires_at
verified_at
attempts
is_used
created_at
```

El tiempo máximo de uso del token debe ser calculado por el programa principal antes de guardar el registro en la base de datos.

## Ejecución del script de base de datos

1. Abrir SQL Server Management Studio.
2. Conectarse al servidor local de SQL Server.
3. Abrir el archivo `HangmanDB_base_original.sql`.
4. Ejecutar el script completo.
5. Verificar que se haya creado la base de datos `HangmanDB`.

## Importante

El script elimina la base de datos `HangmanDB` si ya existe y la vuelve a crear desde cero.

No debe ejecutarse sobre una base de datos que contenga información importante.

## Cadena de conexión

No se deben publicar credenciales reales dentro del repositorio.

Cada desarrollador debe configurar su propia cadena de conexión de forma local.

Ejemplo con autenticación integrada de Windows:

```text
Server=TU_SERVIDOR;Database=HangmanDB;Integrated Security=True;TrustServerCertificate=True;
```

Reemplazar `TU_SERVIDOR` por el nombre real de la instancia de SQL Server.

## Archivos que no deben subirse

No se deben subir archivos generados automáticamente por Visual Studio o por la compilación.

Ejemplos:

```text
.vs/
bin/
obj/
*.user
*.suo
TestResults/
```

Se recomienda usar un archivo `.gitignore` para evitar subir estos archivos.

## Estado actual

La solución contiene la estructura base del backend WCF para el juego del ahorcado.

El desarrollo continuará integrando los servicios, el acceso a datos y posteriormente la aplicación cliente WPF.
