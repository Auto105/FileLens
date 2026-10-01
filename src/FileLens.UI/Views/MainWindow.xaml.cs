using FileLens.UI.ViewModels;

namespace FileLens.UI.Views;

/// <summary>
/// Hosts the primary application shell window.
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The shell view model supplied by dependency injection.</param>
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        DataContext = viewModel;
    }
}
