@echo off
 
:Build
cls
  
   
if not exist tools\Cake\Cake.exe ( 
  	echo Installing Cake...
 	nuget.exe install Cake -OutputDirectory tools -ExcludeVersion -NonInteractive
	 	echo.
)
			 
SET BUILDMODE="Release"
IF NOT [%1]==[] (set BUILDMODE="%1")
			  
echo Starting Cake...
"tools\Cake\Cake.exe" "build.csx" "-verbosity=verbose" "-config=%BUILDMODE%"
			   
exit /b %errorlevel%
