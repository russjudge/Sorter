using System.Collections;
using System.ComponentModel;

namespace RussJudge.Sorter
{
    internal class ComparableSorter : IComparer
    {

        private ListSortDirection Direction { get; set; }
        private ComparableSorter() { }
        public ComparableSorter(ListSortDirection direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// Compare two comparable. objects.
        /// </summary>
        /// <param name="x">first object.</param>
        /// <param name="y">second object.</param>
        /// <returns>-1 if x lt y, 1 if x gt y.</returns>
        public int Compare(object? x, object? y)
        {
            IComparable? dtx = x as IComparable;
            IComparable? dty = y as IComparable;
            if (dtx != null && dty != null)
            {
                return (Direction == ListSortDirection.Ascending) ? dtx.CompareTo(dty) : dty.CompareTo(dtx);
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