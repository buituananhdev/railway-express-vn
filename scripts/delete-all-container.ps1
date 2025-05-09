$resourceGroup = "railway-resource"

$containerApps = @(
    "railway-admin-api",
    "railway-api-gateway",
    "railway-booking-api",
    "railway-notification-api",
    "railway-payment-api",
    "railway-auth-api",
    "railway-usermanagement-api"
)

foreach ($app in $containerApps) {
    Write-Host "Deleting Container App: $app"
    az containerapp delete --name $app --resource-group $resourceGroup --yes
}
