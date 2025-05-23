using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using desktop_application.win32api;

namespace desktop_application.Views.FileLock;

public partial class FileLockView : UserControl {
    public FileLockView() {
        InitializeComponent();

        DragDrop.SetAllowDrop(DropZone, true);

        DropZone.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        DropZone.AddHandler(DragDrop.DragOverEvent, DragOver);
        DropZone.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        DropZone.AddHandler(DragDrop.DropEvent, Drop);
    }


    private void ChangeStyle() {
        DropZone.Background = new SolidColorBrush(Color.Parse("#4000AAFF"));
        DropZone.BorderBrush = new SolidColorBrush(Color.Parse("#FF0078D7"));
        DropZone.BorderThickness = new Thickness(2);
    }

    private void CannelStyle() {
        DropZone.Background = new SolidColorBrush(Color.Parse("#F0F0F0"));
        DropZone.BorderBrush = new SolidColorBrush(Color.Parse("Gray"));
        DropZone.BorderThickness = new Thickness(2);
    }

    private void DragEnter(object? sender, DragEventArgs e) {
        ChangeStyle();
        e.DragEffects = IsFileDrag(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void DragOver(object? sender, DragEventArgs e) {
        e.DragEffects = IsFileDrag(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void DragLeave(object? sender, RoutedEventArgs e) {
        CannelStyle();
    }

    private void Drop(object? sender, DragEventArgs e) {
        CannelStyle();

        if (IsFileDrag(e.Data)) {
            var files = e.Data.GetFiles()?.Select(f => f.Path.AbsolutePath).ToList();
            if (files != null) {
                Console.WriteLine($"Dropped {files[0]} file(s)");

            }
        }

        e.Handled = true;
    }

    private static bool IsFileDrag(IDataObject data) {
        return data.Contains(DataFormats.Files) || data.Contains(DataFormats.FileNames);
    }
}