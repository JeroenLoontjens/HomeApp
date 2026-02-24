$content = Get-Content "WEB_USER_API\Program.cs" -Encoding UTF8 -Raw

# Fix de CORS-middleware opmaak
$oldText = 'app.UseDeveloperExceptionPage();            app.UseCors("AllowAndroidEmulator");'
$newText = "app.UseDeveloperExceptionPage();`r`n            app.UseCors(\"AllowAndroidEmulator\");"

$content = $content -replace [regex]::Escape($oldText), $newText

$content | Out-File -FilePath "WEB_USER_API\Program.cs" -Encoding UTF8 -Force