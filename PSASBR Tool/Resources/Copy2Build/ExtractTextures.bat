FOR %%a IN ("Temp\*.gxt") DO ("Resources\ScarletTestApp.exe" %%a & MOVE "Temp\*.png" "EXTRACTED TEXTURES")

DEL "Temp\*.gxt"

echo Stripping the Image 0 string...

Powershell.exe -executionpolicy remotesigned -File  .\Resources\strip.ps1