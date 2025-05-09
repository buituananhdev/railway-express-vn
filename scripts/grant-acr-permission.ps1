# Configuration
$resourceGroup = "railway-resource"
$acrName = "railwayacr4601"
$containerApps = @(
    "railway-admin-api",
    "railway-api-gateway",
    "railway-booking-api",
    "railway-notification-api",
    "railway-payment-api",
    "railway-auth-api",
    "railway-usermanagement-api"
)

# Verify current Azure context and permissions
try {
    $currentContext = az account show --query name -o tsv
    Write-Host "Current Azure context: $currentContext"
    
    # Ensure you're using the right subscription if working with multiple
    $subscriptionId = az account show --query id -o tsv
    Write-Host "Using subscription: $subscriptionId"
    
    # Check if the resource group exists
    $rgCheck = az group show --name $resourceGroup 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Resource group '$resourceGroup' not found. Please check your configuration."
        exit 1
    }
    
    # Check if the ACR exists
    $acrCheck = az acr show --name $acrName --resource-group $resourceGroup 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "ACR '$acrName' not found in resource group '$resourceGroup'. Please check your configuration."
        exit 1
    }
    
    # Get the ACR resource ID
    $acrId = az acr show --name $acrName --resource-group $resourceGroup --query id -o tsv
    Write-Host "ACR Resource ID: $acrId"
} 
catch {
    Write-Error "Failed to verify Azure context and resources: $_"
    exit 1
}

# Process each container app
foreach ($app in $containerApps) {
    try {
        Write-Host "Processing container app: $app..." -ForegroundColor Cyan
        
        # Check if the container app exists
        $appCheck = az containerapp show --name $app --resource-group $resourceGroup 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Container app '$app' not found. Skipping..."
            continue
        }
        
        # Check if managed identity is already assigned
        $identityStatus = az containerapp show --name $app --resource-group $resourceGroup --query "identity.type" -o tsv
        
        if ($identityStatus -ne "SystemAssigned") {
            Write-Host "  Assigning managed identity to: $app..."
            az containerapp identity assign --name $app --resource-group $resourceGroup --system-assigned | Out-Null
            
            if ($LASTEXITCODE -ne 0) {
                Write-Error "  Failed to assign managed identity to '$app'. Skipping role assignment."
                continue
            }
        } else {
            Write-Host "  Managed identity already assigned to: $app"
        }
        
        # Get the principal ID of the assigned identity
        $principalId = az containerapp show --name $app --resource-group $resourceGroup --query identity.principalId -o tsv
        
        if ([string]::IsNullOrWhiteSpace($principalId)) {
            Write-Error "  Could not retrieve principal ID for '$app'. Skipping role assignment."
            continue
        }
        
        Write-Host "  Retrieved principal ID: $principalId"
        
        # Check if the role assignment already exists
        $existingAssignment = az role assignment list --assignee $principalId --role "AcrPull" --scope $acrId --query "[].id" -o tsv
        
        if ([string]::IsNullOrWhiteSpace($existingAssignment)) {
            Write-Host "  Assigning AcrPull role to $app..."
            az role assignment create --assignee $principalId --role "AcrPull" --scope $acrId | Out-Null
            
            if ($LASTEXITCODE -ne 0) {
                Write-Error "  Failed to assign AcrPull role to '$app'."
                continue
            }
        } else {
            Write-Host "  AcrPull role already assigned to $app"
        }
        
        Write-Host "  Completed processing for $app" -ForegroundColor Green
    }
    catch {
        Write-Error "An error occurred while processing '$app': $_"
    }
}

Write-Host "Script execution completed." -ForegroundColor Green