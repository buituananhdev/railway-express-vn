# Configuration
$resourceGroup = "railway-resource"
$keyVaultName = "railway-vault"
$containerApps = @(
    "railway-admin-api",
    "railway-api-gateway",
    "railway-booking-api",
    "railway-notification-api",
    "railway-payment-api",
    "railway-auth-api",
    "railway-usermanagement-api"
)

# Verify current Azure context and resources
try {
    # Get the current subscription ID and name
    $subscriptionId = az account show --query id -o tsv
    $subscriptionName = az account show --query name -o tsv
    
    Write-Host "Current Azure context: $subscriptionName ($subscriptionId)" -ForegroundColor Cyan
    
    # Check if the resource group exists
    $rgCheck = az group show --name $resourceGroup 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Resource group '$resourceGroup' not found. Please check your configuration."
        exit 1
    }
    
    # Check if the Key Vault exists
    $kvCheck = az keyvault show --name $keyVaultName --resource-group $resourceGroup 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Key Vault '$keyVaultName' not found in resource group '$resourceGroup'. Please check your configuration."
        exit 1
    }
    
    # Get the Key Vault resource ID
    $keyVaultScope = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup/providers/Microsoft.KeyVault/vaults/$keyVaultName"
    Write-Host "Key Vault Resource ID: $keyVaultScope" -ForegroundColor Cyan
} 
catch {
    Write-Error "Failed to verify Azure context and resources: $_"
    exit 1
}

# Process each container app
$processedCount = 0
$skippedCount = 0
$errorCount = 0

foreach ($app in $containerApps) {
    try {
        Write-Host "`nProcessing container app: $app..." -ForegroundColor Cyan
        
        # Check if the container app exists
        $appCheck = az containerapp show --name $app --resource-group $resourceGroup 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Container app '$app' not found. Skipping..."
            $skippedCount++
            continue
        }
        
        # Get the principal ID of the assigned identity
        $identity = az containerapp show --name $app --resource-group $resourceGroup --query "identity.principalId" --output tsv
        
        if ([string]::IsNullOrWhiteSpace($identity)) {
            Write-Warning "No managed identity found for '$app'. Make sure you've assigned one first. Skipping..."
            $skippedCount++
            continue
        }
        
        Write-Host "  Retrieved identity principal ID: $identity"
        
        # Check if the role assignment already exists
        $existingAssignment = az role assignment list --assignee $identity --role "Key Vault Secrets User" --scope $keyVaultScope --query "[].id" -o tsv
        
        if ([string]::IsNullOrWhiteSpace($existingAssignment)) {
            Write-Host "  Assigning 'Key Vault Secrets User' role to $app..."
            
            $roleAssignment = az role assignment create --assignee $identity --role "Key Vault Secrets User" --scope $keyVaultScope 2>&1
            
            if ($LASTEXITCODE -ne 0) {
                Write-Error "  Failed to assign role to '$app': $roleAssignment"
                $errorCount++
                continue
            }
            
            Write-Host "  Successfully assigned 'Key Vault Secrets User' role to $app" -ForegroundColor Green
        } else {
            Write-Host "  'Key Vault Secrets User' role already assigned to $app" -ForegroundColor Yellow
        }
        
        $processedCount++
    }
    catch {
        Write-Error "An error occurred while processing '$app': $_"
        $errorCount++
    }
}

# Summary report
Write-Host "`n====== Summary ======" -ForegroundColor Cyan
Write-Host "Total container apps processed: $processedCount" -ForegroundColor Green
if ($skippedCount -gt 0) {
    Write-Host "Skipped container apps: $skippedCount" -ForegroundColor Yellow
}
if ($errorCount -gt 0) {
    Write-Host "Failed container apps: $errorCount" -ForegroundColor Red
}
Write-Host "======================" -ForegroundColor Cyan