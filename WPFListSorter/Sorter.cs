using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace RussJudge.WPFListSorter
{
    public static class Sorter
    {
        private static readonly Geometry AscGeometry = Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

        private static readonly Geometry DescGeometry = Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");

        public static Geometry AscendingIcon { get; set; } = AscGeometry;

        public static Geometry DecendingIcon { get; set; } = DescGeometry;

        public static Brush SortIconBrush { get; set; } = Brushes.LightSteelBlue;


        //To use: Binding for ItemsSource should be ObservableCollection<T>.
        //Currently only works for ListViews.
        // Add <GridColumnHeader GridViewColumnHeaderSorter.SortColumnID="xxx"> where "xxx" is the field name in the binding
        //  to sort on.  only works with simple (one field) sorting.

        /// <summary>
        /// Sort direction.
        /// </summary>
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.RegisterAttached(
                "SortDirection",
                typeof(ListSortDirection),
                typeof(Sorter),
                new FrameworkPropertyMetadata(ListSortDirection.Ascending));

        /// <summary>
        /// Is Default property.  Set the particular column to true to automatically sort on this column on load.
        /// </summary>
        public static readonly DependencyProperty IsDefaultProperty =
           DependencyProperty.RegisterAttached(
               "IsDefault",
               typeof(bool),
               typeof(Sorter));

        /// <summary>
        /// Sort column id property.
        /// </summary>
        public static readonly DependencyProperty SortColumnIDProperty =
            DependencyProperty.RegisterAttached(
                "SortColumnID",
                typeof(string),
                typeof(Sorter),
                new FrameworkPropertyMetadata(OnSortColumnIDChanged));

        /// <summary>
        /// Parent ItemsControl property.  (note: used only in this class.)
        /// </summary>
        private static readonly DependencyProperty ParentItemsControlProperty =
            DependencyProperty.RegisterAttached(
                "ParentItemsControl",
                typeof(ItemsControl),
                typeof(Sorter),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnParentItemsControlChanged)));

        private static readonly DependencyProperty CurrentSortColumnProperty =
            DependencyProperty.RegisterAttached(
                "CurrentSortColumn",
                typeof(GridViewColumnHeader),
                typeof(Sorter),
                new FrameworkPropertyMetadata());

        private static readonly DependencyProperty CurrentSortAdornerProperty =
            DependencyProperty.RegisterAttached(
                "CurrentSortAdorner",
                typeof(SortAdorner),
                typeof(Sorter));

        private static readonly DependencyProperty IsSortingProperty =
          DependencyProperty.RegisterAttached(
              "IsSorting",
              typeof(bool),
              typeof(Sorter),
              new FrameworkPropertyMetadata());

        /// <summary>
        /// Get sort direction.
        /// </summary>
        /// <param name="element">element.</param>
        /// <returns>sort direction.</returns>
        private static ListSortDirection GetSortDirection(this DependencyObject? element)
        {
            ListSortDirection value = ListSortDirection.Ascending;
            if (element != null)
            {
                object result = element.GetValue(SortDirectionProperty);
                if (result != null)
                {
                    value = (ListSortDirection)result;
                }
                else
                {
                    value = ListSortDirection.Ascending;
                }
            }

            return value;
        }

        /// <summary>
        /// Set is default.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <param name="value">true if default.</param>
        public static void SetIsDefault(this DependencyObject? element, bool value)
        {
            element?.SetValue(IsDefaultProperty, value);
        }

        /// <summary>
        /// Get is default.
        /// </summary>
        /// <param name="element">element.</param>
        /// <returns>true if default.</returns>
        public static bool GetIsDefault(this DependencyObject? element)
        {
            bool value = true;
            if (element != null)
            {
                value = (bool)element.GetValue(IsDefaultProperty);
            }

            return value;
        }

        /// <summary>
        /// Set sort column id.
        /// </summary>
        /// <param name="element">object to set.</param>
        /// <param name="value">Value to set.</param>
        public static void SetSortColumnID(this DependencyObject? element, string value)
        {
            element?.SetValue(SortColumnIDProperty, value);
        }

        /// <summary>
        /// Get sort column id.
        /// </summary>
        /// <param name="element">column.</param>
        /// <returns>The id.</returns>
        public static string? GetSortColumnID(this DependencyObject? element)
        {
            string? value = null;
            if (element != null)
            {
                value = (string)element.GetValue(SortColumnIDProperty);
            }

            return value;
        }

        /// <summary>
        /// Set Current sort column.
        /// </summary>
        /// <param name="element">List view to set.</param>
        /// <param name="value">Column to set to.</param>
        public static void SetCurrentSortColumn(this ItemsControl? element, GridViewColumnHeader value)
        {
            if (element != null)
            {
                if (value == null)
                {
                    element.RemoveSortAdorner();
                }
                element.SetValue(CurrentSortColumnProperty, value);
            }
        }

        /// <summary>
        /// Get Current sort column.
        /// </summary>
        /// <param name="element">element to check.</param>
        /// <returns>Column Header.</returns>
        public static GridViewColumnHeader? GetCurrentSortColumn(this DependencyObject? element)
        {
            GridViewColumnHeader? value = null;
            if (element != null)
            {
                value = (GridViewColumnHeader)element.GetValue(CurrentSortColumnProperty);
            }

            return value;
        }
        /// <summary>
        /// Finds all rows matching search item.
        /// </summary>
        /// <param name="element">The ItemsControl</param>
        /// <param name="match">The search text</param>
        /// <param name="searchOnCurrentSortColumnOnly">Whether or not to search only on the currently sorted column, or on all columns.</param>
        /// <returns></returns>
        public static object[] FindAll(this ItemsControl? element, string match, bool searchOnCurrentSortColumnOnly = true)
        {
            List<object> retVal = [];
            GridViewColumnHeader? sortColumn = element?.GetCurrentSortColumn();
            if (sortColumn != null && element != null && element.Items.Count > 0)
            {
                List<PropertyInfo> matchProperties = [];
                string? field = GetSortColumnID(sortColumn);
                if (!string.IsNullOrEmpty(field))
                {
                    foreach (var p in element.Items[0].GetType().GetProperties())
                    {
                        if (p.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase) || !searchOnCurrentSortColumnOnly)
                        {
                            matchProperties.Add(p);
                            break;
                        }
                    }
                }

                if (matchProperties.Count > 0 && CollectionViewSource.GetDefaultView(element.ItemsSource) is ICollectionView dataView)
                {
                    foreach (var item in dataView)
                    {
                        foreach (var matchProperty in matchProperties)
                        {
                            var propValue = matchProperty.GetValue(item);
                            if (propValue != null)
                            {
                                string? checkValue = propValue.ToString();
                                if (!string.IsNullOrEmpty(checkValue) && checkValue.Contains(match))
                                {
                                    retVal.Add(item);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
#pragma warning disable IDE0305 // Simplify collection initialization
            return retVal.ToArray();
#pragma warning restore IDE0305 // Simplify collection initialization
        }
        /// <summary>
        /// Returns first matching row.
        /// </summary>
        /// <param name="element">The ItemsControl</param>
        /// <param name="match">The search term</param>
        /// <param name="searchOnCurrentSortColumnOnly">Whether or not to search just the currently sorted column, or all columns.</param>
        /// <returns></returns>
        public static object? Find(this ItemsControl? element, string match, bool searchOnCurrentSortColumnOnly = true)
        {
            object? retVal = null;
            GridViewColumnHeader? sortColumn = element.GetCurrentSortColumn();
            if (sortColumn != null && element != null && element.Items.Count > 0)
            {
                List<PropertyInfo> matchProperties = [];
                string? field = sortColumn.GetSortColumnID();
                foreach (var p in element.Items[0].GetType().GetProperties())
                {
                    if (p.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase) || !searchOnCurrentSortColumnOnly)
                    {
                        matchProperties.Add(p);
                    }
                }
                if (matchProperties.Count > 0 && CollectionViewSource.GetDefaultView(element.ItemsSource) is ICollectionView dataView)
                {
                    foreach (var item in dataView)
                    {
                        foreach (var matchProperty in matchProperties)
                        {
                            var propValue = matchProperty.GetValue(item);
                            if (propValue != null)
                            {
                                string? checkValue = propValue.ToString();
                                if (!string.IsNullOrEmpty(checkValue) && checkValue.Contains(match))
                                {
                                    retVal = item;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// Returns all rows where the column starts with the search text.
        /// </summary>
        /// <param name="element">The ItemsControl</param>
        /// <param name="match">The text to search for</param>
        /// <param name="searchOnCurrentSortColumnOnly">Whether or not to search only the sort column or all columns.</param>
        /// <returns></returns>
        public static object[] FindAllStartsWith(this ItemsControl? element, string match, bool searchOnCurrentSortColumnOnly = true)
        {
            List<object> retVal = [];
            GridViewColumnHeader? sortColumn = element?.GetCurrentSortColumn();
            if (sortColumn != null && element != null && element.Items.Count > 0)
            {
                List<PropertyInfo> matchProperties = [];

                string? field = sortColumn.GetSortColumnID();
                foreach (var p in element.Items[0].GetType().GetProperties())
                {
                    if (p.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase) || !searchOnCurrentSortColumnOnly)
                    {
                        matchProperties.Add(p);
                        break;
                    }
                }

                if (matchProperties.Count > 0 && CollectionViewSource.GetDefaultView(element.ItemsSource) is ICollectionView dataView)
                {
                    foreach (var item in dataView)
                    {
                        foreach (var matchProperty in matchProperties)
                        {
                            var propValue = matchProperty.GetValue(item);
                            if (propValue != null)
                            {
                                string? checkValue = propValue.ToString();
                                if (!string.IsNullOrEmpty(checkValue) && checkValue.StartsWith(match))
                                {
                                    retVal.Add(item);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

#pragma warning disable IDE0305 // Simplify collection initialization
            return retVal.ToArray();
#pragma warning restore IDE0305 // Simplify collection initialization
        }
        /// <summary>
        /// Returns first row with column that starts with match text.
        /// </summary>
        /// <param name="element">The ItemsControl to search</param>
        /// <param name="match">The search text to match</param>
        /// <param name="searchOnCurrentSortColumnOnly">Whether or not to match only the currently sorted column or all columns.</param>
        /// <returns></returns>
        public static object? FindStartsWith(this ItemsControl? element, string match, bool searchOnCurrentSortColumnOnly = true)
        {
            object? retVal = null;

            GridViewColumnHeader? sortColumn = element?.GetCurrentSortColumn();
            if (sortColumn != null && element != null && element.Items.Count > 0)
            {
                List<PropertyInfo> matchProperties = [];

                string? field = sortColumn.GetSortColumnID();
                foreach (var p in element.Items[0].GetType().GetProperties())
                {
                    if (p.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase) || !searchOnCurrentSortColumnOnly)
                    {
                        matchProperties.Add(p);
                        break;
                    }
                }
                if (matchProperties.Count > 0 && CollectionViewSource.GetDefaultView(element.ItemsSource) is ICollectionView dataView)
                {
                    foreach (var item in dataView)
                    {
                        foreach (var matchProperty in matchProperties)
                        {
                            var propValue = matchProperty.GetValue(item);
                            if (propValue != null)
                            {
                                string? checkValue = propValue.ToString();
                                if (!string.IsNullOrEmpty(checkValue) && checkValue.StartsWith(match))
                                {
                                    retVal = item;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Sort.
        /// </summary>
        /// <param name="column">Column to sort on.</param>
        private static void Sort(this GridViewColumnHeader column)
        {
            ItemsControl? parent = column.ParentItemsControl();
            if (parent != null)
            {
                if (!parent.GetIsSorting())
                {
                    parent.SetIsSorting(true);
                    string? field = column.GetSortColumnID();
                    GridViewColumnHeader? curSortCol = parent.GetCurrentSortColumn();

                    SortAdorner? curAdorner = parent.GetCurrentSortAdorner();
                    if (!string.IsNullOrEmpty(field))
                    {

                        if (CollectionViewSource.GetDefaultView(parent.ItemsSource) is ListCollectionView dataView)
                        {
                            if (curSortCol != null)
                            {
                                AdornerLayer.GetAdornerLayer(curSortCol).Remove(curAdorner);
                                dataView.SortDescriptions.Clear();
                                parent.Items.SortDescriptions.Clear();
                            }
                            else
                            {
                                if (curAdorner != null)
                                {
                                    AdornerLayer.GetAdornerLayer(parent).Remove(curAdorner);
                                    dataView.SortDescriptions.Clear();
                                    parent.Items.SortDescriptions.Clear();
                                }
                            }

                            ListSortDirection newDir = GetSortDirection(column);

                            curAdorner = new SortAdorner(column, newDir);
                            AdornerLayer.GetAdornerLayer(column).Add(curAdorner);
                            SetCurrentSortColumn(parent, column);

                            SetCurrentSortAdorner(parent, curAdorner);

                            IComparer? sorter = null;

                            foreach (ItemPropertyInfo p in dataView.ItemProperties)
                            {
                                if (p.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Type ptype = p.PropertyType;
                                    if (ptype == typeof(DateTime))
                                    {
                                        sorter = new DateSorter(newDir);
                                    }
                                    else if (ptype == typeof(IComparable))
                                    {
                                        sorter = new ComparableSorter(newDir);
                                    }

                                    break;
                                }
                            }
                            if (sorter == null)
                            {
                                dataView.SortDescriptions.Add(new SortDescription(field, newDir));
                            }
                            else
                            {
                                dataView.CustomSort = sorter;
                            }

                            dataView.Refresh();
                        }
                    }
                    parent.SetIsSorting(false);
                }
            }
        }
        private static void RemoveSortAdorner(this ItemsControl? target)
        {
            if (target != null)
            {
                GridViewColumnHeader? curSortCol = target.GetCurrentSortColumn();
                SortAdorner? curAdorner = target.GetCurrentSortAdorner();
                if (curSortCol != null && curAdorner != null)
                {
                    AdornerLayer.GetAdornerLayer(curSortCol).Remove(curAdorner);
                    if (CollectionViewSource.GetDefaultView(target.ItemsSource) is ListCollectionView dataView)
                    {
                        dataView.SortDescriptions.Clear();
                    }

                    target.Items.SortDescriptions.Clear();
                }
            }
        }
        private static void ItemContainerGenerator_ItemsChanged(ItemsControl? parent)
        {
            parent?.GetCurrentSortColumn()?.Sort();
        }
        private static void SetCurrentSortAdorner(this ItemsControl? element, SortAdorner value)
        {
            element?.SetValue(CurrentSortAdornerProperty, value);
        }

        private static SortAdorner? GetCurrentSortAdorner(this ItemsControl? element)
        {
            SortAdorner? value = null;
            if (element != null)
            {
                value = (SortAdorner)element.GetValue(CurrentSortAdornerProperty);
            }

            return value;
        }

        private static void GridColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader headerClicked)
            {
                headerClicked.SetValue(SortDirectionProperty, headerClicked.GetSortDirection() == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                headerClicked.Sort();
            }
        }

        private static ItemsControl? ParentItemsControl(this GridViewColumnHeader me)
        {
            ItemsControl? parent = GetParentItemsControl(me);
            if (parent == null)
            {
                parent = GetAncestor<ItemsControl>(me);
                if (parent != null)
                {
                    SetParentItemsControl(me, parent);
                }
            }
            return parent;
        }

        private static void SetIsSorting(this DependencyObject? element, bool value)
        {
            element?.SetValue(IsSortingProperty, value);
        }

        private static bool GetIsSorting(this DependencyObject? element)
        {
            bool value = false;
            if (element != null)
            {
                bool? val = element.GetValue(IsSortingProperty) as bool?;
                if (val == null)
                {
                    value = false;
                }
                else
                {
                    value = val.Value;
                }
            }

            return value;
        }

        private static void SetParentItemsControl(DependencyObject? element, ItemsControl value)
        {
            element?.SetValue(ParentItemsControlProperty, value);
        }

        private static ItemsControl? GetParentItemsControl(DependencyObject? element)
        {
            ItemsControl? value = null;
            if (element != null)
            {
                value = (ItemsControl)element.GetValue(ParentItemsControlProperty);
            }

            return value;
        }

        private static void OnSortColumnIDChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is GridViewColumnHeader column)
            {
                if (e.NewValue != null && e.OldValue == null)
                {
                    column.Click += new RoutedEventHandler(GridColumnHeader_Click);
                    column.Loaded += new RoutedEventHandler(Column_Loaded);
                }
                if (e.NewValue == null && e.OldValue != null)
                {
                    column.Click -= new RoutedEventHandler(GridColumnHeader_Click);
                    column.Loaded -= new RoutedEventHandler(Column_Loaded);
                }
            }
        }

        private static void Column_Loaded(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (GridViewColumnHeader)sender;
            if (column.GetIsDefault())
            {
                column.Sort();
            }
        }

        private static T? GetAncestor<T>(DependencyObject reference)
            where T : DependencyObject
        {
            if (reference != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(reference);
                if (parent != null)
                {
                    while (parent is not T)
                    {
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                }

                if (parent != null)
                {
                    return (T)parent;
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }
        private static void OnParentItemsControlChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ItemsControl newparent)
            {
                newparent.ItemContainerGenerator.ItemsChanged += (o, err) => ItemContainerGenerator_ItemsChanged(newparent);
            }

            if (e.OldValue is ItemsControl oldparent)
            {
                oldparent.ItemContainerGenerator.ItemsChanged -= (o, err) => ItemContainerGenerator_ItemsChanged(oldparent);
            }
        }
    }
}