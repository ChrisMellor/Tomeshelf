function Get-CoberturaPackageCoverage {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$true, HelpMessage="Path to the Cobertura XML file.")]
        [string]$XmlFilePath
    )

    Write-Host "Checking for file: $XmlFilePath"
    if (-not (Test-Path $XmlFilePath)) {
        Write-Error "File not found: $XmlFilePath"
        return
    }
    Write-Host "File found."

    try {
        Write-Host "Reading XML content..."
        [xml]$coberturaXml = Get-Content $XmlFilePath -ErrorAction Stop
        Write-Host "XML content read."

        $packages = $coberturaXml.coverage.packages.package
        Write-Host "Found $($packages.Count) packages."

        if (-not $packages) {
            Write-Warning "No package information found in the Cobertura XML file."
            return
        }

        $results = @()
        foreach ($package in $packages) {
            $packageName = $package.name
            $lineRate = [double]$package.'line-rate'
            $lineCoveragePercentage = [math]::Round($lineRate * 100, 2)

            $results += [PSCustomObject]@{
                PackageName = $packageName
                LineCoverage = "$lineCoveragePercentage%"
            }
        }

        $results | Format-Table -AutoSize

    }
    catch {
        Write-Error "An error occurred while processing the XML file: $($_.Exception.Message)"
    }
}