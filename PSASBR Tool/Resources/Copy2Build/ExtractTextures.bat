FOR %%a IN ("GXT\*.gxt") DO ("Resources\ScarletTestApp.exe" %%a & MOVE "GXT\*.png" "EXTRACTED TEXTURES")

DEL "GXT\*.gxt"

echo Stripping the Image 0 string...

Powershell.exe -executionpolicy remotesigned -File  .\Resources\strip.ps1