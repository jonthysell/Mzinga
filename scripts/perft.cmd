@@echo off

:RunPowerShellScript
@@set POWERSHELL_BAT_ARGS=%*
@@if defined POWERSHELL_BAT_ARGS set POWERSHELL_BAT_ARGS=%POWERSHELL_BAT_ARGS:"=\"%
@@PowerShell -ExecutionPolicy RemoteSigned -Command Invoke-Expression $('$args=@(^&{$args} %POWERSHELL_BAT_ARGS%);'+[String]::Join([Environment]::NewLine,$((Get-Content '%~f0') -notmatch '^^@@^|^^:'))) & goto :EOF

{ 
    # Start PowerShell
    param ([int]$startingDepth=0)
    
    Add-Type -Path Mzinga.Core.dll | Out-Null
    
    function perft($gb, $depth)
    {
        if ($depth -eq 0)
        {
            return 1
        }

        [Mzinga.Core.MoveSet] $validMoves = $gb.GetValidMoves()
        
        if ($depth -eq 1)
        {
            return $validMoves.Count
        }

        [long] $nodes = 0
        
        foreach ($move in $validMoves)
        {
            $gb.Play($move)
            $nodes += perft -gb $gb -depth ($depth - 1)
            $gb.UndoLastMove()
        }

        return $nodes
    }
    
    if ($startingDepth -eq 0)
    {
        Write-Host "depth,count,time (ms)"
    }
    
    [int] $depth = $startingDepth;
    
    while ($True)
    {
        $gameBoard = New-Object Mzinga.Core.GameBoard
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        [long] $count = perft -gb $gameBoard -depth $depth
        $elapsed = $sw.Elapsed.TotalMilliseconds
        Write-Host "$depth,$count,$elapsed"
        $depth++
    }
    
    # End PowerShell
}.Invoke($args)
