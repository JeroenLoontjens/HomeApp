using DataAccess.Model;
using AppForLogin.Services;
using AppForLogin.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Drawing;
using Color = Microsoft.Maui.Graphics.Color;
using AppForLogin.ViewModel.Generic;


namespace AppForLogin.ViewModel
{
    public partial class CategoriesPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CategoryViewModel> rootCategories = new(); 

        private readonly BudgetService _budgetService;

        public CategoriesPageViewModel(BudgetService budgetService)
        {
            _budgetService = budgetService;
            _ = LoadRootCategories();

        }

        private async Task LoadRootCategories()
        {
            var allcategories =  await _budgetService.GetAllCategoriesAsync();
            
            var roots = allcategories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => new CategoryViewModel(c))
                .ToList();

            foreach (var root in roots)
            {
                BuildTree(root, allcategories);
            }

            // Clear existing items and add new ones
            RootCategories.Clear();
            foreach (var root in roots)
            {
                RootCategories.Add(root);
            }
        }

        private void BuildTree(CategoryViewModel parentvm, List<Category> allCategories)
        {
            var children = allCategories
                .Where(c => c.ParentCategoryId == parentvm.Category.Id)
                .ToList();
            
            foreach (var child in children)
            {
                var childVm = new CategoryViewModel(child)
                { 
                    Level = parentvm.Level + 1
                };

                parentvm.Children.Add(childVm);
                
                BuildTree(childVm, allCategories);
            }

            
        }

        [RelayCommand]
        private async Task OpenCategoryDetail(CategoryViewModel categoryVM)
        {
            var category = categoryVM.Category;
            await Shell.Current.GoToAsync(nameof(CategorieDetailPage), true, new Dictionary<string, object>
                {
                    { "Category", category }
                });
        }

        [RelayCommand]
        private async Task CreateCategory()
        {
            var category = new Category();
            await Shell.Current.GoToAsync(nameof(CategorieDetailPage), true, new Dictionary<string, object>
                {
                    { "Category", category }
                });

        }





    }
}

