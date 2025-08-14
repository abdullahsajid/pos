using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using pos.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pos.ViewModels
{
    public partial class SettingsModel : ObservableObject
    {
        private readonly DB_Services _dbService;

        [ObservableProperty]
        private string image;

        [ObservableProperty]
        private string companyName;

        [ObservableProperty]
        private string companyAddress;

        [ObservableProperty]
        private string companyPhone;

        public SettingsModel(DB_Services dbService)
        {
            _dbService = dbService;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await LoadSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing settings: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load settings: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task UploadImage()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select company logo"
                });

                if (result != null)
                {
                    Image = result.FullPath;
                    Debug.WriteLine($"Selected image: {result.FullPath}");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to upload image: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task SaveSettings()
        {
            try
            {   if (string.IsNullOrEmpty(CompanyName) || string.IsNullOrEmpty(CompanyAddress) || string.IsNullOrEmpty(CompanyPhone))
                {
                    await Shell.Current.DisplayAlert("Error", "Please fill in all fields.", "OK");
                    return;
                }
                var settings = new Settings
                {
                    Image = Image,
                    CompanyName = CompanyName,
                    CompanyAddress = CompanyAddress,
                    CompanyPhone = CompanyPhone
                };
                var result = await _dbService.createSettings(settings);
                Debug.WriteLine($"Settings saved: {result}");
                Image = string.Empty;
                CompanyName = string.Empty;
                CompanyAddress = string.Empty;
                CompanyPhone = string.Empty;
                //if (result)
                //{
                //    await Application.Current.MainPage.DisplayAlert("Success", "Settings saved successfully.", "OK");
                //}
                //else
                //{
                //    await Application.Current.MainPage.DisplayAlert("Error", "Failed to save settings.", "OK");
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
            }
        }

        public async Task LoadSettings()
        {
            try
            {
                var settings = await _dbService.getSettings();
                if (settings != null)
                {
                    Image = settings.Image;
                    CompanyName = settings.CompanyName;
                    CompanyAddress = settings.CompanyAddress;
                    CompanyPhone = settings.CompanyPhone;
                }
                else
                {
                    Debug.WriteLine("No settings found, using default values.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load settings: {ex.Message}", "OK");
            }
        }
    }
}
