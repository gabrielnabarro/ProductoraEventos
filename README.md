Productora de Eventos - Plataforma de Venta de Entradas

Antes de iniciar, se debe tener instalado en el sistema:
		•.NET 8 SDK
	  •	SQL Server LocalDB (incluido con Visual Studio)
	  •	Visual Studio 2022 o Visual Studio Code
  
Pasos para ejecutar el proyecto:
  1.	Clonar repositorio
	    •	git clone https://github.com/gabrielnabarro/ProductoraEventos/tree/master

  2.	Abrir una terminal y navegar a la carpeta del repositorio clonado
	    •	cd EventsApi

  3.	Configurar la cadena de conexión
	    •	La cadena de conexión por defecto está configurada en EventsApi/appsettings.json :
	      "ConnectionStrings":{"DefaultConnection":"Server=(localdb)\\mssqllocaldb;Database=ProductoraDB;Tr}
	    	
  4.	Ejecutar las migraciones y levantar la aplicación
	    •	Las migraciones se aplican automáticamente al iniciar la aplicación. No es necesario ejecutarlas manualmente.
	    •	Al iniciar por primera vez, el sistema:
	        i.	Crea la base de datos ProductoraDB automáticamente
	        ii.	Aplica todas las migraciones
	        iii.	Precarga los datos iniciales: 1 evento, 2 sectores y 50 butacas por sector
    	
  5.	Acceder a la aplicación
	    •	Swagger UI  http://localhost:5269/swagger
	    •	API (HTTPS)  https://localhost:7026
