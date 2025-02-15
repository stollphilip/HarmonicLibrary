using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ScoreLibrary
{
    public class Zoom
    {
        /// <summary>
        /// default ColumnDefinition width and RowDefinition height of the grid.
        /// </summary>
        private const double DefaultGridRowHeightColumnSize = 12.5;
        private const double DefaulatRectangleHeight = 1.5;
        public static double ZoomFactor { get; set; } = 1;
        public static double GridRowHeightColumnSize => DefaultGridRowHeightColumnSize * ZoomFactor;
        public static double RectangleHeight => DefaulatRectangleHeight * ZoomFactor;
        // .02 so that
        public static double fontScaleFactor => .02 * DefaultGridRowHeightColumnSize * ZoomFactor;
        // .5 so that notes are centered on the staff line
        public static double margin => .5 * DefaultGridRowHeightColumnSize * ZoomFactor;
        /// <summary>
        /// Update the ColumnDefinition width and RowDefinition height of the grid.
        /// </summary>
        public static void UpdateZoom(Grid grid, double zoomFactor)
        {
            ZoomFactor = zoomFactor;
            foreach (var rectangle in grid.Children.OfType<Rectangle>())
            {
                rectangle.Height = RectangleHeight;
            }
            foreach (var columnDefinition in grid.ColumnDefinitions)
            {
                columnDefinition.Width = new System.Windows.GridLength(GridRowHeightColumnSize);
            }
            foreach (var rowDefinition in grid.RowDefinitions)
            {
                rowDefinition.Height = new System.Windows.GridLength(GridRowHeightColumnSize);
            }
        }
        /// <summary>
        /// Update the number of ColumnDefinitions in the grid.
        /// </summary>
        public static void UpdateColumnDefinitions(Grid grid, int columnCount)
        {
            if (grid.ColumnDefinitions.Count == columnCount)
            {
                return;
            }
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < 40; ++i)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(GridRowHeightColumnSize) });
            }
        }
        public static ZoomStruct DefaultZoomStruct => zoomFactor3s[6];
        public static readonly ObservableCollection<ZoomStruct> zoomFactor3s = new ObservableCollection<ZoomStruct>
        {
            new ZoomStruct("25%", 0.25),
            new ZoomStruct("31%", 0.3125),
            new ZoomStruct("40%", 0.4),
            new ZoomStruct("50%", 0.5),
            new ZoomStruct("63%", 0.625),
            new ZoomStruct("80%", 0.8),
            new ZoomStruct("100%", 1),
            new ZoomStruct("125%", 1.25),
            new ZoomStruct("160%", 1.6),
            new ZoomStruct("200%", 2),
            //new ZoomStruct("250%", 2.5),
            //new ZoomStruct("320%", 3.2),
            //new ZoomStruct("400%", 4),

        };
        public static readonly double[] zoomFactors4 = new double[]
        {
            0.25,
            0.3,
            0.35,
            0.425,
            0.5,
            0.6,
            0.7,
            0.85,
            1,
            1.2,
            1.4,
            1.7,
            2,
            //2.4,
            //2.8,
            //3.4,
            //4,
        };
    }
    public class ZoomStruct
    {
        // getters and setters are required to bind to the ComboBox
        public string Name
        {
            get; set;
        }
        public double Value
        {
            get; set;
        }

        public ZoomStruct(string name, double value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
