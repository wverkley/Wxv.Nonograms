using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Wxv.Nonograms.IO.Importers;

namespace Wxv.Nonograms.UX;

public partial class ImportDialog : Window
{
    public object Data { get; set; } = string.Empty;
    public NonogrameImporterResult? NonogrameImporterResult { get; private set; } 

    private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

    public ImportDialog()
    {
        InitializeComponent();
    }
    
    private void ImportDialog_OnLoaded(object sender, RoutedEventArgs e)
    {
        Task.Run(Import);
    }

    private void ImportDialog_OnUnloaded(object sender, RoutedEventArgs e)
    {
        CancellationTokenSource.Cancel();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CancellationTokenSource.Cancel();
        DialogResult = false;
        Close();
    }

    private async Task Import()
    {
        var importers = Registry.Importers;

        NonogrameImporterResult? result = null;
        var index = 0;
        foreach (var importer in importers)
        {
            index++;

            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = index / (double) importers.Count * 100.0; 
                ProgressTextBox.Text = importer.Name;
            });

            try
            {
                result = await importer.TryImportAsync(Data, CancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                continue;
            }

            if (result.Value.IsSuccess || CancellationTokenSource.IsCancellationRequested)
                break;
        }

        NonogrameImporterResult = CancellationTokenSource.IsCancellationRequested ? null : result;

        Dispatcher.Invoke(() =>
        {
            DialogResult = NonogrameImporterResult.HasValue;
            Close();
        });
    }


}