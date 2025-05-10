<#
.SYNOPSIS
    Script to create or update Azure Container Apps for multiple microservices, assign ACR pull role.
.DESCRIPTION
    This script automates the deployment of multiple microservices to Azure Container Apps,
    handling both initial creation and updates of existing apps. It configures networking,
    scaling, resource limits, and assigns proper ACR pull permissions.
.PARAMETER AcrName
    Azure Container Registry name (without .azurecr.io).
.PARAMETER ResourceGroup
    Azure Resource Group containing ACR and Container App Environment.
.PARAMETER Location
    Azure region for Container App Environment.
.PARAMETER Version
    Image tag/version to deploy. Use 'latest' to auto-fetch most recent tag from ACR.
.PARAMETER EnvironmentName
    Name of the Container App Environment.
.PARAMETER ConfigFile
    Path to JSON configuration file for service customization.
.EXAMPLE
    .\Deploy-ContainerApps.ps1 -AcrName "myacr" -ResourceGroup "myRG" -Version "1.0.3"
.NOTES
    Requires Azure CLI and PowerShell 5.1 or later.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$AcrName,
    
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = 'southeastasia',
    
    [Parameter(Mandatory=$false)]
    [string]$Version = 'latest',
    
    [Parameter(Mandatory=$false)]
    [string]$EnvironmentName = 'railway-env',
    
    [Parameter(Mandatory=$false)]
    [string]$ConfigFile
)

# Set strict mode and error action preference
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Function to log with timestamp
function Write-Log {
    param([string]$Message)
    Write-Host "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Message"
}

# Function to check Azure CLI installation
function Test-AzCli {
    try {
        $null = az --version
        return $true
    }
    catch {
        return $false
    }
}

# Verify Azure CLI is installed
if (-not (Test-AzCli)) {
    Write-Error "Azure CLI is not installed or not in PATH. Please install Azure CLI first."
    exit 1
}

# Check if logged in to Azure
try {
    $account = az account show --output json | ConvertFrom-Json
    Write-Log "Using Azure account: $($account.name) (Subscription: $($account.id))"
}
catch {
    Write-Error "Not logged in to Azure. Please run 'az login' first."
    exit 1
}

# Define default services configuration if no config file provided
$serviceDefaults = @{
    "api-gateway" = @{
        "port" = 80
        "minReplicas" = 1
        "maxReplicas" = 3
        "cpu" = 0.5
        "memory" = "1Gi"
        "env" = @{
            "ENVIRONMENT" = "production"
        }
    }
    "auth-api" = @{
        "port" = 80
        "minReplicas" = 1
        "maxReplicas" = 3
        "cpu" = 0.5
        "memory" = "1Gi"
        "env" = @{
            "ENVIRONMENT" = "production"
        }
    }
    "usermanagement-api" = @{
        "port" = 80
        "minReplicas" = 1
        "maxReplicas" = 3
        "cpu" = 0.5
        "memory" = "1Gi"
        "env" = @{
            "ENVIRONMENT" = "production"
        }
    }
    "admin-api" = @{
        "port" = 80
        "minReplicas" = 1
        "maxReplicas" = 3
        "cpu" = 0.5
        "memory" = "1Gi"
        "env" = @{
            "ENVIRONMENT" = "production"
        }
    }
    "booking-api" = @{
        "port" = 80
        "minReplicas" = 1
        "maxReplicas" = 3
        "cpu" = 0.5
        "memory" = "1Gi"
        "env" = @{
            "ENVIRONMENT" = "production"
        }
    }
    "payment-api" = @{
        "port" = 80
        "minReplicas" = 1
        "maxReplicas" = 3
        "cpu" = 0.5
        "memory" = "1Gi"
        "env" = @{
            "ENVIRONMENT" = "production"
        }
    }
    "notification-api" = @{
        "port" = 80
        "minReplicas" = 1
        "maxReplicas" = 3
        "cpu" = 0.5
        "memory" = "1Gi"
        "env" = @{
            "ENVIRONMENT" = "production"
        }
    }
}

# Load configuration file if provided
if ($ConfigFile -and (Test-Path $ConfigFile)) {
    try {
        Write-Log "Loading service configuration from $ConfigFile"
        $serviceConfig = Get-Content -Path $ConfigFile -Raw | ConvertFrom-Json -AsHashtable
        # Merge with defaults
        foreach ($service in $serviceConfig.Keys) {
            if ($serviceDefaults.ContainsKey($service)) {
                $serviceDefaults[$service] = $serviceConfig[$service]
            }
        }
    }
    catch {
        Write-Error "Failed to load configuration file: $_"
        exit 1
    }
}

# Login to ACR
Write-Log "Logging into ACR $AcrName..."
try {
    az acr login --name $AcrName | Out-Null
    
    # Verify ACR access by listing repositories
    Write-Log "Verifying ACR access..."
    $repos = az acr repository list --name $AcrName --output tsv
    if (-not $repos) {
        Write-Warning "Successfully logged in but no repositories found in ACR. This might be expected for a new registry."
    } else {
        Write-Log "Successfully verified ACR access. Found repositories: $repos"
    }
    
    # Get ACR credentials for the containerapp to use
    Write-Log "Getting ACR credentials for container app authentication..."
    $acrCreds = az acr credential show --name $AcrName | ConvertFrom-Json
    $acrUsername = $acrCreds.username
    $acrPassword = $acrCreds.passwords[0].value
    
    if (-not $acrUsername -or -not $acrPassword) {
        Write-Error "Failed to get valid ACR credentials. Please ensure you have proper permissions."
        exit 1
    }
}
catch {
    Write-Error "Failed to login to ACR $AcrName. Error: $_"
    exit 1
}

# Create Container App Environment if not exists
Write-Log "Checking Container App Environment '$EnvironmentName'..."
$envExists = az containerapp env list --resource-group $ResourceGroup --query "[?name=='$EnvironmentName'].name" -o tsv
if (-not $envExists) {
    Write-Log "Creating Container App Environment '$EnvironmentName' in $Location..."
    try {
        az containerapp env create --name $EnvironmentName --resource-group $ResourceGroup --location $Location | Out-Null
        Write-Log "Container App Environment created successfully."
    }
    catch {
        Write-Error "Failed to create Container App Environment: $_"
        exit 1
    }
} else {
    Write-Log "Container App Environment '$EnvironmentName' already exists."
}

# Get subscription ID for role assignment scope
$subscriptionId = az account show --query id -o tsv
$acrScope = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.ContainerRegistry/registries/$AcrName"

# Process each service
$services = $serviceDefaults.Keys
$totalServices = $services.Count
$currentService = 0

foreach ($svc in $services) {
    $currentService++
    Write-Log "[$currentService/$totalServices] Processing service: $svc"
    
    # Get service configuration
    $config = $serviceDefaults[$svc]
    
    # Determine tag: if 'latest', fetch most recent tag from ACR
    $tag = $Version
    if ($Version -eq 'latest') {
        try {
            $tag = az acr repository show-tags --name $AcrName --repository $svc --orderby time_desc --top 1 -o tsv
            if (-not $tag) {
                Write-Warning "No tags found for repository '$svc' in ACR '$AcrName'. Skipping service."
                continue
            }
            Write-Log "Latest tag for $svc is $tag"
        }
        catch {
            Write-Warning "Failed to get latest tag for $svc. Error: $_. Skipping service."
            continue
        }
    }

    # Build image string and app name
    $image = "$($AcrName).azurecr.io/$($svc):$($tag)"
    $appName = "railway-$svc"
    
    # Prepare environment variables
    $envVars = @("SERVICE_VERSION=$tag")
    foreach ($key in $config.env.Keys) {
        $envVars += "$key=$($config.env[$key])"
    }
    
    # Check if container app exists
    $exists = $null
    try {
        $exists = az containerapp show --name $appName --resource-group $ResourceGroup --query name -o tsv 2>$null
    }
    catch {}
    
    if (-not $exists) {
        Write-Log "Creating Container App $appName..."
        try {
            # Create new container app with registry credentials
            az containerapp create `
                --name $appName `
                --resource-group $ResourceGroup `
                --environment $EnvironmentName `
                --image $image `
                --ingress external `
                --target-port $config.port `
                --min-replicas $config.minReplicas `
                --max-replicas $config.maxReplicas `
                --cpu $config.cpu `
                --memory $config.memory `
                --env-vars $envVars `
                --revision-suffix $tag `
                --registry-server "$($AcrName).azurecr.io" `
                --registry-username $acrUsername `
                --registry-password $acrPassword `
                --system-assigned | Out-Null

            # Wait a moment for identity propagation
            Start-Sleep -Seconds 5

            # Assign ACR Pull role
            $identityObjectId = az containerapp show --name $appName --resource-group $ResourceGroup --query "identity.principalId" -o tsv
            if ($identityObjectId) {
                Write-Log "Assigning AcrPull role to $appName..."
                az role assignment create `
                    --assignee $identityObjectId `
                    --role AcrPull `
                    --scope $acrScope | Out-Null
                Write-Log "Created $appName and assigned AcrPull role."
            }
            else {
                Write-Warning "Failed to get managed identity for $appName. ACR Pull role not assigned."
            }
        }
        catch {
            Write-Error "Failed to create Container App $appName. Error: $_"
            continue
        }
    } else {
        Write-Log "Updating Container App $appName..."
        try {
            az containerapp registry set `
                --name $appName `
                --resource-group $ResourceGroup `
                --server "$($AcrName).azurecr.io" `
                --username $acrUsername `
                --password $acrPassword | Out-Null

            az containerapp update `
                --name $appName `
                --resource-group $ResourceGroup `
                --image $image `
                --cpu $config.cpu `
                --memory $config.memory `
                --min-replicas $config.minReplicas `
                --max-replicas $config.maxReplicas `
                --set-env-vars $envVars `
                --revision-suffix $tag | Out-Null
            Write-Log "Updated $appName with new image and settings."
        }
        catch {
            Write-Error "Failed to update Container App $appName. Error: $_"
            continue
        }
    }
}

Write-Log "All services processed. Deployment complete."

# Display public URLs for all deployed services
Write-Log "Service Endpoints:"
foreach ($svc in $services) {
    $appName = "railway-$svc"
    try {
        $fqdn = az containerapp show --name $appName --resource-group $ResourceGroup --query "properties.configuration.ingress.fqdn" -o tsv
        if ($fqdn) {
            Write-Host "$appName : https://$fqdn"
        }
    }
    catch {}
}