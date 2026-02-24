using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AppForLogin.Model;

namespace AppForLogin.Services
{
    public class UserService
    {        
            private readonly HttpClient _client;


            public UserService(HttpClient client)
            {
                _client = client;
            }


            public async Task<List<User>> GetAllAsync()
            {
                return await _client.GetFromJsonAsync<List<User>>("api/user") ?? new List<User>();
            }

            //public async Task<User?> AddAsync(AddUserDto dto)
            //{
            //    var resp = await _client.PostAsJsonAsync("api/user", dto);
            //    if (!resp.IsSuccessStatusCode) return null;
            //    return await resp.Content.ReadFromJsonAsync<User>();
            //}

             public async Task<User?> AddAsync(AddUserDto dto)
             {
                try
                { // POST naar je API endpoint
                  var response = await _client.PostAsJsonAsync("api/user", dto); 
                  if (response.IsSuccessStatusCode) 
                    { // API gaf 200/201 terug → lees de User uit de body
                         var createdUser = await response.Content.ReadFromJsonAsync<User>();
                         return createdUser; 
                    } 
                  else 
                    { // API gaf een fout terug → log details
                         var errorContent = await response.Content.ReadAsStringAsync();
                         
                         Console.WriteLine($"AddAsync failed: {response.StatusCode} - {errorContent}");
                         Console.WriteLine(_client.BaseAddress + "user");
                         return null;
                    } 
                } 
                catch (Exception ex) 
                    { // Netwerkfout of iets anders
                         Console.WriteLine($"AddAsync exception: {ex.Message}"); 
                         return null; 
                    } 
             }

            public async Task<User?> UpdateAsync(Guid id, UpdateUserDto dto)
            {
                var resp = await _client.PutAsJsonAsync($"api/user/{id}", dto);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<User>();
            }

            public async Task DeleteAsync(Guid id)
            {
                var resp = await _client.DeleteAsync($"api/user/{id}");
                if (!resp.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Delete failed: {resp.StatusCode}");
                }
            }
        
    }



}

