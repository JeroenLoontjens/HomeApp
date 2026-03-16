using CommunityToolkit.Mvvm.ComponentModel;
using DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Input;

namespace AppForLogin.ViewModel.Generic
{
    public partial class CategoryViewModel : ObservableObject
    {
        public Category Category { get; }

        [ObservableProperty]
        private bool isExpanded;

        [ObservableProperty]
        private int level;

        public string Icon => IsExpanded ? "chevron_down.png" : "chevron_right.png";

        public ObservableCollection<CategoryViewModel> Children { get; } = new();

        public bool HasChildren => Children.Count > 0;  

        public CategoryViewModel(Category category)
        {
            Category = category;
            Level = 0; // Root level for new categories
           
            // When children are added/removed, update HasChildren
            Children.CollectionChanged += OnChildrenCollectionChanged;
        }

        private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasChildren));
        }



        partial void OnIsExpandedChanged(bool value)
        {
            OnPropertyChanged(nameof(Icon));
        }

        public ICommand ToggleExpandCommand => new Command(() =>
        {
            Console.WriteLine("TOGGLE " + Category.Name);
            IsExpanded = !IsExpanded;
        });

        
    }
}
