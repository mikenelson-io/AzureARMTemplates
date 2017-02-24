
$ResourceGroupName = ""
$StorageAccountPrefix = ""

foreach( $index in 0,1,2 ) {
    $StorageAccountName = "{0}{1}" -f $StorageAccountPrefix, $index
    $keys = Get-AzureRmStorageAccountKey -ResourceGroupName  $ResourceGroupName -Name $StorageAccountName
    $PrimaryKey = $keys | Where KeyName -ieq "Key1" | Select -ExpandProperty Value
    $context = New-AzureStorageContext -StorageAccountName $StorageAccountName -StorageAccountKey $PrimaryKey

    Write-Output ("Creating Context for {0}" -f  $StorageAccountName) 

    if( $index -eq 0 ) {
        Write-Output ("Creating Containers for {0}" -f  $StorageAccountName)
        1 .. 3  | %  { New-AzureStorageContainer -Name ("clients-{0}0-{0}9" -f $_) -Permission Off -Context $context }
    }
    elseif( $index -eq 1 ) {
        Write-Output ("Creating Containers for {0}" -f  $StorageAccountName)
        4 .. 6  | %  { New-AzureStorageContainer -Name ("clients-{0}0-{0}9" -f $_) -Permission Off -Context $context }
    }
    elseif( $index -eq 2 ) {
        Write-Output ("Creating Containers for {0}" -f  $StorageAccountName)
        7 .. 9  | %  { New-AzureStorageContainer -Name ("clients-{0}0-{0}9" -f $_) -Permission Off -Context $context }
    }
}