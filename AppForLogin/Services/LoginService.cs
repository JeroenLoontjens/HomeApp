using AppForLogin.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AppForLogin.Services
{
    class LoginService : IloginRepository
    {
        private readonly HttpClient _client;

        public LoginService(HttpClient client)
        {
            _client = client;
        }

        public async Task<LoginResponse> Login(string email, string password)
        {                        
            string url = "api/user/login";
            System.Diagnostics.Debug.WriteLine("LOGIN: Attempting to connect to " + _client.BaseAddress + url);
            System.Diagnostics.Debug.WriteLine("LOGIN: Email=" + email + ", Password=" + (string.IsNullOrEmpty(password) ? "[provided]" : "[empty]"));

            var dto = new LoginDto
            {
                Email = email,
                Password = password
            };

            try
            {
                // Stuur request als JSON. stuur geen belangrijke data in URL.
                System.Diagnostics.Debug.WriteLine("LOGIN: Sending POST request...");
                HttpResponseMessage response = await _client.PostAsJsonAsync(url, dto);
                System.Diagnostics.Debug.WriteLine("LOGIN: Response received");

                var content = await response.Content.ReadAsStringAsync(); 
                System.Diagnostics.Debug.WriteLine("RESPONSE: " + content); 
                System.Diagnostics.Debug.WriteLine("STATUS: " + response.StatusCode);


                // Controleer of de statuscode in de 200-serie valt
                if (!response.IsSuccessStatusCode)
                {
                    // Optioneel: Specifiekere logica op basis van statuscode
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new InvalidOperationException("E-mailadres and/or Password are incorrect");
                    }

                    throw new HttpRequestException($"Server issue: {response.StatusCode}, Content: {content}");
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
                {
                    throw new InvalidOperationException("Login failed: No Token received.");
                }

                return loginResponse;
                
                
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine("LOGIN ERROR: HttpRequestException - " + ex.Message);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("LOGIN ERROR: Inner exception - " + ex.InnerException.Message);
                }
                throw new InvalidOperationException("Connection failed: " + ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine("LOGIN ERROR: TaskCanceledException - " + ex.Message);
                throw new InvalidOperationException("Connection timeout: " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LOGIN ERROR: Exception - " + ex.GetType().Name + ": " + ex.Message);
                throw new InvalidOperationException("Login failed: " + ex.Message);
            }
            
            
        }

        public void SetAuthHeader(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }


    }
    
}