using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace RussJudge.WPFListSorter
{
    internal class SortAdorner : Adorner
    {
        private SortAdorner() : base(null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SortAdorner"/> class.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <param name="direction">Direction.</param>
        public SortAdorner(GridViewColumnHeader element, ListSortDirection direction)
            : base(element)
        {
            Direction = direction;
        }
        /// <summary>
        /// Gets Direction.
        /// </summary>
        private ListSortDirection Direction { get; set; }
        /// <summary>
        /// On render.
        /// </summary>
        /// <param name="drawingContext">Drawing context.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (this.AdornedElement.RenderSize.Width < 20)
            {
                return;
            }

            if (drawingContext != null)
            {
                drawingContext.PushTransform(
                     new TranslateTransform(
                       this.AdornedElement.RenderSize.Width - 15,
                       (this.AdornedElement.RenderSize.Height - 5) / 2));

                drawingContext.DrawGeometry(
                    Sorter.SortIconBrush,
                    null,
                    this.Direction == ListSortDirection.Ascending ? Sorter.AscendingIcon : Sorter.DecendingIcon);

                drawingContext.Pop();
            }
        }
    }
}