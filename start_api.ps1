cd "D:\WEB_USER_API\WEB_USER_API"
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run", "--urls", "http://localhost:5053", "--no-launch-profile"
Start-Sleep -Seconds 10