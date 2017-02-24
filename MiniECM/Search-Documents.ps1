param(
    [string] $search
)

$headers = @{}
$headers.Add('api-key', '')
Invoke-RestMethod -Method Get -Uri ('https://bjdsearch.search.windows.net/indexes/docs/docs?api-version=2015-02-28&search={0}" -f $serach) -Headers $headers