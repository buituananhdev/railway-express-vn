#!/bin/bash

# ====================
# CONFIG
# ====================
RESOURCE_GROUP="railway-resource"
LOCATION="southeastasia"
ENV_NAME="railway-env"
ACR_NAME="railwayacr$RANDOM"
REDIS_NAME="railwayredis$RANDOM"
MYSQL_SERVER="railwaymysql$RANDOM"
MYSQL_ADMIN="mysqladmin"
MYSQL_PASSWORD="StrongPassword123!"

# ====================
# CREATE RESOURCE GROUP
# ====================
echo "🧱 Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

# ====================
# CREATE ACR
# ====================
echo "📦 Creating Azure Container Registry..."
az acr create --name $ACR_NAME --resource-group $RESOURCE_GROUP --sku Basic --location $LOCATION --admin-enabled true

# Get ACR credentials
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query username -o tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv)
ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --query loginServer -o tsv)

# ====================
# CREATE CONTAINER APPS ENVIRONMENT
# ====================
echo "🌱 Creating Azure Container Apps environment..."
az containerapp env create \
  --name $ENV_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# ====================
# CREATE REDIS
# ====================
echo "🧠 Creating Azure Cache for Redis..."
az redis create \
  --name $REDIS_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Basic \
  --vm-size c0

# ====================
# CREATE MYSQL FLEXIBLE SERVER
# ====================
echo "🐬 Creating MySQL Flexible Server..."
az mysql flexible-server create \
  --name $MYSQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $MYSQL_ADMIN \
  --admin-password $MYSQL_PASSWORD \
  --sku-name B1ms \
  --storage-size 32

# ====================
# OUTPUT
# ====================
echo "✅ DONE!"
echo "🔐 ACR login: $ACR_LOGIN_SERVER"
echo "🔐 ACR username: $ACR_USERNAME"
echo "🔐 ACR password: $ACR_PASSWORD"
echo "🔐 MySQL server: $MYSQL_SERVER.mysql.database.azure.com"
echo "🔐 MySQL user: $MYSQL_ADMIN@$MYSQL_SERVER"
echo "🔐 MySQL password: $MYSQL_PASSWORD"
echo "🔐 Redis name: $REDIS_NAME"
