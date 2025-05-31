# README - Sistema de Agricultura de Precisión

## 📊 Descripción General

Este sistema de agricultura de precisión permite monitorizar y analizar datos de diversos sensores agrícolas en tiempo real. La plataforma simula sensores, recopila datos, detecta anomalías y almacena toda la información para su posterior análisis.

## 🏗️ Arquitectura

El proyecto está compuesto por varios módulos que trabajan juntos:

- **SensorSimulator**: Genera datos simulados de diferentes tipos de sensores agrícolas.
- **DataHub**: API REST que recibe, procesa y almacena los datos de los sensores.
- **PostgreSQL**: Base de datos para almacenamiento persistente.
- **RabbitMQ**: Sistema de mensajería para comunicación entre componentes.
- **pgAdmin**: Interfaz gráfica para gestionar la base de datos.

Todos los componentes se ejecutan como contenedores Docker.

## 🔧 Requisitos Previos

- [Docker](https://www.docker.com/products/docker-desktop) y Docker Compose
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download) (solo para desarrollo)

## 🚀 Configuración y Ejecución

### Paso 1: Clonar el repositorio

```bash
git clone https://github.com/tuusuario/precision-agriculture.git
cd precision-agriculture
```

### Paso 2: Iniciar los servicios con Docker Compose

```bash
cd docker
docker-compose up -d
```

Esto iniciará:
- PostgreSQL en puerto 5432
- RabbitMQ en puerto 5672 (UI en 15672)
- pgAdmin en puerto 5050
- DataHub en puerto 5000

### Paso 3: Iniciar el simulador de sensores

```bash
cd ../src/SensorSimulator
dotnet run
```

Para apuntar a un DataHub en otra ubicación:

```bash
dotnet run http://otra-url:puerto
```

## 🔍 Acceso a los servicios

- **DataHub API**: http://localhost:5000
- **RabbitMQ Management**: http://localhost:15672
  - Usuario: admin
  - Contraseña: agriculture123
- **pgAdmin**: http://localhost:5050
  - Email: admin@example.com
  - Contraseña: admin123

### Configurar pgAdmin

1. Accede a pgAdmin en http://localhost:5050
2. Inicia sesión con las credenciales indicadas
3. Añade un nuevo servidor:
   - Nombre: Agriculture DB
   - Host: postgres
   - Puerto: 5432
   - Base de datos: agriculturedb
   - Usuario: agriuser
   - Contraseña: agripass

## 📊 Estructura de Datos

El sistema almacena dos tipos principales de datos:

1. **sensor_data**: Registros de lecturas de sensores
   - ID del sensor, tipo, marca temporal, ubicación
   - Datos en bruto como JSON
   - Indicador de anomalía

2. **anomalies**: Anomalías detectadas en los datos
   - Referencia al registro de datos
   - Descripción de la anomalía
   - Fecha de detección

## 🧩 Tipos de Sensores Simulados

- **Sensores de Humedad del Suelo**: Miden la humedad del suelo
- **Sensores Climáticos**: Registran temperatura, humedad, velocidad del viento
- **Sensores de Nutrientes**: Miden niveles de N-P-K en el suelo
- **Sensores de Crecimiento**: Monitorizan altura y salud de los cultivos

## 🛠️ Desarrollo y Extensión

### Añadir nuevos tipos de sensores

Crea una nueva clase en `src/SensorSimulator/Simulators` que implemente la generación de datos.

### Personalizar el almacenamiento

Modifica `DatabaseService.cs` para cambiar la forma de almacenar los datos.

### Implementar nuevos análisis

Extiende los servicios de análisis en el DataHub para detectar diferentes tipos de anomalías.

## ⚠️ Solución de Problemas

### Los servicios no se inician correctamente

```bash
docker-compose down
docker-compose up -d
```

### Error de conexión a la base de datos

Verifica que estés usando el host correcto:
- Desde fuera de Docker: `localhost`
- Entre contenedores: `postgres`

### El simulador no puede conectar con el DataHub

Asegúrate de que la URL sea correcta y el contenedor DataHub esté funcionando:

```bash
docker ps
```

## 📚 Tablas de la Base de Datos

- **sensor_data**: Almacena todas las lecturas de los sensores
- **anomalies**: Registra anomalías detectadas en las lecturas

## 🔄 Flujo de Datos

1. El Simulador genera lecturas de sensores
2. Las envía al DataHub mediante HTTP
3. DataHub analiza los datos en busca de anomalías
4. Los datos y anomalías se guardan en PostgreSQL
5. Los datos pueden visualizarse mediante pgAdmin

## 📄 Licencia

[Especificar licencia]

---

Desarrollado para el proyecto de Agricultura de Precisión 🌱
