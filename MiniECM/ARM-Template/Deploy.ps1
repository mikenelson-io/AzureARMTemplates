param (
	[Parameter(Mandatory=$true)]
    [string]       $ResourceGroupName          = "Template-Demo",
    [string]       $ResourceLocation           = "SouthCentral US"
) 

$opts = @{
    Name                  = ("Deployment-{0}-{1}" -f $ResourceGroupName, $(Get-Date).ToString("yyyyMMddhhmmss"))
    ResourceGroupName     = $ResourceGroupName
    TemplateFile          = (Join-Path -Path $PWD.Path -ChildPath "azuredeploy.json")
    TemplateParameterFile = (Join-Path -Path $PWD.Path -ChildPath "azuredeploy.parameters.json")
}

New-AzureRmResourcegroup -Name $ResourceGroupName -Location $ResourceLocation -Verbose
New-AzureRmResourceGroupDeployment @opts -verbose   