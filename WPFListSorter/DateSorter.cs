using System.Collections;
using System.ComponentModel;

namespace RussJudge.WPFListSorter
{
    internal class DateSorter : IComparer
    {
        private ListSortDirection Direction { get; set; }
        private DateSorter() { }
        public DateSorter(ListSortDirection direction)
        {
            Direction = direction;
        }
        /// <summary>
        /// Compare two date objects.
        /// </summary>
        /// <param name="x">first date object.</param>
        /// <param name="y">second date object.</param>
        /// <returns>-1 if x lt y, 1 if x gt y.</returns>
        public int Compare(object? x, object? y)
        {
            DateTime? dtx = x as DateTime?;
            DateTime? dty = y as DateTime?;
            if (dtx != null && dty != null)
            {
                return (Direction == ListSortDirection.Ascending) ? dtx.Value.CompareTo(dty.Value) : dty.Value.CompareTo(dtx.Value);
            }
            else if (dtx == null && dty != null)
            {
                return (Direction == ListSortDirection.Ascending) ? 1 : -1;
            }
            else if (dtx != null && dty == null)
            {
                return (Direction == ListSortDirection.Ascending) ? -1 : 1;
            }
            else
            {
                return 0;
            }
        }
    }
}