using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TextMetricLib;

namespace TextMetricUI;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    const int row = 8;
    const int column = 3;
    //static double fontSize = -1;
    public MainWindow()
    {
        InitializeComponent();
        foreach (var t in TextMetric.Data)
        {
            comboBox.Items.Add(t);
        }
    }

    private Size MeasureString(string candidate)
    {
        var formattedText = new FormattedText(
            candidate,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(
                new System.Windows.Media.FontFamily("Segoe UI Symbol"),
                FontStyles.Normal,
                FontWeights.Regular,
                FontStretches.Normal),
            //new Typeface(this.tb.FontFamily, this.tb.FontStyle, this.tb.FontWeight, this.tb.FontStretch),
            140,
            //this.rtb.FontSize,
            Brushes.Black,
            new NumberSubstitution(),
            1);
        return new Size(formattedText.Width, formattedText.Height);
    }

    private void fontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (textBlock is null) return;
        if (e.NewValue < 1) return;
        textBlock.FontSize = e.NewValue;
        fontSizeLabel.Content = e.NewValue.ToString("#.");
    }

    private void marginLeft_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (textBlock is null) return;
        textBlock.Margin = new Thickness(e.NewValue, textBlock.Margin.Top, textBlock.Margin.Right, textBlock.Margin.Bottom);
        marginLeftLabel.Content = e.NewValue.ToString("#.");
    }

    private void marginTop_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (textBlock is null) return;
        textBlock.Margin = new Thickness(textBlock.Margin.Left, e.NewValue, textBlock.Margin.Right, textBlock.Margin.Bottom);
        marginTopLabel.Content = e.NewValue.ToString("#.");
    }

    private void textBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (textBlock is null) return;
        textBlock.Text = textBox.Text;
    }

    private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (comboBox.SelectedItem as TextMetric != null)
        {
            var tm = comboBox.SelectedItem as TextMetric;
            textBlock.Text = tm.Text;
            textBlock.FontSize = tm.FontSize;
            textBlock.Margin = new Thickness(tm.MarginLeft, tm.MarginTop, 0, 0);
            fontSize.Value = tm.FontSize;
            // for change event
            //marginLeft.Value = tm.MarginLeft + 1;
            marginLeft.Value = tm.MarginLeft;
            marginTop.Value = tm.MarginTop;
            Grid.SetRow(textBlock, row + tm.Row);
            Grid.SetColumn(textBlock, column + tm.Column);
            Grid.SetRowSpan(textBlock, tm.RowSpan);
            Grid.SetColumnSpan(textBlock, tm.ColumnSpan);

            Grid.SetRow(border, row + tm.Row);
            Grid.SetColumn(border, column + tm.Column);
            Grid.SetRowSpan(border, tm.RowSpan);
            Grid.SetColumnSpan(border, tm.ColumnSpan);
            //Debug.WriteLine($"{Grid.GetRow(textBlock)} {Grid.GetColumn(textBlock)} {Grid.GetRowSpan(textBlock)} {Grid.GetColumnSpan(textBlock)}");

            //if (checkBox.IsChecked == true)
            //{
            //    if (column + 1 < pushGrid.ColumnDefinitions.Count)
            //    {
            //        var t = new TextBlock
            //        {
            //            Text = tm.Text,
            //            FontSize = tm.FontSize,
            //            Margin = new Thickness(tm.MarginLeft, tm.MarginTop, 0, 0),
            //        };
            //        Grid.SetRow(t, row + tm.Row);
            //        Grid.SetColumn(t, column + tm.Column);
            //        Grid.SetRowSpan(t, tm.RowSpan);
            //        Grid.SetColumnSpan(t, tm.ColumnSpan);

            //        pushGrid.Children.Add(t);
            //    }
            //}
        }
    }
}
