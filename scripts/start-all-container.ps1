$resourceGroup = "railway-resource"
$subscriptionId = "8ea5379c-0314-4694-a83d-eafe02fc4f36"

$containerApps = az containerapp list --resource-group $resourceGroup --query "[].name" -o tsv

foreach ($app in $containerApps) {
    Write-Host "Stopping app: $app..."
    
    Start-AzContainerApp -Name $app `
                        -ResourceGroupName $resourceGroup `
                        -SubscriptionId $subscriptionId

    Write-Host "Stopped app: $app`n"
}
