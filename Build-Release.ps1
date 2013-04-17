param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("\d\.\d\.\d\.\d")]
	[string]
	$ReleaseVersionNumber
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $SolutionRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Releases\v$ReleaseVersionNumber";
if ((Get-Item $ReleaseFolder -ErrorAction SilentlyContinue) -ne $null)
{
	Write-Warning "$ReleaseFolder already exists on your local machine. It will now be deleted."
	Remove-Item $ReleaseFolder -Recurse
}

# Set the version number in SolutionInfo.cs
$SolutionInfoPath = Join-Path -Path $SolutionRoot -ChildPath "SolutionInfo.cs"
(gc -Path $SolutionInfoPath) `
	-replace "(?<=Version\(`")[.\d]*(?=`"\))", $ReleaseVersionNumber |
	sc -Path $SolutionInfoPath -Encoding UTF8

# Build the solution in release mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.sln"
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

$CoreFolder = Join-Path -Path $ReleaseFolder -ChildPath "Core";
$MvcFolder = Join-Path -Path $ReleaseFolder -ChildPath "Mvc";

New-Item $CoreFolder -Type directory
New-Item $MvcFolder -Type directory

$CoreBinFolder = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.Core\bin\Release";
Copy-Item "$CoreBinFolder\*.dll" -Destination $CoreFolder

$MvcBinFolder = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.Mvc\bin\Release";
Copy-Item "$MvcBinFolder\*.dll" -Destination $MvcFolder -Include "ClientDependency.Core.Mvc.dll" 

$CoreNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "ClientDependency.nuspec";
Copy-Item $CoreNuSpecSource -Destination $CoreFolder
Copy-Item "$BuildFolder\nuget-transforms\Core\web.config.transform" -Destination (New-Item (Join-Path -Path $CoreFolder -ChildPath "nuget-transforms") -Type directory);
Copy-Item "$BuildFolder\nuget-transforms\Mvc\web.config.transform" -Destination (New-Item (Join-Path -Path $MvcFolder -ChildPath "nuget-transforms") -Type directory);

$CoreNuSpec = Join-Path -Path $CoreFolder -ChildPath "ClientDependency.nuspec";

$NuGet = Join-Path $SolutionRoot -ChildPath "Dependencies\NuGet.exe" 
& $NuGet pack $CoreNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber


$MvcNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "ClientDependency-Mvc.nuspec";
Copy-Item $MvcNuSpecSource -Destination $MvcFolder

$MvcNuSpec = Join-Path -Path $MvcFolder -ChildPath "ClientDependency-Mvc.nuspec"
(gc -Path (Join-Path -Path $MvcFolder -ChildPath "ClientDependency-Mvc.nuspec")) `
	-replace "(?<=dependency id=`"ClientDependency`" version=`")[.\d]*(?=`")", $ReleaseVersionNumber |
	sc -Path $MvcNuSpec -Encoding UTF8
  
$NuGet = Join-Path $SolutionRoot -ChildPath "Dependencies\NuGet.exe"
& $NuGet pack $MvcNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber

""
"Build $ReleaseVersionNumber is done!"
"NuGet packages also created, so if you want to push them just run:"
"  nuget push $CoreNuSpec"
"  nuget push $MvcNuSpec"