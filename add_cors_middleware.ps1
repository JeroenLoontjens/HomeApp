$content = Get-Content "WEB_USER_API\Program.cs" -Encoding UTF8 -Raw

# Voeg CORS-middleware toe na UseDeveloperExceptionPage
$corsMiddleware = @"
            app.UseCors("AllowAndroidEmulator");
"@

$content = $content -replace 'app.UseDeveloperExceptionPage\(\);', ('app.UseDeveloperExceptionPage();' + $corsMiddleware)

$content | Out-File -FilePath "WEB_USER_API\Program.cs" -Encoding UTF8 -Force