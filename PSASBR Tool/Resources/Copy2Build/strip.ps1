get-childitem ".\EXTRACTED TEXTURES\*.png" | foreach { rename-item $_ $_.Name.Replace(" (Image 0)", "") }