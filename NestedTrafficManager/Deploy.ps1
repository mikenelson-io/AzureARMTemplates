param (
	[Parameter(Mandatory=$true)]
    [string]       $ResourceGroupName          = "Template-Demo",

    [Parameter(Mandatory=$true)]
    [string]       $RootTrafficManagerName     = "bjd",

    [string]       $ResourceLocation           = "southcentralus"
)  
$opts = @{
    Name                  = ("Deployment-{0}-{1}" -f $ResourceGroupName, $(Get-Date).ToString("yyyyMMddhhmmss"))
    ResourceGroupName     = $ResourceGroupName
    TemplateFile          = (Join-Path -Path $PWD.Path -ChildPath "azuredeploy.json")
    TemplateParameterFile = (Join-Path -Path $PWD.Path -ChildPath "azuredeploy.parameters.json")
}

New-AzureRmResourcegroup -Name $ResourceGroupName -Location $ResourceLocation -Verbose
New-AzureRmResourceGroupDeployment @opts -verbose   

$app_services = Get-AzureRmWebApp -ResourceGroupName $ResourceGroupName 
$tm_url = "{0}.trafficmanager.net" -f $RootTrafficManagerName
foreach( $web in $app_services ) {
    Out-Host -InputObject ("[{0}] - Setting {1} HostName to include {2} . . ." -f $(Get-Date), $web.Name, $tm_url)
    $hostnames = $web.HostNames
    $hostnames += $tm_url
    Set-AzureRmWebApp -HostNames $hostnames -ResourceGroupName $ResourceGroupName -Name $web.Name       
}
