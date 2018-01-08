@@echo off

:RunPowerShellScript
@@set POWERSHELL_BAT_ARGS=%*
@@if defined POWERSHELL_BAT_ARGS set POWERSHELL_BAT_ARGS=%POWERSHELL_BAT_ARGS:"=\"%
@@PowerShell -ExecutionPolicy RemoteSigned -Command Invoke-Expression $('$args=@(^&{$args} %POWERSHELL_BAT_ARGS%);'+[String]::Join([Environment]::NewLine,$((Get-Content '%~f0') -notmatch '^^@@^|^^:'))) & goto :EOF

{ 
    # Start PowerShell
    param ([int]$startingDepth=0)
    
    try
    {
        Add-Type -Path ..\Mzinga.Core\bin\Release\Mzinga.Core.dll | Out-Null
    }
    catch
    {
        Add-Type -Path Mzinga.Core.dll | Out-Null
    }
    
    if ($startingDepth -eq 0)
    {
        Write-Host "Depth`tCount`tTime`tKN/s"
    }
    
    [int] $depth = $startingDepth;
    
    while ($True)
    {
        $gameBoard = New-Object Mzinga.Core.GameBoard
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        [long] $count = $gameBoard.CalculatePerft($depth)
        $elapsed = $sw.Elapsed
        $kns = ($count / $elapsed.TotalMilliseconds).ToString("0.00")
        Write-Host "$depth`t$count`t$elapsed`t$kns"
        $depth++
    }
    
    # End PowerShell
}.Invoke($args)
