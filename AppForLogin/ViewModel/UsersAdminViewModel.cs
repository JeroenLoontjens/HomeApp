using AppForLogin.Model;
using AppForLogin.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Data;


namespace AppForLogin.ViewModel
{
    public partial class UsersAdminViewModel : ObservableObject
    {
        private readonly UserService _userService;
        [ObservableProperty]
        private Guid id;
        [ObservableProperty] 
        private string name; 
        [ObservableProperty] 
        private string email; 
        [ObservableProperty] 
        private string password; 
        [ObservableProperty] 
        private string role;
        [ObservableProperty]
        private string phone;
        
        [ObservableProperty]
        private User selectedUser;
        
        private ObservableCollection<User> users = new();
        public ObservableCollection<User> Users
        {
            get => users;
            set => SetProperty(ref users, value);
        }

        public UsersAdminViewModel(UserService userService)
        {
            _userService = userService;
        }


        partial void OnSelectedUserChanged(User value)
        {
            if (value != null)
            {
                
                Name = value.Name;
                Email = value.Email;
                Phone = value.Phone;
                Role = value.Role;
                Id = value.Id;
            }
        }

        private void ResetForm()
        {
            SelectedUser = null;
            Name = Email = Password = Role = string.Empty;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                var list = await _userService.GetAllAsync();
                if (list != null)
                {
                    Users.Clear();
                    foreach (var user in list) 
                        Users.Add(user);
                    
                }
                else
                    await Shell.Current.DisplayAlertAsync("Error", "Could not found users", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Exception: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        public async Task AddAsync()
        {
            var dto = new AddUserDto
            {
                Name = Name,
                Email = Email,
                Password = await Shell.Current.DisplayPromptAsync("Nieuw wachtwoord", "Voer een wachtwoord in:", "OK", "Annuleren", "Wachtwoord", maxLength: 50, keyboard: Keyboard.Text),
                Role = Role ?? "User",
                Phone = Phone ?? "00000000000",
                Licenses = new List<LicenseDto>()
            };

            var created = await _userService.AddAsync(dto);
            if (created != null)
            {
                LoadAsync();
                Shell.Current.DisplayAlertAsync("Success", "User added", "OK");
                ResetForm();    
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "Could not add user", "OK");
            }
        }


        [RelayCommand]
        public async Task SaveEditAsync()
        {
            if (SelectedUser == null) return;

            var dto = new UpdateUserDto 
            { 
                Name = Name, 
                Email = Email, 
                Phone = Phone, 
                Role = Role 
            };

            var updated = await _userService.UpdateAsync(SelectedUser.Id, dto);
            if (updated != null)
            {
                var idx = Users.IndexOf(SelectedUser);
                Users[idx] = updated;
                await Shell.Current.DisplayAlertAsync("Success", "User updated", "OK");
                ResetForm();
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "Could not update user", "OK");
            }
        }

        [RelayCommand]
        public async Task DeleteUser(Guid userid)
        {
            try
            {
                await _userService.DeleteAsync(userid);

                await Shell.Current.DisplayAlertAsync("Success", "User deleted", "OK");

                await LoadAsync();

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Exception: {ex.Message}", "OK");
            }
        }

        
    }
}