[CmdletBinding()]
Param(
	[string]$Version
)

if (-Not $Version) {
	Write-Host -ForegroundColor Red "Please specify the version to publish"
	Write-Host -ForegroundColor Cyan -NoNewLine "USAGE: "
	Write-Host "publish-exceldata-cli.ps1 -version <version>"
    # nuget list datask-excel -source https://www.myget.org/F/jeevanjames/api/v3/index.json
    nuget list datask-excel
	exit -1
}

dotnet pack ./tool/ExcelData/Cli/Cli.csproj -c Release-Tool /p:Version=$Version
# dotnet nuget push ./tool/ExcelData/Cli/bin/Release-Tool/datask-excel.$Version.nupkg --source https://www.myget.org/F/jeevanjames/api/v2/package
dotnet nuget push ./tool/ExcelData/Cli/bin/Release-Tool/datask-excel.$Version.nupkg --source https://api.nuget.org/v3/index.json
