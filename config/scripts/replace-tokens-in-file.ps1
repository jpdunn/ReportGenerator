param(
	[Parameter(Mandatory = $true)][string] $Token,
	[Parameter(Mandatory = $true)][string] $Value,
	[Parameter(Mandatory = $true)][string] $Path,
	[Parameter(Mandatory = $true)] $Include,
	[Parameter(Mandatory = $false)][bool] $Recurse = $false
)

$files = $null

if ($Recurse) {
	$files = Get-ChildItem -Path $Path -Include $Include -Recurse
}
else {
	$files = Get-ChildItem -Path $Path -Include $Include
}

$files | ForEach-Object {
	Write-Host "Replacing token '$Token' with value '$Value' in file '$_'"
	$_.IsReadOnly = $false
	(Get-Content -Path $_) -replace $Token, $Value |
	Set-Content -Path $_
}