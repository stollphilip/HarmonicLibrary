using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel.Design;
using ScoreLibrary.Parameter;
namespace HarmonicAnalysisUI.SecondWindows;

// partial class is required by MVVM Toolkit
public partial class MyViewModel : ObservableObject
{
    // see https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
    // see https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.datagrid.rowdetailstemplate?view=windowsdesktop-8.0
    public MyViewModel()
    {
        // tip: use RelayCommand override to provide a CanExecute method
        FileNewCommand = new RelayCommand(FileNew);
        FileOpenCommand = new RelayCommand(FileOpen);
        FileAddCommand = new RelayCommand(FileAdd);
        FileDeleteCommand = new RelayCommand(FileDelete);
        FileMoveUpCommand = new RelayCommand(FileMoveUp);
        FileMoveDownCommand = new RelayCommand(FileMoveDown);
        FileSaveCommand = new RelayCommand(FileSave);
        FileSaveAsCommand = new RelayCommand(FileSaveAs);
        FilePrintCommand = new RelayCommand(FilePrint);
        FileCloseCommand = new RelayCommand(FileClose);
        FileExitCommand = new RelayCommand(FileExit);
        FileRecentFileCommand= new RelayCommand<MenuItem>(FileRecentFileAction);
        SendCommand = new RelayCommand(Send);
        ProgressionsModel = new ProgressionsParam();
        ProgressionsModel.AddData();
        ProgressionsModel.AttachSnapshots();
        AttachProgressionsModel();
        ResetIsDirty();
        LoadRecentList();
    }

    [ObservableProperty]
    private ProgressionsParam progressionsModel;

    private int ProgressionsModelHashCode;

    OpenFileDialog openFileDialog = new OpenFileDialog
    {
        AddExtension = true,
        DefaultExt = "json",
        Filter = "Json files (*.json)|*.json|All files (*.*)|*.*",
    };
    SaveFileDialog saveFileDialog = new SaveFileDialog
    {
        AddExtension = true,
        DefaultExt = "json",
        Filter = "Json files (*.json)|*.json|All files (*.*)|*.*",
    };

    /// <summary>
    /// On Send progression SecondWindow raises NotifyEvent, MainWindow listens for NotifyEvent
    /// </summary>
    public event Action<string> NotifyEvent;

    /// <summary>
    /// On miscellaneous command SecondWindow raises CommandEvent, MainWindow listens for CommandEvent
    /// </summary>
    public event Action<string> CommandEvent;
    public ICommand FileNewCommand { get; }
    public ICommand FileOpenCommand { get; }
    public ICommand FileAddCommand { get; }
    public ICommand FileDeleteCommand { get; }
    public ICommand FileMoveUpCommand { get; }
    public ICommand FileMoveDownCommand { get; }
    public ICommand FileSaveCommand { get; }
    public ICommand FileSaveAsCommand { get; }
    public ICommand FilePrintCommand { get; }
    public ICommand FileCloseCommand { get; }
    public ICommand FileExitCommand { get; }
    // Command for each RecentFile MenuItem
    public ICommand FileRecentFileCommand { get; }
    public ICommand SendCommand { get; }
    private void FileNew()
    {
        if (PromptToSaveChanges() == MessageBoxResult.Cancel)
        {
            return;
        }
        if (!IsDirty())
        {
            ProgressionsModel.DetachSnapshots();
            DetachProgressionsModel();
            ProgressionsModel.Progressions.Clear();
            ProgressionsModel.AttachSnapshots();
            AttachProgressionsModel();
            ClearFilename();
            ResetIsDirty();
        }
    }

    private void FileOpen()
    {
        if (PromptToSaveChanges() == MessageBoxResult.Cancel)
        {
            return;
        }
        if (!IsDirty())
        {
            Open();
            // no need to check whether file was opened
            ResetIsDirty();
        }
    }

    private void FileAdd()
    {
        var progressionParam = InitializeParam.CreateProgressionParam();
        progressionParam.Progression = "0 17 12 4, 0 5 12 19, 7 19 12 -1B, //NumberBase12";

        DetachProgressionsModel();
        ProgressionsModel.DetachSnapshots();
        ProgressionsModel.Progressions.Add(progressionParam);
        ProgressionsModel.AttachSnapshots();
        AttachProgressionsModel();

        ProgressionsModel.SelectedIndex = ProgressionsModel.Progressions.Count - 1;
    }
    private void FileDelete()
    {
        if (ProgressionsModel.SelectedIndex != -1)
        {
            var selectedProgressionModel = ProgressionsModel.Progressions[ProgressionsModel.SelectedIndex];
            ProgressionsModel.Progressions.Remove(selectedProgressionModel);
        }
    }
    private void FileMoveUp()
    {
        if (ProgressionsModel.SelectedIndex != -1)
        {
            Move(ProgressionsModel.SelectedIndex, ProgressionsModel.SelectedIndex - 1, ProgressionsModel.Progressions.Count);
        }
    }
    private void FileMoveDown()
    {
        if (ProgressionsModel.SelectedIndex != -1)
        {
            Move(ProgressionsModel.SelectedIndex, ProgressionsModel.SelectedIndex + 1, ProgressionsModel.Progressions.Count);
        }
    }
    private void FileSave()
    {
        if (Save(true))
        {
            ResetIsDirty();
        }
    }
    private void FileSaveAs()
    {
        if (Save())
        {
            ResetIsDirty();
        }
    }
    public const string Print = "Print";
    private void FilePrint()
    {
        if (CommandEvent != null)
        {
            CommandEvent.Invoke(Print);
        }
    }
    private void FileClose()
    {
        if (PromptToSaveChanges() == MessageBoxResult.Cancel)
        {
            return;
        }
        if (!IsDirty())
        {
            ProgressionsModel.DetachSnapshots();
            DetachProgressionsModel();
            ProgressionsModel = new ProgressionsParam();
            ProgressionsModel.Progressions.Clear();
            ClearFilename();
            ResetIsDirty();
        }
    }

    /// <summary>
    /// Open recent file
    /// </summary>
    private void FileRecentFile(string fileName)
    {
        if (PromptToSaveChanges() == MessageBoxResult.Cancel)
        {
            return;
        }
        if (!IsDirty())
        {
            Open(fileName);
            SaveRecentFile(fileName);
            ResetIsDirty();
            SetFilename(fileName);
        }
    }

    /// <summary>
    /// Exit the application
    /// </summary>
    private void FileExit()
    {
        if (PromptToSaveChanges() == MessageBoxResult.Cancel)
        {
            return;
        }
        if (!IsDirty())
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
    private void Send()
    {
        if (NotifyEvent != null)
        {
            var json = JsonSerializer.Serialize(ProgressionsModel);
            NotifyEvent.Invoke(json);
        }
    }
    private void Move(int oldIndex, int index, int count)
    {
        if (0 <= index && index < count)
        {
            ProgressionsModel.Progressions.Move(oldIndex, index);
        }
    }

    private void Open()
    {
        if (openFileDialog.ShowDialog() == true)
        {
            var fileName = openFileDialog.FileName;
            Open(fileName);
            SaveRecentFile(fileName);
            SetFilename(fileName);
        }
    }
    private void Open(string fileName)
    {
        var json = File.ReadAllText(fileName);
        ProgressionsParam? ProgressionsModel_ =
            JsonSerializer.Deserialize<ProgressionsParam>(json);
        if (ProgressionsModel_ != null)
        {
            ProgressionsModel.DetachSnapshots();
            DetachProgressionsModel();
            ProgressionsModel = ProgressionsModel_;
            AttachProgressionsModel();
            ProgressionsModel.AttachSnapshots();
        }
    }
    /// <summary>
    /// Save to file
    /// </summary>
    /// <param name="silentSave">Silently save without prompting the user</param>
    /// <returns>true if saved to file</returns>
    private bool Save(bool silentSave = false)
    {
        var fileName = string.Empty;
        if (silentSave && File.Exists(currentFilename))
        {
            fileName = currentFilename;
        }
        else if (saveFileDialog.ShowDialog() == true)
        {
            fileName = saveFileDialog.FileName;
        }
        else
        {
            // used click Cancel
            return false;
        }
        // File.Exists returns false if path is null, an invalid path, or a zero-length string.
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var json = JsonSerializer.Serialize(ProgressionsModel);
            File.WriteAllText(fileName, json);
            SaveRecentFile(fileName);
            SetFilename(fileName);
            return true;
        }
        return false;
    }
    /// <summary>
    /// File is dirty if it has changed
    /// </summary>
    /// <returns>true if file has changed</returns>
    private bool IsDirty()
    {
        return ProgressionsModel.MyGetHashCode() != ProgressionsModelHashCode;
    }

    private void ResetIsDirty()
    {
        ProgressionsModelHashCode = ProgressionsModel.MyGetHashCode();
    }

    /// <summary>
    /// name of the current file, used for Save without prompting for the file name
    /// </summary>
    /// <remarks>Access using ClearFilename and SetFilename</remarks>
    string currentFilename = string.Empty;
    private void ClearFilename() => currentFilename = string.Empty;
    private void SetFilename(string fileName) => currentFilename = fileName.Trim();

    /// <summary>
    /// Prompt user to save changes if necessary
    /// </summary>
    /// <returns>Cancel if Cancel button was clicked by the user, otherwise OK</returns>
    private MessageBoxResult PromptToSaveChanges()
    {
        if (IsDirty())
        {
            switch (MessageBoxShow())
            {
                case MessageBoxResult.Yes:
                    // give user option to save
                    if (Save())
                    {
                        ResetIsDirty();
                    }
                    return MessageBoxResult.OK;
                case MessageBoxResult.No:
                    ResetIsDirty();
                    return MessageBoxResult.OK;
                case MessageBoxResult.Cancel:
                default:
                    return MessageBoxResult.Cancel;
            }
        }
        return MessageBoxResult.OK;
    }

    private static MessageBoxResult MessageBoxShow()
    {
        return MessageBox.Show(Application.Current.MainWindow, "Save changes?", Application.Current.MainWindow.Title, MessageBoxButton.YesNoCancel, MessageBoxImage.None, MessageBoxResult.OK);
    }

    public void RespondEventCallback(string json)
    {
        ProgressionsParam? ProgressionsModel_ =
           JsonSerializer.Deserialize<ProgressionsParam>(json);
        var selectedIndex = ProgressionsModel.SelectedIndex;
        Debug.Assert(selectedIndex != -1);
        Debug.Assert(selectedIndex == ProgressionsModel_.SelectedIndex);

        ProgressionsModel.DetachSnapshots();
        DetachProgressionsModel();
        InitializeParam.Merge(ProgressionsModel_.Progressions[selectedIndex], ProgressionsModel.Progressions[selectedIndex]);
        AttachProgressionsModel();
        ProgressionsModel.AttachSnapshots();

        //RespondEvent?.Invoke(json);
    }

    #region Recent Files

    const int MaxRecentFiles = 10;

    [ObservableProperty]
    private ObservableCollection<MenuItem> recentFileMenuItems = new ObservableCollection<MenuItem>();

    // from https://www.codeguru.com/dotnet/creating-a-most-recently-used-menu-list-in-net/
    // it was full of bugs so I ended up rewriting much of it
    private List<string> MRUlist = new List<string>();

    // C:\Users\philip\AppData\Local
    // TODO: create an app folder C:\Users\philip\AppData\Local\TryCommunityToolkitMvvm
    private string GetPath() =>
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"RecentFiles.txt");

    /// <summary>
    ///  Load Recent File list
    /// </summary>
    private void LoadRecentList()
    {
        MRUlist.Clear();

        try
        {
            if (!File.Exists(GetPath()))
            {
                return;
            }
            using (StreamReader srStream = new StreamReader(GetPath()))
            {
                string strLine = string.Empty;
                while ((InlineAssignHelper(ref strLine,
                      srStream.ReadLine())) != null)
                    MRUlist.Add(strLine);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error");
        }
        GenerateMenuItems();
    }
    /// <summary>
    ///  Generate Recent File MenuItems
    /// </summary>
    private void GenerateMenuItems()
    {
        foreach (string strItem in MRUlist)
        {
            var item = new MenuItem { Header = strItem, Command = FileRecentFileCommand };
            // TODO: better to do this in XAML
            item.CommandParameter = item;
            RecentFileMenuItems.Add(item);
        }
    }
    /// <summary>
    ///  Save Recent File list
    /// </summary>
    private void SaveRecentFile(string fileName)
    {
        var list = new List<string>{ fileName};
        list.AddRange(MRUlist.Where(x => x != fileName));
        MRUlist = new List<string>(list.Take(MaxRecentFiles));

        RecentFileMenuItems.Clear();

        GenerateMenuItems();

        try
        {
            using (StreamWriter stringToWrite = new StreamWriter(GetPath()))
            {
                foreach (string item in MRUlist)
                    stringToWrite.WriteLine(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error");
        }
    }
    /// <summary>
    /// Command for dynamically generated MenuItems
    /// </summary>
    /// <param name="menuItem">MenuItem itself used as Command parameter</param>
    private void FileRecentFileAction(MenuItem? menuItem)
    {
        FileRecentFile(menuItem?.Header.ToString() ?? string.Empty);
    }

    private static T InlineAssignHelper<T>(ref T target, T value)
    {
        target = value;
        return value;
    }

    #endregion
    private void DetachProgressionsModel()
    {
        if (ProgressionsModel != null)
        {
            ProgressionsModel.PropertyChanged -= ProgressionsModel_PropertyChanged;
        }
    }
    private void AttachProgressionsModel()
    {
        ProgressionsModel.PropertyChanged += ProgressionsModel_PropertyChanged;
    }

    /// <summary>
    /// Send progression parameters to Score for redrawing when certain properties change
    /// </summary>
    private void ProgressionsModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var propertyName = e.PropertyName;
        if (ShouldSend())
        {
            if (ProgressionParam.propertyNames.Contains(propertyName))
            {
                Debug.WriteLine($"Send() {propertyName}");
                Send();
            }
            else
            {
                Debug.WriteLine($"Send() {propertyName} filtered");
            }
        }
        else
        {
            Debug.WriteLine($"Send() should not send");
        }
    }
    /// <summary>
    /// Determine whether parameters are in a condition that they should be sent to the Score
    /// </summary>
    private bool ShouldSend()
    {
        return ProgressionsModel.SelectedIndex != -1 &&
            ProgressionsModel.SelectedIndex < ProgressionsModel.Progressions.Count &&
            ProgressionsModel.Progressions[ProgressionsModel.SelectedIndex].SnapshotSelections.Count > 0;
    }
}
