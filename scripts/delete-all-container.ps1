$resourceGroup = "railway-resource"

$containerApps = az containerapp list --resource-group $resourceGroup --query "[].name" -o tsv

foreach ($app in $containerApps) {
    Write-Host "Deleting Container App: $app"
    az containerapp delete --name $app --resource-group $resourceGroup --yes
}
