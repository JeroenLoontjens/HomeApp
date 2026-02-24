$content = Get-Content "WEB_USER_API\Program.cs" -Encoding UTF8 -Raw

# Voeg CORS-configuratie toe na AddControllers
$corsConfig = @"
            // Voeg CORS toe voor Android emulator
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAndroidEmulator",
                    builder => builder
                        .WithOrigins("http://10.0.2.2:5053")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
"@

$content = $content -replace 'builder.Services.AddControllers\(\);', ('builder.Services.AddControllers();' + $corsConfig)

$content | Out-File -FilePath "WEB_USER_API\Program.cs" -Encoding UTF8 -Force