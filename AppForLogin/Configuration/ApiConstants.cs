using System;
using Microsoft.Maui.Devices;

namespace AppForLogin.Configuration
{
    public static class ApiConstants
    {
        public static readonly string BaseAddress;

        static ApiConstants()
        {
            // Detecteer platform op runtime — flexibeler dan preprocessor
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // Android emulator (AVD) naar host machine
                BaseAddress = "http://10.0.2.2:5053/";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // iOS simulator belooft localhost van de host machine
                BaseAddress = "https://localhost:7177/";
            }
            else
            {
                // Windows, Mac, of fallback
                BaseAddress = "https://localhost:7177/";
            }
        }
    }
}
