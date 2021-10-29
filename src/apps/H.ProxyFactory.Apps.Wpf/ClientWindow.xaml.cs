using System.Windows;
using H.ProxyFactory.TestTypes;

namespace H.ProxyFactory.Apps.Wpf;

public partial class ClientWindow
{
    #region Properties

    private PipeProxyFactory Factory { get; }
    private ISimpleEventClass? Instance { get; set; }

    #endregion

    #region Constructors

    public ClientWindow()
    {
        InitializeComponent();

        Factory = new PipeProxyFactory();
        Factory.MethodCalled += (_, args) =>
        {
            WriteLine($"{nameof(Factory.MethodCalled)}: {args.MethodInfo.Name}");
        };
        Factory.ExceptionOccurred += (_, exception) =>
        {
            WriteLine($"{nameof(Factory.ExceptionOccurred)}: {exception}");
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
        try
        {
            await Factory.InitializeAsync(ServerWindow.ServerName).ConfigureAwait(false);

            WriteLine($"Initilized");

            Instance = await Factory.CreateInstanceAsync<ISimpleEventClass>(
                typeof(SimpleEventClass).FullName ?? string.Empty).ConfigureAwait(false);
            Instance.Event1 += (_, args) =>
            {
                WriteLine($"{nameof(Instance.Event1)}: {args}");
            };
            Instance.Event3 += text =>
            {
                WriteLine($"{nameof(Instance.Event3)}: {text}");
            };

            WriteLine($"Instance created");
        }
        catch (Exception exception)
        {
            WriteLine($"{nameof(Window_Loaded)}: {exception}");
        }
    }

    private async void Window_Unloaded(object _, RoutedEventArgs e)
    {
        try
        {
            await Factory.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            WriteLine($"{nameof(Window_Unloaded)}: {exception}");
        }
    }

    private void RaiseEvent1Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Instance == null)
            {
                return;
            }

            Instance.RaiseEvent1();
            WriteLine($"{nameof(Instance.RaiseEvent1)}");
        }
        catch (Exception exception)
        {
            WriteLine($"{nameof(RaiseEvent1Button_Click)}: {exception}");
        }
    }

    private void RaiseEvent3Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Instance == null)
            {
                return;
            }

            Instance.RaiseEvent3();
            WriteLine($"{nameof(Instance.RaiseEvent3)}");
        }
        catch (Exception exception)
        {
            WriteLine($"{nameof(RaiseEvent3Button_Click)}: {exception}");
        }
    }

    private void Method1Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Instance == null)
            {
                return;
            }

            var result = Instance.Method1(123);
            WriteLine($"{nameof(Instance.Method1)}: {result}");
        }
        catch (Exception exception)
        {
            WriteLine($"{nameof(Method1Button_Click)}: {exception}");
        }
    }

    private void Method2Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Instance == null)
            {
                return;
            }

            var result = Instance.Method2(Method2ArgumentTextBox.Text);
            WriteLine($"{nameof(Instance.Method2)}: {result}");
        }
        catch (Exception exception)
        {
            WriteLine($"{nameof(Method2Button_Click)}: {exception}");
        }
    }

    #endregion
}
