$ResourceLocation = "southcentralus"

New-AzureRmResourcegroup -Name "DevSub01_Network_RG" -Location $ResourceLocation -Verbose
New-AzureRmResourcegroup -Name "DevSub01_App01_RG" -Location $ResourceLocation -Verbose
New-AzureRmResourcegroup -Name "DevSub01_App02_RG" -Location $ResourceLocation -Verbose

$CoreNetworkResourceGroup = "Core_Infra_Network_RG"
$core_vnet_name  = "BJD-Core-VNet-001"

$appsub_vnet_name  = "DevSub01-VNet-001"
$appsub_vnet_network_range = "10.2.0.0/16"

$subnet_configs = @()
$core_vnet_subnets = @(
	@{ Name= 'App01'; Range = "10.2.1.0/24" }
	@{ Name= 'App02'; Range = "10.2.2.0/24" }
)

foreach( $subnet in $core_vnet_subnets) {
	$subnet_configs += New-AzureRmVirtualNetworkSubnetConfig -Name $subnet.Name -AddressPrefix $subnet.Range
}
New-AzureRmVirtualNetwork -Name $appsub_vnet_name -ResourceGroupName "DevSub01_Network_RG" -Location $ResourceLocation -AddressPrefix $appsub_vnet_network_range  -Subnet $subnet_configs
Set-AzureRMVnetDNSServer -ResourceGroupName "DevSub01_Network_RG" -VnetName "DevSub01-VNet-001" -PrimaryDnsServerAddress 10.1.1.4

$core_vnet = Get-AzureRmVirtualNetwork -ResourceGroupName $CoreNetworkResourceGroup -Name $core_vnet_name
$depent_vnet = Get-AzureRmVirtualNetwork -ResourceGroupName $network_resource_group -Name $appsub_vnet_name 

Add-AzureRmVirtualNetworkPeering -name ("{0}-Peering" -f $appsub_vnet_name) -VirtualNetwork $core_vnet -RemoteVirtualNetworkId $depent_vnet.id -AllowForwardedTraffic -AllowGatewayTransit 
Add-AzureRmVirtualNetworkPeering -name "Core-Peering" -VirtualNetwork $depent_vnet -RemoteVirtualNetworkId $core_vnet.id -AllowForwardedTraffic -UseRemoteGateways
