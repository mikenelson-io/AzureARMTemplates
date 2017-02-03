$ResourceLocation = "southcentralus"

New-AzureRmResourcegroup -Name "Core_Infra_Network_RG" -Location $ResourceLocation -Verbose
New-AzureRmResourcegroup -Name "Core_Infra_AD_RG" -Location $ResourceLocation -Verbose
New-AzureRmResourcegroup -Name "Core_Infra_Mgmt_RG" -Location $ResourceLocation -Verbose
New-AzureRmResourcegroup -Name "Core_Infra_Storage_RG" -Location $ResourceLocation -Verbose
New-AzureRmResourcegroup -Name "Core_Infra_Automation_RG" -Location $ResourceLocation -Verbose
New-AzureRmResourcegroup -Name "Core_Infra_KeyVault_RG" -Location $ResourceLocation -Verbose
New-AzureRmResourcegroup -Name "Core_Infra_BackupVault_RG" -Location $ResourceLocation -Verbose

New-AzureRmStorageAccount -ResourceGroupName "Core_Infra_Storage_RG" -AccountName "bjdcoresa001" -Location $ResourceLocation -SkuName "Standard_GRS" -Kind "Storage"
New-AzureRMAutomationAccount -ResourceGroupName "Core_Infra_Automation_RG" -Name "Automation" -Location $ResourceLocation -Plan Free -Verbose
New-AzureRmKeyVault -ResourceGroupName "Core_Infra_KeyVault_RG" -VaultName "BjdCoreKeyVault" -Location $ResourceLocation -EnabledForDiskEncryption -EnabledForDeployment -EnabledForTemplateDeployment
New-AzureRmRecoveryServicesVault -ResourceGroupName "Core_Infra_BackupVault_RG" -Name "bjdCoreBackupVault" -Location $ResourceLocation
New-AzureRmOperationalInsightsWorkspace -ResourceGroupName "Core_Infra_BackupVault_RG" -Name "bjdCoreLogVault" -Location "East US" -Sku "Free"

$CoreNetworkResourceGroup = "Core_Infra_Network_RG"

$core_vnet_name  = "BJD-Core-VNet-001"
$core_vnet_network_range = "10.1.0.0/16"

$subnet_configs = @()
$core_vnet_subnets = @(
	@{ Name= 'AD'; Range = "10.1.1.0/24" }
	@{ Name= 'Utils'; Range = "10.1.2.0/24" }
	@{ Name= 'Mgmt'; Range = "10.1.3.0/24" }
	@{ Name= 'GatewaySubnet'; Range = "10.1.250.0/28" }
)

foreach( $subnet in $core_vnet_subnets) {
	$subnet_configs += New-AzureRmVirtualNetworkSubnetConfig -Name $subnet.Name -AddressPrefix $subnet.Range
}
New-AzureRmVirtualNetwork -Name $core_vnet_name -ResourceGroupName $CoreNetworkResourceGroup -Location $ResourceLocation -AddressPrefix $core_vnet_network_range -Subnet $subnet_configs

Set-AzureRMVnetDNSServer -ResourceGroupName Core_Infra_Network_RG -VnetName "BJD-Core-VNet-001" -PrimaryDnsServerAddress 10.1.1.4

$nsg_rules_config = @()
$nsg_rules = @()

$nsg_rules += @{
	Name = "RDP"
	Access = "Allow"
	Protocol = "Tcp"
	Direction = "Inbound"
	Priority = 100
	SourceAddressPrefix = "Internet"
	SourcePortRange = "*"
	DestinationAddressPrefix = "*"
	DestinationPortRange = "3389"
}
$nsg_rules += @{
	Name = "SSH"
	Access = "Allow"
	Protocol = "Tcp"
	Direction = "Inbound"
	Priority = 100
	SourceAddressPrefix = "Internet"
	SourcePortRange = "*"
	DestinationAddressPrefix = "*"
	DestinationPortRange = "22"
}
foreach( $rule in $nsg_rules ) {
	$nsg_rules_config = New-AzureRmNetworkSecurityRuleConfig @rule 
}
$nsg = New-AzureRmNetworkSecurityGroup -ResourceGroupName $CoreNetworkResourceGroup -Location $ResourceLocation -Name  "JumpServers-NSG-Rule" -SecurityRules $nsg_rules_config

$vnet = Get-AzureRmVirtualNetwork -Name $core_vnet_name -ResourceGroupName $CoreNetworkResourceGroup
$jump_subnet = Get-AzureRmVirtualNetworkSubnetConfig -Name 'Mgmt' -VirtualNetwork $vnet
Set-AzureRmVirtualNetworkSubnetConfig -NetworkSecurityGroup $nsg -Name 'mgmt' -VirtualNetwork $vnet -AddressPrefix $jump_subnet.AddressPrefix
Set-AzureRmVirtualNetwork -VirtualNetwork $vnet

$VPNClientAddressPool = "172.16.201.0/24"
$GWName = "{0}-Gateway" -f $core_vnet_name
$GWIPName = "Gateway-PIP"
$GWIPconfName = "Gateway-IpConfiguration"
$P2SRootCertName = "BJDVPN.cer"

$vnet = Get-AzureRmVirtualNetwork -Name $core_vnet_name -ResourceGroupName $CoreNetworkResourceGroup
$subnet = Get-AzureRmVirtualNetworkSubnetConfig -Name 'GatewaySubnet' -VirtualNetwork $vnet

$pip = New-AzureRmPublicIpAddress -Name $GWIPName -ResourceGroupName $CoreNetworkResourceGroup -Location $ResourceLocation -AllocationMethod Dynamic
$ipconf = New-AzureRmVirtualNetworkGatewayIpConfig -Name $GWIPconfName -Subnet $subnet -PublicIpAddress $pip

$cert = Get-ChildItem -Path "cert:\CurrentUser\my" | where Subject -imatch "CN=BJDVPN"
$MyP2SRootCertPubKeyBase64 = [convert]::ToBase64String($cert.Export("cer", [string]::Empty))
$p2srootcert = New-AzureRmVpnClientRootCertificate -Name $P2SRootCertName -PublicCertData $MyP2SRootCertPubKeyBase64

$opts = @{
	Name                 		= $GWName
	ResourceGroupName    		= $CoreNetworkResourceGroup 
	Location             		= $ResourceLocation
	IpConfigurations     		= $ipconf
	GatewayType          		= "Vpn" 
	VpnType              		= "RouteBased"
	EnableBgp            		= $false
	GatewaySku                 	= "Standard"
	VpnClientAddressPool       	= $VPNClientAddressPool
	VpnClientRootCertificates  	= $p2srootcert
}
New-AzureRmVirtualNetworkGateway @opts