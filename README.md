# *** Productora de Eventos - Plataforma de Venta de Entradas ***

## Requisitos
Antes de iniciar, se debe tener instalado en el sistema:
- `.NET 8 SDK`
- `SQL Server LocalDB`
- `Visual Studio 2022` o `Visual Studio Code`

Pasos para ejecutar el proyecto:

## 1. Clonar el repositorio
```bash
git clone https://github.com/gabrielnabarro/ProductoraEventos.git
cd ProductoraEventos
```

## 2. Configurar la cadena de conexion
La cadena de conexion se encuentra en `EventsApi/appsettings.json`

Configuracion por defecto:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductoraDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

## 3. Ejecutar el proyecto
Desde la raiz de la solucion, ejecutar:

```bash
dotnet run --project EventsApi
```

Tambien se puede abrir `ProductoraEventos.sln` en Visual Studio y establecer `EventsApi` como proyecto de inicio.

## 4. Inicializacion de base de datos
Al iniciar la aplicacion por primera vez, el sistema realiza automaticamente:
- creacion de la base de datos
- aplicacion de migraciones pendientes
- precarga de datos iniciales

No es necesario ejecutar migraciones manualmente.

Importante: si la base de datos contiene datos parciales de una inicializacion anterior, la precarga no se rehace automaticamente y la aplicacion puede fallar al iniciar. En ese caso, se debe limpiar la base de datos antes de volver a ejecutar el proyecto.

## Datos demo precargados
La aplicacion crea automaticamente:
- 1 evento: ACDC en Argentina
- 2 sectores: VIP y General
- 50 butacas por sector
- 2 usuarios demo

## Usuarios demo
- Email: demo@productoraeventos.local
- Password: demo1234
- Email: demo2@productoraeventos.local
- Password: demo1234

## Accesos a la aplicacion
Una vez levantada la aplicacion, se puede acceder desde:
- Pagina web HTTP: http://localhost:5269/
- Pagina web HTTPS: https://localhost:7026/
- Swagger UI: http://localhost:5269/swagger

