using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BarangayCIS.Desktop.Models;
using BarangayCIS.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BarangayCIS.Desktop.ViewModels
{
    public partial class ResidentsViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;

        [ObservableProperty]
        private ObservableCollection<Resident> residents = new();

        [ObservableProperty]
        private Resident? selectedResident;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isEditMode = false;

        public ResidentsViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [RelayCommand]
        public async Task LoadResidentsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var endpoint = string.IsNullOrWhiteSpace(SearchText) 
                    ? "residents" 
                    : $"residents?search={Uri.EscapeDataString(SearchText)}";
                
                var result = await _apiClient.GetAsync<List<Resident>>(endpoint);
                
                if (result != null)
                {
                    Residents.Clear();
                    foreach (var resident in result)
                    {
                        Residents.Add(resident);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load residents: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NewResident()
        {
            SelectedResident = new Resident
            {
                DateOfBirth = DateTime.Now.AddYears(-30),
                Gender = "Male",
                IsVoter = false,
                IsPWD = false,
                IsSenior = false
            };
            IsEditMode = true;
        }

        [RelayCommand]
        private void EditResident()
        {
            if (SelectedResident != null)
            {
                IsEditMode = true;
            }
        }

        [RelayCommand]
        private async Task SaveResidentAsync()
        {
            if (SelectedResident == null) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                if (SelectedResident.Id == 0)
                {
                    // Create new
                    var created = await _apiClient.PostAsync<Resident>("residents", SelectedResident);
                    if (created != null)
                    {
                        Residents.Add(created);
                        SelectedResident = created;
                        MessageBox.Show("Resident created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // Update existing
                    var updated = await _apiClient.PutAsync<Resident>($"residents/{SelectedResident.Id}", SelectedResident);
                    if (updated != null)
                    {
                        var index = Residents.IndexOf(Residents.FirstOrDefault(r => r.Id == SelectedResident.Id)!);
                        if (index >= 0)
                        {
                            Residents[index] = updated;
                        }
                        SelectedResident = updated;
                        MessageBox.Show("Resident updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                IsEditMode = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save resident: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteResidentAsync()
        {
            if (SelectedResident == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete {SelectedResident.FullName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await _apiClient.DeleteAsync($"residents/{SelectedResident.Id}");
                    Residents.Remove(SelectedResident);
                    SelectedResident = null;
                    MessageBox.Show("Resident deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Failed to delete resident: {ex.Message}";
                    MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedResident = null;
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = LoadResidentsAsync();
        }
    }
}
