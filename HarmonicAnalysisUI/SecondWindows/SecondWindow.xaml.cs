using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HarmonicAnalysisUI.SecondWindows;
/// <summary>
/// Interaction logic for SecondWindow.xaml
/// </summary>
public partial class SecondWindow : Window
{
    public SecondWindow()
    {
        myViewModel = new MyViewModel();
        myViewModel.NotifyEvent += (str) => NotifyEvent?.Invoke(str);
        myViewModel.CommandEvent += (str) => CommandEvent?.Invoke(str);
        this.RespondEvent += myViewModel.RespondEventCallback;
        DataContext = myViewModel;
        InitializeComponent();
        //Progressions.SelectionChanged += (sender, e) =>
        //{
        //    var dataGrid = (DataGrid)sender;
        //    Debug.WriteLine($"Progressions.SelectionChanged SelectedIndex {dataGrid.SelectedIndex}");
        //    if (e.AddedItems.Count != 0 && e.AddedItems[0].GetType().ToString() == "ScoreLibrary.Parameter.ProgressionParam")
        //    {
        //        var item = (ScoreLibrary.Parameter.ProgressionParam)e.AddedItems[0];
        //        int index = myViewModel.ProgressionsModel.Progressions.IndexOf(item);
        //        int itemIndex  = Progressions.Items.IndexOf(item);
        //        Debug.WriteLine($"Progressions.SelectionChanged AddedItems Progressions.IndexOf {index} Items.IndexOf {itemIndex}");
        //    }
        //    if (e.AddedItems.Count != 0)
        //    {
        //        Debug.WriteLine($"Progressions.SelectionChanged AddedItems {e.AddedItems[0].GetType()}");
        //    }
        //};
    }
    public MyViewModel myViewModel { get; }

    #region MainWindow SecondWindow communication
    // sequence of events:
    // MyViewModel raises NotifyEvent
    // SecondWindow raises NotifyEvent
    // MainWindow subscribes to NotifyEvent
    public event Action<string>? NotifyEvent;

    // sequence of events:
    // MainWindow raises RespondEvent
    // SecondWindow raises RespondEvent
    // MyViewModel subscribes to RespondEvent
    public event Action<string>? RespondEvent;

    public event Action<string>? CommandEvent;

    public void NotifyEventCallback(string json)
    {
        //MessageBox.Show(json);
        //ProcessMessage(json);
    }
    public void RespondEventCallback(string json)
    {
        RespondEvent?.Invoke(json);
    }
    #endregion
}
