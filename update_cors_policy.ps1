$content = Get-Content "WEB_USER_API\Program.cs" -Encoding UTF8 -Raw

# Vervang de huidige CORS-configuratie door een meer permissieve
$newCorsConfig = @"
            // Voeg CORS toe voor Android emulator en ontwikkeling
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAndroidEmulator",
                    builder => builder
                        .WithOrigins("http://10.0.2.2:5053", "http://localhost:5053")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
"@

$content = $content -replace '// Voeg CORS toe voor Android emulator.*?});', $newCorsConfig

$content | Out-File -FilePath "WEB_USER_API\Program.cs" -Encoding UTF8 -Force