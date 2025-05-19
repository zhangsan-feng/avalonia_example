using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using desktop_application.ViewModels;

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

    private void DragEnter(object? sender, DragEventArgs e) {
        DropZone.Classes.Add("dragover");
        if (IsFileDrag(e.Data)) {
            e.DragEffects = DragDropEffects.Copy;
        }
        else {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void DragOver(object? sender, DragEventArgs e) {
 
        if (IsFileDrag(e.Data)) {
            e.DragEffects = DragDropEffects.Copy;
        }
        else {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void DragLeave(object? sender, RoutedEventArgs e) {
        DropZone.Classes.Remove("dragover");
    }

    private void Drop(object? sender, DragEventArgs e) {
 
        DropZone.Classes.Remove("dragover");

        if (IsFileDrag(e.Data)) {
            var files = e.Data.GetFiles()?.Select(f => f.Path.LocalPath).ToList();
            if (files != null) {
                Console.WriteLine($"Dropped {files.Count} file(s)");
                ProcessDroppedFiles(files);
            }
        }

        e.Handled = true;
    }

    private bool IsFileDrag(IDataObject data) {
        return data.Contains(DataFormats.Files) || data.Contains(DataFormats.FileNames);
    }
    
    public void ProcessDroppedFiles(List<string> filePaths) {
        Console.WriteLine($"Processing files from {filePaths.Count} file(s)");
        foreach (var filePath in filePaths) {
            var processes = GetProcessesLockingFile(filePath);
            Console.WriteLine($"Processes: {processes.Name}");

        }
    }

    private List<string> GetProcessesLockingFile(string filePath) {
        var processes = new List<string>();
        try {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
                fs.Close();
            }
        }
        catch (IOException) {
            processes.AddRange(System.Diagnostics.Process.GetProcesses()
                .Where(p => !p.HasExited && IsFileLockedByProcess(p, filePath))
                .Select(p => p.ProcessName));
        }

        return processes;
    }

    private bool IsFileLockedByProcess(System.Diagnostics.Process process, string filePath) {
        try {
            return process.Modules.Cast<System.Diagnostics.ProcessModule>()
                .Any(m => m.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        }
        catch {
            return false;
        }
    }
}