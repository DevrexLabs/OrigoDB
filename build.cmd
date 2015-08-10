@echo off
 
:Build
cls
     
if not exist tools\Cake\Cake.exe ( 
  	echo Installing Cake...
 	tools\nuget\nuget.exe install Cake -OutputDirectory tools -ExcludeVersion -NonInteractive
 	echo.
)

if not exist tools\NUnit.Runners\tools\nunit-console.exe (
	echo Installing NUnit runners...
	tools\nuget\nuget.exe install nunit.runners -OutputDirectory tools -ExcludeVersion -NonInteractive
	echo.
)
			  
echo Starting Cake...
"tools\Cake\Cake.exe" "build.cake" "-verbosity=verbose"
			   
exit /b %errorlevel%
