#!/bin/bash

echo "🌱 Starting Precision Agriculture Services..."

# Iniciar RabbitMQ
echo "📦 Starting RabbitMQ..."
cd docker && docker-compose up -d

# Esperar a que RabbitMQ esté listo
echo "⏳ Waiting for RabbitMQ to be ready..."
sleep 10

# Iniciar Data Hub
echo "🏢 Starting Data Hub..."
cd ../src/DataHub && dotnet run &

# Esperar a que el Hub esté listo
sleep 5

# Iniciar Sensor Simulator
echo "📡 Starting Sensor Simulator..."
cd ../SensorSimulator && dotnet run

echo "✅ All services started successfully!"