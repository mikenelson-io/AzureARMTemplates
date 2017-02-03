param (
    [string] $ResourceGroupName          = "Core_Infra_KeyVault_RG",
    [string] $ResourceLocation           = "southcentralus",
    [string] $KeyVaultName               = "BjdCoreKeyVault",
    [string] $KeyVaultSecret             = "managerPassword",
    
    [Parameter(Mandatory=$true)]
    [string] $KeyVaultSecretValue        
)  

$secretvalue = ConvertTo-SecureString -String $KeyVaultSecretValue -AsPlainText -Force
Set-AzureKeyVaultSecret -VaultName $KeyVaultName -Name $KeyVaultSecret -SecretValue $secretvalue
 
Write-Output "Add to Parameter File for the localAdminPassword Variable:"
Write-Output ("SecretName  - {0}" -f $KeyVaultSecret)
Write-Output ("KeyVault Id - {0}" -f ("/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.KeyVault/vaults/{2}" -f (Get-AzureRmSubscription).SubscriptionId, $ResourceGroupName, $KeyVaultName)) 