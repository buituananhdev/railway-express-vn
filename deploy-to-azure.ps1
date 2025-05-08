# ================================
# DEPLOY INFRASTRUCTURE TO AZURE (EMPTY BE APP + RabbitMQ/Redis)
# ================================

# === CONFIGURATION ===
$resourceGroup    = "railway-resource"
$location         = "southeastasia"
$envName          = "railway-env"
$random           = Get-Random -Maximum 9999

# ACR (for future use)
$acrName          = "railwayacr$random"
# Container Apps names
$beAppName        = "railway-microservices-app$random"
$rabbitAppName       = "railway-msg-app$random"
$redisAppName     = "railway-redis-app$random"

# MySQL credentials
$mysqlServer      = "railway-mysql$random"
$mysqlAdmin       = "mysqladmin"
$mysqlPassword    = "pAssw0rd!"

# RabbitMQ secret
$rabbitmqPassword = "admin123"

# # === CREATE RESOURCE GROUP ===
Write-Host "Creating resource group..."
az group create --name $resourceGroup --location $location | Out-Null

# # === CREATE ACR ===
Write-Host "Creating Azure Container Registry..."
az acr create --name $acrName --resource-group $resourceGroup --sku Basic --location $location --admin-enabled true | Out-Null

# # === GET ACR CREDENTIALS ===
$acrUsername    = az acr credential show --name $acrName --query username -o tsv
$acrPassword    = az acr credential show --name $acrName --query "passwords[0].value" -o tsv
$acrLoginServer = az acr show --name $acrName --query loginServer -o tsv

# # === CREATE CONTAINER APPS ENVIRONMENT ===
# Write-Host "Creating Azure Container Apps environment..."
az containerapp env create --name $envName --resource-group $resourceGroup --location $location | Out-Null

# # === CREATE MYSQL FLEXIBLE SERVER ===
Write-Host "Creating MySQL Flexible Server..."
az mysql flexible-server create `
  --name $mysqlServer `
  --resource-group $resourceGroup `
  --location $location `
  --admin-user $mysqlAdmin `
  --admin-password $mysqlPassword `
  --tier Burstable `
  --sku-name Standard_B1ms `
  --storage-size 32 | Out-Null

# # === CONFIGURE MYSQL FIREWALL RULES (ALLOW ALL) ===
Write-Host "Configuring MySQL firewall rule to allow all IPs..."
# Allow all IPv4 addresses (0.0.0.0 - 255.255.255.255)
az mysql flexible-server firewall-rule create `
  --resource-group $resourceGroup `
  --name $mysqlServer `
  --rule-name AllowAll `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 255.255.255.255 | Out-Null

# # === CREATE EMPTY BACKEND CONTAINER APP ===
Write-Host "Creating empty backend Container App..."
az containerapp create `
  --name $beAppName `
  --resource-group $resourceGroup `
  --environment $envName `
  --image mcr.microsoft.com/azuredocs/aci-helloworld `
  --cpu 0.5 --memory 1.0Gi `
  --registry-server $acrLoginServer `
  --registry-username $acrUsername `
  --registry-password $acrPassword `
  --ingress external `
  --target-port 80 | Out-Null

# # === DEPLOY RABBITMQ CONTAINER APP ===
Write-Host "Deploying RabbitMQ Container App..."
az containerapp create `
  --name $rabbitAppName `
  --resource-group $resourceGroup `
  --environment $envName `
  --image rabbitmq:3-management `
  --cpu 0.5 --memory 1Gi `
  --secrets "rabbitmq-password=$rabbitmqPassword" `
  --env-vars "RABBITMQ_DEFAULT_USER=admin" "RABBITMQ_DEFAULT_PASS=secretref:rabbitmq-password" `
  --ingress external `
  --target-port 15672 | Out-Null


# # === DEPLOY REDIS CONTAINER APP ===
Write-Host "Deploying REDIS Container App..."
az containerapp create `
  --name $redisAppName `
  --resource-group $resourceGroup `
  --environment $envName `
  --image redis:alpine `
  --cpu 0.25 --memory 0.5Gi `
  --ingress internal `
  --target-port 6379 | Out-Null

# # === OUTPUT INFO ===
Write-Host ""
Write-Host "===== ✅ DEPLOYMENT COMPLETED ====="
Write-Host "Resource Group:       $resourceGroup"
Write-Host "ACR Login Server:     $acrLoginServer"
Write-Host "ACR Username:         $acrUsername"
Write-Host "ACR Password:         $acrPassword"
Write-Host "MySQL Server:         $mysqlServer.mysql.database.azure.com"
Write-Host "MySQL User:           $mysqlAdmin@$mysqlServer"
Write-Host "MySQL Password:       $mysqlPassword"
Write-Host "Empty BE App Name:    $beAppName"
Write-Host "BE App URL:           $(az containerapp show --name $beAppName --resource-group $resourceGroup --query properties.configuration.ingress.fqdn -o tsv)"
Write-Host "Msg App Name:         $rabbitAppName"
Write-Host "RabbitMQ UI Port:     15672"
Write-Host "RabbitMQ Password:    $rabbitmqPassword"