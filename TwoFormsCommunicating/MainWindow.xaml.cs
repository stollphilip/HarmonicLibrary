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
using CommunityToolkit.Mvvm.Input;

namespace TwoFormsCommunicating;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        secondaryWindow = new SecondWindow();
        secondaryWindow.ActivateEvent += () => this.Activate();
        secondaryWindow.NotifyEvent += SecondaryWindow_NotifyEventCallback;

        ActivateWindowCommand = new RelayCommand(() => ActivateWindow());
        this.ActivateEvent += () => secondaryWindow.Activate();

        DataContext = this;
        InitializeComponent();
        secondaryWindow.Show();
    }
    readonly SecondWindow secondaryWindow;

    #region Activate Window
    public ICommand ActivateWindowCommand
    {
        get;
    }
    public event Action? ActivateEvent;
    private void ActivateWindow()
    {
        if (ActivateEvent != null)
        {
            ActivateEvent();
        }
    }
    #endregion

    #region Notify Window
    private void SecondaryWindow_NotifyEventCallback(string str)
    {
        textBlock.Text = str;
    }
    #endregion
}