param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("^\d\.\d\.(?:\d\.\d$|\d$)")]
	[string]
	$ReleaseVersionNumber,
	[Parameter(Mandatory=$true)]	
	[string]
	$PreReleaseName
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $SolutionRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Releases\v$ReleaseVersionNumber$PreReleaseName";
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
$LessFolder = Join-Path -Path $ReleaseFolder -ChildPath "Less";
$SassFolder = Join-Path -Path $ReleaseFolder -ChildPath "SASS";
$CoffeeFolder = Join-Path -Path $ReleaseFolder -ChildPath "Coffee";
$TypeScriptFolder = Join-Path -Path $ReleaseFolder -ChildPath "TypeScript";

New-Item $CoreFolder -Type directory
New-Item $MvcFolder -Type directory
New-Item $LessFolder -Type directory
New-Item $SassFolder -Type directory
New-Item $CoffeeFolder -Type directory
New-Item $TypeScriptFolder -Type directory

$include = @('ClientDependency.Core.dll','ClientDependency.Core.pdb')
$CoreBinFolder = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.Core\bin\Release";
Copy-Item "$CoreBinFolder\*.*" -Destination $CoreFolder -Include $include

$include = @('ClientDependency.Core.Mvc.dll','ClientDependency.Core.Mvc.pdb')
$MvcBinFolder = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.Mvc\bin\Release";
Copy-Item "$MvcBinFolder\*.*" -Destination $MvcFolder -Include $include

$include = @('ClientDependency.Less.dll','ClientDependency.Less.pdb')
$LessBinFolder = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.Less\bin\Release";
Copy-Item "$LessBinFolder\*.*" -Destination $LessFolder -Include $include

$include = @('ClientDependency.SASS.dll','ClientDependency.SASS.pdb')
$SassBinFolder = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.SASS\bin\Release";
Copy-Item "$SassBinFolder\*.*" -Destination $SassFolder -Include $include

$include = @('ClientDependency.Coffee.dll','ClientDependency.Coffee.pdb')
$CoffeeBinFolder = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.Coffee\bin\Release";
Copy-Item "$CoffeeBinFolder\*.*" -Destination $CoffeeFolder -Include $include

$include = @('ClientDependency.TypeScript.dll','ClientDependency.TypeScript.pdb')
$TypeScriptBinFolder = Join-Path -Path $SolutionRoot -ChildPath "ClientDependency.TypeScript\bin\Release";
Copy-Item "$TypeScriptBinFolder\*.*" -Destination $TypeScriptFolder -Include $include

# COPY THE TRANSFORMS OVER
Copy-Item "$BuildFolder\nuget-transforms\Core\web.config.transform" -Destination (New-Item (Join-Path -Path $CoreFolder -ChildPath "nuget-transforms") -Type directory);
Copy-Item "$BuildFolder\nuget-transforms\Mvc\web.config.transform" -Destination (New-Item (Join-Path -Path $MvcFolder -ChildPath "nuget-transforms") -Type directory);
Copy-Item "$BuildFolder\nuget-transforms\Less\web.config.transform" -Destination (New-Item (Join-Path -Path $LessFolder -ChildPath "nuget-transforms") -Type directory);
Copy-Item "$BuildFolder\nuget-transforms\Coffee\web.config.transform" -Destination (New-Item (Join-Path -Path $CoffeeFolder -ChildPath "nuget-transforms") -Type directory);
Copy-Item "$BuildFolder\nuget-transforms\Sass\web.config.transform" -Destination (New-Item (Join-Path -Path $SassFolder -ChildPath "nuget-transforms") -Type directory);
Copy-Item "$BuildFolder\nuget-transforms\TypeScript\web.config.transform" -Destination (New-Item (Join-Path -Path $TypeScriptFolder -ChildPath "nuget-transforms") -Type directory);

# COPY OVER THE CORE NUSPEC AND BUILD THE NUGET PACKAGE
$CoreNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "ClientDependency.nuspec";
Copy-Item $CoreNuSpecSource -Destination $CoreFolder
$CoreNuSpec = Join-Path -Path $CoreFolder -ChildPath "ClientDependency.nuspec";
$NuGet = Join-Path $SolutionRoot -ChildPath ".nuget\NuGet.exe"
Write-Output "DEBUGGING: " $CoreNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName
& $NuGet pack $CoreNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName

# COPY OVER THE MVC NUSPEC AND BUILD THE NUGET PACKAGE
$MvcNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "ClientDependency-Mvc.nuspec";
Copy-Item $MvcNuSpecSource -Destination $MvcFolder
$MvcNuSpec = Join-Path -Path $MvcFolder -ChildPath "ClientDependency-Mvc.nuspec"
$NuGet = Join-Path $SolutionRoot -ChildPath ".nuget\NuGet.exe"
& $NuGet pack $MvcNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName

# COPY OVER THE LESS NUSPEC AND BUILD THE NUGET PACKAGE
$LessNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "ClientDependency-Less.nuspec";
Copy-Item $LessNuSpecSource -Destination $LessFolder
$LessNuSpec = Join-Path -Path $LessFolder -ChildPath "ClientDependency-Less.nuspec"
$NuGet = Join-Path $SolutionRoot -ChildPath ".nuget\NuGet.exe"
& $NuGet pack $LessNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName

# COPY OVER THE SASS NUSPEC AND BUILD THE NUGET PACKAGE
$SassNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "ClientDependency-SASS.nuspec";
Copy-Item $SassNuSpecSource -Destination $SassFolder
$SassNuSpec = Join-Path -Path $SassFolder -ChildPath "ClientDependency-SASS.nuspec"
$NuGet = Join-Path $SolutionRoot -ChildPath ".nuget\NuGet.exe"
& $NuGet pack $SassNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName

# COPY OVER THE COFFEE NUSPEC AND BUILD THE NUGET PACKAGE
$CoffeeNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "ClientDependency-Coffee.nuspec";
Copy-Item $CoffeeNuSpecSource -Destination $CoffeeFolder
$CoffeeNuSpec = Join-Path -Path $CoffeeFolder -ChildPath "ClientDependency-Coffee.nuspec"
$NuGet = Join-Path $SolutionRoot -ChildPath ".nuget\NuGet.exe"
& $NuGet pack $CoffeeNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName

# COPY OVER THE TypeScript NUSPEC AND BUILD THE NUGET PACKAGE
$TypeScriptNuSpecSource = Join-Path -Path $BuildFolder -ChildPath "ClientDependency-TypeScript.nuspec";
Copy-Item $TypeScriptNuSpecSource -Destination $TypeScriptFolder
$TypeScriptNuSpec = Join-Path -Path $TypeScriptFolder -ChildPath "ClientDependency-TypeScript.nuspec"
$NuGet = Join-Path $SolutionRoot -ChildPath ".nuget\NuGet.exe"
& $NuGet pack $TypeScriptNuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName

# NOT SURE WHAT THIS WAS DOING BUT SEEMS TO BE WORKING WITHOUT IT!
# (gc -Path (Join-Path -Path $MvcFolder -ChildPath "ClientDependency-Mvc.nuspec")) `
# 	-replace "(?<=dependency id=`"ClientDependency`" version=`")[.\d]*(?=`")", $ReleaseVersionNumber |
# 	sc -Path $MvcNuSpec -Encoding UTF8

""
"Build $ReleaseVersionNumber$PreReleaseName is done!"
"NuGet packages also created, so if you want to push them just run:"
"  nuget push $CoreNuSpec"
"  nuget push $MvcNuSpec"