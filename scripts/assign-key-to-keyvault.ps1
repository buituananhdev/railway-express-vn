param (
    [Parameter(Mandatory = $true)]
    [string]$vaultName,
    
    [Parameter(Mandatory = $true)]
    [string]$configFilePath,
    
    [switch]$DryRun
)

# Helper function to convert PSCustomObject to Hashtable (for PowerShell 5.1 compatibility)
function ConvertTo-Hashtable {
    param (
        [Parameter(Mandatory = $true)]
        [object]$InputObject
    )
    
    if ($InputObject -is [System.Collections.IEnumerable] -and $InputObject -isnot [string]) {
        $collection = @()
        foreach ($object in $InputObject) {
            $collection += ConvertTo-Hashtable -InputObject $object
        }
        return $collection
    } elseif ($InputObject -is [PSCustomObject]) {
        $hash = @{}
        foreach ($property in $InputObject.PSObject.Properties) {
            $hash[$property.Name] = ConvertTo-Hashtable -InputObject $property.Value
        }
        return $hash
    } else {
        return $InputObject
    }
}

# Verify Azure CLI and authentication
function Test-AzureCliAndAuth {
    try {
        $azVersion = az --version 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Azure CLI not installed or not in PATH"
        }
        
        $account = az account show 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Not authenticated to Azure. Run 'az login' first."
        }
        
        # Check if user has access to the specified vault
        $vaultCheck = az keyvault show --name $vaultName 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Cannot access Key Vault '$vaultName'. Check if it exists and you have permissions."
        }
        
        return $true
    }
    catch {
        Write-Error "Azure CLI check failed: $_"
        return $false
    }
}

function Flatten-Json {
    param (
        [Parameter(Mandatory = $true)]
        [object]$InputObject,
        [string]$Prefix = ""
    )

    $result = @{}

    if ($InputObject -is [System.Collections.IDictionary] -or $InputObject -is [PSCustomObject]) {
        $properties = if ($InputObject -is [PSCustomObject]) { 
            $InputObject.PSObject.Properties 
        } else { 
            $InputObject.Keys | ForEach-Object { [PSCustomObject]@{Name=$_; Value=$InputObject[$_]} }
        }
        
        foreach ($prop in $properties) {
            $key = $prop.Name
            $value = $prop.Value
            $newKey = if ($Prefix) { "$Prefix--$key" } else { $key }

            if ($value -is [System.Collections.IDictionary] -or $value -is [PSCustomObject]) {
                $nested = Flatten-Json -InputObject $value -Prefix $newKey
                foreach ($nestedKey in $nested.Keys) {
                    $result[$nestedKey] = $nested[$nestedKey]
                }
            }
            # Handle arrays specially
            elseif ($value -is [Array]) {
                # Convert array to JSON string
                $result[$newKey] = ($value | ConvertTo-Json -Compress -Depth 10)
            }
            else {
                $result[$newKey] = $value
            }
        }
    }
    return $result
}

# Load JSON configuration from external file
function Load-Config {
    param (
        [string]$FilePath
    )
    
    try {
        if (-not (Test-Path $FilePath)) {
            throw "Configuration file not found: $FilePath"
        }
        
        $fileContent = Get-Content -Path $FilePath -Raw
        
        # Check PowerShell version to handle JSON conversion appropriately
        if ($PSVersionTable.PSVersion.Major -ge 6) {
            # PowerShell 6+ supports AsHashtable parameter
            $config = $fileContent | ConvertFrom-Json -AsHashtable
        } else {
            # For PowerShell 5.1 and earlier, convert to PSCustomObject and then to hashtable
            $jsonObject = $fileContent | ConvertFrom-Json
            $config = ConvertTo-Hashtable -InputObject $jsonObject
        }
        
        return $config
    }
    catch {
        Write-Error "Failed to load configuration: $_"
        throw
    }
}

# Upload secrets to Azure Key Vault
function Upload-Secrets {
    param (
        [hashtable]$Secrets,
        [string]$VaultName,
        [bool]$IsDryRun
    )
    
    $successCount = 0
    $failureCount = 0
    $failures = @()
    
    foreach ($key in $Secrets.Keys) {
        try {
            $value = $Secrets[$key]
            
            # Skip empty values
            if ([string]::IsNullOrEmpty($value)) {
                Write-Warning "Skipping empty value for key: $key"
                continue
            }
            
            Write-Host "Processing secret: $key"
            
            if ($IsDryRun) {
                Write-Host "  [DRY RUN] Would set secret: $key (value hidden)" -ForegroundColor Cyan
            }
            else {
                # Replace characters not allowed in Key Vault secret names
                $safeKey = $key -replace '[^a-zA-Z0-9-]', '-'
                
                if ($safeKey -ne $key) {
                    Write-Warning "Key contains invalid characters. Using '$safeKey' instead of '$key'"
                }
                
                # Key Vault secret names can't end with a hyphen
                $safeKey = $safeKey -replace '-+$', ''
                
                # Set the secret
                $result = az keyvault secret set --vault-name $VaultName --name $safeKey --value $value 2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "  Secret uploaded: $safeKey" -ForegroundColor Green
                    $successCount++
                }
                else {
                    Write-Warning "  Failed to upload secret: $safeKey - $result"
                    $failureCount++
                    $failures += [PSCustomObject]@{
                        Key = $safeKey
                        Error = $result
                    }
                }
            }
        }
        catch {
            Write-Error "Error processing secret '$key': $_"
            $failureCount++
            $failures += [PSCustomObject]@{
                Key = $key
                Error = $_.Exception.Message
            }
        }
    }
    
    return [PSCustomObject]@{
        SuccessCount = $successCount
        FailureCount = $failureCount
        Failures = $failures
    }
}

# Main script execution
try {
    # Verify Azure CLI and authentication
    $azureReady = Test-AzureCliAndAuth
    if (-not $azureReady) {
        throw "Azure CLI check failed. Please ensure you have the Azure CLI installed and you're authenticated."
    }
    
    # Load configuration
    $config = Load-Config -FilePath $configFilePath
    
    # Flatten the configuration
    $flattened = Flatten-Json -InputObject $config
    
    Write-Host "`nPreparing to upload $($flattened.Count) secrets to Azure Key Vault: $vaultName`n"
    
    # Upload secrets
    $result = Upload-Secrets -Secrets $flattened -VaultName $vaultName -IsDryRun $DryRun
    
    # Display results
    Write-Host "`n---- Upload Summary ----"
    Write-Host "Vault: $vaultName"
    Write-Host "Total secrets processed: $($flattened.Count)"
    Write-Host "Successfully uploaded: $($result.SuccessCount)" -ForegroundColor Green
    
    if ($result.FailureCount -gt 0) {
        Write-Host "Failed uploads: $($result.FailureCount)" -ForegroundColor Red
        Write-Host "`nFailed secrets:"
        $result.Failures | Format-Table -Property Key, Error -AutoSize
    }
}
catch {
    Write-Error "Script execution failed: $_"
    exit 1
}