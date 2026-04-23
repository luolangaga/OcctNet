using Avalonia.Controls;
using OcctNet.Controls;
using OcctNet.Models;
using OcctNet.ViewModels;

namespace OcctNet.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Viewport.StatusChanged += Viewport_OnStatusChanged;
        Viewport.SetTool(SketchTool.Select);
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private void SelectTool_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetTool(SketchTool.Select, "选择");
    }

    private void LineTool_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetTool(SketchTool.Line, "直线");
    }

    private void RectangleTool_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetTool(SketchTool.Rectangle, "矩形");
    }

    private void PushPullTool_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetTool(SketchTool.PushPull, "推拉");
    }

    private void MeasureTool_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetTool(SketchTool.Measure, "测量");
    }

    private void ClearScene_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Viewport.Clear();
        UpdateEntityCount();
    }

    private void PushPullUp_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Viewport.PushPullSelected(1);
        UpdateEntityCount();
    }

    private void PushPullDown_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Viewport.PushPullSelected(-1);
        UpdateEntityCount();
    }

    private void SetTool(SketchTool tool, string displayName)
    {
        Viewport.SetTool(tool);

        if (ViewModel is not null)
        {
            ViewModel.ActiveTool = displayName;
        }
    }

    private void Viewport_OnStatusChanged(object? sender, SketchStatusChangedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.StatusMessage = e.Message;
        ViewModel.EntityCount = e.EntityCount;
    }

    private void UpdateEntityCount()
    {
        if (ViewModel is not null)
        {
            ViewModel.EntityCount = Viewport.Document.EntityCount;
        }
    }
}
