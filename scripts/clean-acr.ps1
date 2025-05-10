# Set ACR name and number of tags to keep
$ACR_NAME = "railwayacr4601"
$TAGS_TO_KEEP = 2
$DRY_RUN = $false

# Login to ACR
Write-Host "Logging into Azure Container Registry: $ACR_NAME"
az acr login --name $ACR_NAME

# Get list of repositories in ACR
Write-Host "Retrieving repository list..."
$repositories = (az acr repository list --name $ACR_NAME --output tsv) | Where-Object { $_ }

# Process each repository
foreach ($repo in $repositories) {
    Write-Host "Cleaning repository: $repo"
    
    # Get list of tags in repository sorted by time in descending order
    $tags = (az acr repository show-tags --name $ACR_NAME --repository $repo --orderby time_desc --output tsv) | Where-Object { $_ }
    
    # Check if tags were found
    if ($null -eq $tags) {
        Write-Host "No tags found in repository $repo"
        continue
    }
    
    # Convert to array if it's not already
    if ($tags -is [string]) {
        $tagArray = @($tags)
    } else {
        $tagArray = $tags
    }
    
    # Check number of tags
    $tagCount = $tagArray.Count
    
    # If there are more tags than we want to keep, delete the old ones
    if ($tagCount -gt $TAGS_TO_KEEP) {
        Write-Host "Found $tagCount tags, keeping the $TAGS_TO_KEEP newest ones"
        
        # Delete old tags (except the newest ones we want to keep)
        for ($i = $TAGS_TO_KEEP; $i -lt $tagCount; $i++) {
            $tag = $tagArray[$i]
            if ($DRY_RUN) {
                Write-Host "[DRY RUN] Would delete tag: $tag from repository $repo"
            } else {
                Write-Host "Deleting tag: $tag from repository $repo"
                # Use the correct syntax: az acr repository delete --name ACR_NAME --image REPO:TAG
                az acr repository delete --name $ACR_NAME --image "$repo`:$tag" --yes
            }
        }
    } else {
        Write-Host "Only $tagCount tags found in repository $repo, no cleanup needed"
    }
}

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host "Clean completed at $timestamp!"
Write-Host "Summary: Processed $($repositories.Count) repositories"