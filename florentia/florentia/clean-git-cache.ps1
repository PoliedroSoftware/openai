# Script para limpiar la caché de Git y aplicar el nuevo .gitignore
# Ejecutar este script en PowerShell para que Git reconozca los cambios en el .gitignore

Write-Host "?? Limpiando caché de Git..." -ForegroundColor Yellow

# Elimina los archivos del índice de Git (no borra los archivos físicamente)
git rm -r --cached . 2>$null

Write-Host "? Caché limpiada" -ForegroundColor Green

# Añade todos los archivos de nuevo, respetando el nuevo .gitignore
Write-Host "?? Aplicando nuevo .gitignore..." -ForegroundColor Yellow
git add .

Write-Host "? Nuevo .gitignore aplicado" -ForegroundColor Green

# Muestra el estado para verificar qué archivos serán incluidos
Write-Host "?? Estado actual:" -ForegroundColor Cyan
git status --short

Write-Host ""
Write-Host "?? Archivos que YA NO aparecerán en futuros commits:" -ForegroundColor Green
Write-Host "   • Carpetas bin/ y obj/"
Write-Host "   • Carpeta .vs/"
Write-Host "   • Archivos *.cache.json"
Write-Host "   • Archivos *.dll, *.exe, *.pdb"
Write-Host "   • Archivos de configuración local"
Write-Host ""
Write-Host "? Solo aparecerán archivos de código fuente esenciales" -ForegroundColor Green
Write-Host ""
Write-Host "Para confirmar los cambios ejecuta:" -ForegroundColor White -BackgroundColor DarkBlue
Write-Host "git commit -m 'Aplicando nuevo .gitignore - solo archivos esenciales'" -ForegroundColor White -BackgroundColor DarkBlue