using System.Windows;

namespace H.ProxyFactory.Apps.Wpf;

public partial class ServerWindow
{
    #region Constants

    public const string ServerName = "H.ProxyFactory.Apps.Wpf";

    #endregion

    #region Properties

    private PipeProxyServer Server { get; }

    #endregion

    #region Constructors

    public ServerWindow()
    {
        InitializeComponent();

        Server = new PipeProxyServer();
        Server.ExceptionOccurred += (_, exception) =>
        {
            WriteLine($"{nameof(Server.ExceptionOccurred)}: {exception}");
        };
        Server.MessageReceived += (_, message) =>
        {
            WriteLine($"{nameof(Server.MessageReceived)}: {message}");
        };
    }

    #endregion

    #region Methods

    public void WriteLine(string text)
    {
        Dispatcher.Invoke(() =>
            ConsoleTextBox.Text += text + Environment.NewLine);
    }

    #endregion

    #region Event Handlers

    private async void Window_Loaded(object _, RoutedEventArgs e)
    {
        await Server.InitializeAsync(ServerName).ConfigureAwait(false);

        WriteLine($"Initilized");
    }

    private async void Window_Unloaded(object _, RoutedEventArgs e)
    {
        await Server.DisposeAsync().ConfigureAwait(false);
    }

    #endregion

}
