# Arquitectura de Agricultura de Precisión - Estructura del Proyecto

## 📁 Estructura de Directorios

```
PrecisionAgriculture/
├── src/
│   ├── SensorSimulator/
│   │   ├── PrecisionAgriculture.SensorSimulator.csproj
│   │   ├── Program.cs
│   │   └── Sensors/
│   │       ├── SoilMoistureSensor.cs
│   │       ├── WeatherSensor.cs
│   │       ├── NutrientSensor.cs
│   │       └── CropGrowthSensor.cs
│   │
│   ├── DataHub/
│   │   ├── PrecisionAgriculture.DataHub.csproj
│   │   ├── Program.cs
│   │   ├── Controllers/
│   │   │   └── SensorDataController.cs
│   │   ├── Services/
│   │   │   └── RabbitMQService.cs
│   │   ├── Models/
│   │   │   └── SensorData.cs
│   │   └── appsettings.json
│   │
│   └── PresentationMVC/ (Futuro)
│       ├── PrecisionAgriculture.Web.csproj
│       ├── Controllers/
│       ├── Views/
│       └── Services/
│
├── docker/
│   ├── docker-compose.yml
│   └── rabbitmq/
│       └── rabbitmq.conf
│
├── scripts/
│   ├── start-services.sh
│   └── setup-environment.sh
│
└── README.md
```

## 🔧 Comandos de Ejecución

### 1. Iniciar RabbitMQ
```bash
cd docker
docker-compose up -d
```

### 2. Iniciar el Data Hub
```bash
cd src/DataHub
dotnet run
```

### 3. Iniciar el Simulador de Sensores
```bash
cd src/SensorSimulator
dotnet run
```

## 📊 Monitoreo y Verificación

### Acceder al Panel de RabbitMQ
- URL: http://localhost:15672
- Usuario: admin
- Contraseña: agriculture123

### Verificar el Data Hub
- Health Check: http://localhost:5000/api/sensordata/health
- Stats: http://localhost:5000/api/sensordata/stats
- Swagger UI: http://localhost:5000/swagger

## 🌐 Arquitectura de Eventos

### Flujo de Datos
1. **Sensores** → Generan datos simulados de diferentes tipos
2. **HTTP Client** → Envía datos al Data Hub via REST API
3. **Data Hub** → Recibe datos y los valida
4. **RabbitMQ Publisher** → Publica eventos en la cola
5. **Queue** → Almacena eventos para procesamiento
6. **Future Consumer** → El componente MVC consumirá estos eventos

### Tipos de Sensores Implementados
- **SoilMoisture**: Humedad del suelo, temperatura, pH
- **Weather**: Temperatura, humedad, viento, presión, UV
- **Nutrient**: Nitrógeno, fósforo, potasio, conductividad
- **CropGrowth**: Altura, índice foliar, clorofila, etapa de crecimiento

### Configuración de Colas RabbitMQ
- **Exchange**: `agriculture.events` (Topic)
- **Queue**: `sensor.data` (Durable)
- **Routing Key**: `sensor.data.received`
