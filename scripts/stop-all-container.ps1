$resourceGroup = "railway-resource"

$containerApps = az containerapp list --resource-group $resourceGroup --query "[].name" -o tsv

foreach ($app in $containerApps) {
    Write-Host "Checking active revision for: $app"

    $revisions = az containerapp revision list `
        --name $app `
        --resource-group $resourceGroup `
        --query "[?properties.active==true].name" -o tsv

    foreach ($revision in $revisions) {
        Write-Host "Stopping revision $revision of $app..."
        az containerapp revision deactivate `
            --name $app `
            --resource-group $resourceGroup `
            --revision $revision
        Write-Host "Stopped revision $revision of $app`n"
    }

    if (-not $revisions) {
        Write-Host "No active revision found for $app`n"
    }
}
