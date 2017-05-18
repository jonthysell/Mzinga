@@echo off
:CheckPowerShellExecutionPolicy
@@FOR /F "tokens=*" %%i IN ('powershell -noprofile -command Get-ExecutionPolicy') DO Set PSExecMode=%%i
@@if /I "%PSExecMode%"=="unrestricted" goto :RunPowerShellScript
@@if /I "%PSExecMode%"=="remotesigned" goto :RunPowerShellScript
 
@@NET FILE 1>NUL 2>NUL
@@if not "%ERRORLEVEL%"=="0" (
@@echo Elevation required to change PowerShell execution policy from [%PSExecMode%] to RemoteSigned
@@powershell -NoProfile -Command "start-process -Wait -Verb 'RunAs' -FilePath 'powershell.exe' -ArgumentList '-NoProfile Set-ExecutionPolicy RemoteSigned'"
@@) else (
@@powershell -NoProfile Set-ExecutionPolicy RemoteSigned
@@)
 
:RunPowerShellScript
@@set POWERSHELL_BAT_ARGS=%*
@@if defined POWERSHELL_BAT_ARGS set POWERSHELL_BAT_ARGS=%POWERSHELL_BAT_ARGS:"=\"%
@@PowerShell -Command Invoke-Expression $('$args=@(^&{$args} %POWERSHELL_BAT_ARGS%);'+[String]::Join([Environment]::NewLine,$((Get-Content '%~f0') -notmatch '^^@@^|^^:'))) & goto :EOF

{ 
    # Start PowerShell
    
    Add-Type -Path Mzinga.Core.dll | Out-Null
    
    function perft($gb, $depth)
    {
        if ($depth -eq 0)
        {
            return 1
        }
        else
        {
            [int] $nodes = 0

            [Mzinga.Core.MoveSet] $validMoves = $gb.GetValidMoves()
            
            foreach ($move in $validMoves)
            {
                $gb.Play($move)
                $nodes += perft -gb $gb -depth ($depth - 1)
                $gb.UndoLastMove()
            }

            return $nodes
        }
    }
    
    Write-Host "depth,count,time (ms)"
    
    [int] $depth = 0;
    
    while ($True)
    {
        $gameBoard = New-Object Mzinga.Core.GameBoard
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        [int] $count = perft -gb $gameBoard -depth $depth
        $elapsed = $sw.Elapsed.TotalMilliseconds
        Write-Host "$depth,$count,$elapsed"
        $depth++
    }
    
    # End PowerShell
}.Invoke($args)
