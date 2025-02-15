using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreLibrary.Parameter;
/// <summary>
/// Represents the list of progressions and the selected progression
/// </summary>
public partial class ProgressionsParam : ObservableObject
{
    public void AddData()
    {
        var progressionParam = InitializeParam.CreateProgressionParam();
        progressionParam.Progression = "0 17 12 4, 0 5 12 19, 7 19 12 -1B, //NumberBase12";
        Progressions.Add(progressionParam);
        progressionParam = InitializeParam.CreateProgressionParam();
        progressionParam.Progression = "0 17 12 4, 0 5 12 19, 7 19 12 -1B, //NumberBase12";
        Progressions.Add(progressionParam);
    }
    public ProgressionsParam()
    {

    }

    [ObservableProperty]
    private ObservableCollection<ProgressionParam> progressions = new ObservableCollection<ProgressionParam>();

    // use SelectedIndex instead of SelectedItem so it can be serialized
    [ObservableProperty]
    private int selectedIndex = -1;

    //[ObservableProperty]
    //[property: System.Text.Json.Serialization.JsonIgnore]
    //private ProgressionParam? selectedItem = null;

    //partial void OnSelectedIndexChanged(int value)
    //{
    //    Debug.WriteLine($"ProgressionsParam.SelectedIndexChanged: {value}");
    //}
    //partial void OnSelectedIndexChanging(int oldValue, int newValue)
    //{
    //    Debug.WriteLine($"ProgressionsParam.SelectedIndexChanging: {oldValue} {newValue}");
    //}

    #region PropertyChanged
    public void DetachSnapshots()
    {
        foreach (var p in Progressions)
        {
            p.DetachSnapshots();
        }
        for (var i = 0; i < Progressions.Count; i++)
        {
            Progressions[i].HandlerCount--;
            Progressions[i].PropertyChanged -= ProgressionsParam_PropertyChanged;
        }
    }
    public void AttachSnapshots()
    {
        foreach (var p in Progressions)
        {
            p.AttachSnapshots();
        }
        for (var i = 0; i < Progressions.Count; i++)
        {
            Progressions[i].HandlerCount++;
            Progressions[i].PropertyChanged += ProgressionsParam_PropertyChanged;
        }
    }

    /// <summary>
    /// Send progression parameters to Score for redrawing when certain properties change
    /// </summary>
    private void ProgressionsParam_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (ProgressionParam.propertyNames.Contains(e.PropertyName))
        {
            OnPropertyChanged(e.PropertyName);
        }
        else
        {
            Debug.WriteLine($"{e.PropertyName} filtered");
        }
    }
    #endregion
    #region overrides
    // TODO: verify there are not used
    //public string FormatForHashCode() => string.Join(" ", Progressions.Select(p => p.FormatForHashCode()));
    public string FormatForHashCode()
    {
        return string.Join(" ", Progressions.Select(p => p.FormatForHashCode()));
    }
    //public override int GetHashCode() => FormatForHashCode().GetHashCode();
    public int MyGetHashCode() => FormatForHashCode().GetHashCode();
    #endregion
}
