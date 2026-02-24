$content = Get-Content "WEB_USER_API\Program.cs" -Encoding UTF8 -Raw

# Fix de CORS-middleware opmaak
$content = $content -replace 'app.UseDeveloperExceptionPage\(\);            app.UseCors\("AllowAndroidEmulator"\);', "app.UseDeveloperExceptionPage();`n            app.UseCors(\"AllowAndroidEmulator\");"

$content | Out-File -FilePath "WEB_USER_API\Program.cs" -Encoding UTF8 -Force