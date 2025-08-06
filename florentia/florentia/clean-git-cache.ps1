# Script para limpiar la caché de Git y aplicar el nuevo .gitignore
# Ejecutar este script en PowerShell para que Git reconozca los cambios en el .gitignore

# Elimina los archivos del índice de Git (no borra los archivos físicamente)
git rm -r --cached .

# Añade todos los archivos de nuevo, respetando el nuevo .gitignore
git add .

# Muestra el estado para verificar qué archivos serán incluidos
git status

Write-Host ""
Write-Host "Se ha limpiado la caché de Git y se han aplicado las reglas del nuevo .gitignore."
Write-Host "Revisa el estado (git status) para verificar qué archivos serán incluidos en el próximo commit."
Write-Host "Si estás satisfecho con los cambios, ejecuta: git commit -m 'Aplicando nuevo .gitignore'"