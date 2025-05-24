#!/bin/bash

echo "🔧 Setting up Precision Agriculture Environment..."

# Crear estructura de directorios
mkdir -p src/{SensorSimulator,DataHub}
mkdir -p docker/rabbitmq
mkdir -p scripts

# Restaurar paquetes NuGet
echo "📦 Restoring NuGet packages..."
cd src/SensorSimulator && dotnet restore
cd ../DataHub && dotnet restore

echo "✅ Environment setup completed!"