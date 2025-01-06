using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Luban.GUI.Models;

namespace Luban.GUI;

public partial class MainWindow : Window
{
    StringWriter stringWriter;
    DispatcherTimer timer;

    public MainWindow()
    {
        InitializeComponent();
        Width = 1024;
        Height = 600;
        MaxWidth = 1024;
        stringWriter = new StringWriter();
        stringWriter.GetStringBuilder().Capacity = 1024 * 64;
        Console.SetOut(stringWriter);
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200), // 每100毫秒检查一次
        };
        timer.Tick += Timer_Tick;
        SettingData.LoadSetting();
        ErrorLog.SyntaxHighlighting = new LogHighlightingDefinition();

        var options = SettingData.Instance.Options;
        if (options != null)
        {
            this.LuBanPath.Text = options.ConfigFile;
            this.ClientOutputDataDir.Text = options.ClientDataTarget;
            this.ClientOutputCodeDir.Text = options.ClientCodeTarget;
            this.ClientLocalizationDir.Text = options.ClientLocalizationPath;

            this.ServerOutputDataDir.Text = options.ServerDataTarget;
            this.ServerOutputCodeDir.Text = options.ServerCodeTarget;
        }
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        // 捕捉 Console 输出
        var output = stringWriter.ToString();
        ErrorLog.Text = output;
        ErrorLogScroll.ScrollToEnd();
    }

    private void HelpButton_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://gameframex.doc.alianblank.com/config/gui.html", UseShellExecute = true // 使用系统外壳来打开 URL
        });
    }

    private void Save()
    {
        SettingData.Instance.Options.ConfigFile = this.LuBanPath.Text;
        SettingData.Instance.Options.ClientDataTarget = this.ClientOutputDataDir.Text;
        SettingData.Instance.Options.ClientCodeTarget = this.ClientOutputCodeDir.Text;
        SettingData.Instance.Options.ServerDataTarget = this.ServerOutputDataDir.Text;
        SettingData.Instance.Options.ServerCodeTarget = this.ServerOutputCodeDir.Text;
        SettingData.SaveSetting();
    }

    void Start()
    {
        GenerateClientBinaryButton.IsEnabled = false;
        GenerateClientJsonButton.IsEnabled = false;
        GenerateServerBinaryButton.IsEnabled = false;
        GenerateServerJsonButton.IsEnabled = false;
    }

    void End()
    {
        GenerateClientBinaryButton.IsEnabled = true;
        GenerateClientJsonButton.IsEnabled = true;
        GenerateServerBinaryButton.IsEnabled = true;
        GenerateServerJsonButton.IsEnabled = true;
    }

    private async Task Run(string[] args)
    {
        stringWriter.GetStringBuilder().Clear();
        timer.Start();
        Start();
        await Task.Run(() =>
        {
            LauncherHelper.Start(args);
        });

        await Task.Delay(300);
        timer.Stop();
        End();
    }

    /// <summary>
    /// 生成客户端json
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GenerateClientJsonButton_OnClick(object sender, RoutedEventArgs e)
    {
        Save();
        await Run(SettingData.GetClientArgs(false));
    }

    /// <summary>
    /// 生成客户端二进制
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GenerateClientBinaryButton_OnClick(object sender, RoutedEventArgs e)
    {
        Save();
        await Run(SettingData.GetClientArgs(true));
    }

    /// <summary>
    /// 生成服务端二进制
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GenerateServerBinaryButton_OnClick(object sender, RoutedEventArgs e)
    {
        Save();
        await Run(SettingData.GetServerArgs(true));
    }

    /// <summary>
    /// 生成服务端json
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GenerateServerJsonButton_OnClick(object sender, RoutedEventArgs e)
    {
        Save();
        await Run(SettingData.GetServerArgs(false));
    }
}
