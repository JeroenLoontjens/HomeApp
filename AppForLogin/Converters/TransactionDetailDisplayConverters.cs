using System;
using System.Globalization;
using DataAccess.Model;
using Microsoft.Maui.Controls;

namespace AppForLogin.Converters
{
    public class BudgetLinePickerDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not BudgetLine budgetLine)
                return string.Empty;

            var notes = string.IsNullOrWhiteSpace(budgetLine.Notes) ? "(geen notitie)" : budgetLine.Notes.Trim();
            var month = budgetLine.Period.ToString("MMMM", culture ?? CultureInfo.CurrentCulture);
            return $"{notes} | {month}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class TransactionSplitSubTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TransactionBudgetLine split)
                return string.Empty;

            var category = split.BudgetLine?.Category?.Name?.Trim();
            var hasCategory = !string.IsNullOrWhiteSpace(category);
            var hasPeriod = split.BudgetLine != null;

            if (hasCategory && hasPeriod)
            {
                var month = split.BudgetLine.Period.ToString("MMMM", culture ?? CultureInfo.CurrentCulture);
                return $"{category} | {month}";
            }

            if (hasCategory)
                return category!;

            if (hasPeriod)
                return split.BudgetLine!.Period.ToString("MMMM", culture ?? CultureInfo.CurrentCulture);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
