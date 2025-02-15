using System;
using System.Collections.Generic;
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
using CommunityToolkit.Mvvm.Input;
using static TwoFormsCommunicating.MainWindow;

namespace TwoFormsCommunicating;
/// <summary>
/// Interaction logic for SecondWindow.xaml
/// </summary>
public partial class SecondWindow : Window
{
    public SecondWindow()
    {
        ActivateWindowCommand = new RelayCommand(() => ActivateWindow());
        DataContext = this;
        InitializeComponent();
    }
    public ICommand ActivateWindowCommand
    {
        get;
    }
    public event Action? ActivateEvent;
    public event Action<string>? NotifyEvent;

    int count = 0;
    private void ActivateWindow()
    {
        if (ActivateEvent != null)
        {
            ActivateEvent();
        }
    }
    private void button_Click(object sender, RoutedEventArgs e)
    {
        if (NotifyEvent != null)
        {
            NotifyEvent($"Message {++count}");
        }
    }
}
