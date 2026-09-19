using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace voidscreecopy;

public partial class MainWindow : Window
{
    private const string ScreenTitle = "OBS_TAB_SCREEN";
    private const string CameraTitle = "OBS_MOBILE_CAMERA";
    private readonly string _appDir;
    private readonly string _toolsDir;
    private readonly string _adbPath;
    private readonly string _scrcpyPath;
    private readonly string _settingsPath;
    private readonly List<Process> _launched = [];
    private List<AndroidDevice> _readyDevices = [];
    private List<TvTarget> _tvTargets = [];
    private UpdateManifest? _pendingUpdate;
    private string _activePage = "screen";

    public MainWindow()
    {
        InitializeComponent();

        _appDir = AppContext.BaseDirectory;
        _toolsDir = Path.Combine(_appDir, "tools", "scrcpy-portable");
        _adbPath = Path.Combine(_toolsDir, "adb.exe");
        _scrcpyPath = Path.Combine(_toolsDir, "scrcpy.exe");
        _settingsPath = Path.Combine(_appDir, "update-settings.json");

        ScreenQualityCombo.ItemsSource = ScreenPreset.All;
        ScreenQualityCombo.SelectedIndex = 0;
        CameraQualityCombo.ItemsSource = CameraPreset.All;
        CameraQualityCombo.SelectedIndex = 1;
        CameraQualityCombo2.ItemsSource = CameraPreset.All;
        CameraQualityCombo2.SelectedIndex = 1;
        CameraFacingCombo.ItemsSource = new[] { "back", "front" };
        CameraFacingCombo.SelectedIndex = 0;

        SettingsVersionText.Text = CurrentVersion.ToString();
        ManifestUrlBox.Text = new UpdateSettings().ManifestUrl;
        NavScreen_Click(null!, null!);

        InitAuth();
        LoginPage.Visibility = Visibility.Visible;

        Loaded += async (_, _) =>
        {
            AddLog("voidscreecopy v" + CurrentVersion + " started.");
        };
    }

    private static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(3, 0, 0);

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            MaximizeButton_Click(sender!, e);
        else
            DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        StopAll();
        Close();
    }

    private void NavScreen_Click(object sender, RoutedEventArgs e) => SwitchPage("screen");
    private void NavCamera_Click(object sender, RoutedEventArgs e) => SwitchPage("camera");
    private void NavTv_Click(object sender, RoutedEventArgs e) => SwitchPage("tv");
    private void NavSettings_Click(object sender, RoutedEventArgs e) => SwitchPage("settings");

    private void SwitchPage(string page)
    {
        _activePage = page;
        AdminPage.Visibility = Visibility.Collapsed;
        ScreenPage.Visibility = page == "screen" ? Visibility.Visible : Visibility.Collapsed;
        CameraPage.Visibility = page == "camera" ? Visibility.Visible : Visibility.Collapsed;
        TvPage.Visibility = page == "tv" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPage.Visibility = page == "settings" ? Visibility.Visible : Visibility.Collapsed;

        var (title, subtitle) = page switch
        {
            "screen" => ("Screen Mirroring", "Share your Android device screen to OBS"),
            "camera" => ("Camera Sharing", "Share your Android device camera to OBS"),
            "tv" => ("TV Connection", "Connect to Android TV over Wi-Fi or Chromecast"),
            "settings" => ("Settings", "Application settings and updates"),
            _ => ("", "")
        };
        PageTitle.Text = title;
        PageSubtitle.Text = subtitle;

        HighlightSidebarButton(page);
    }

    private void HighlightSidebarButton(string page)
    {
        var indicators = new Dictionary<string, System.Windows.Controls.Border?>
        {
            ["screen"] = NavScreenIndicator,
            ["camera"] = NavCameraIndicator,
            ["tv"] = NavTvIndicator,
            ["settings"] = NavSettingsIndicator,
            ["admin"] = NavAdminIndicator
        };

        foreach (var kv in indicators)
        {
            if (kv.Value != null)
                kv.Value.Visibility = kv.Key == page ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await RefreshDevicesAsync();
    private async void ScanTvButton_Click(object sender, RoutedEventArgs e) => await ScanTvsAsync(true);
    private async void ConnectTvButton_Click(object sender, RoutedEventArgs e) => await ConnectSelectedTvAsync();
    private void OpenCastButton_Click(object sender, RoutedEventArgs e) => OpenChromecast();
    private async void FixAdbButton_Click(object sender, RoutedEventArgs e) => await FixAdbAsync();
    private void StopAllButton_Click(object sender, RoutedEventArgs e) => StopAll();
    private void StartButton_Click(object sender, RoutedEventArgs e) => StartObsShare();
    private void StartCameraButton_Click(object sender, RoutedEventArgs e) => StartCameraShare();
    private void CameraQualityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateQualityWarning();
    private async void CameraSizesButton_Click(object sender, RoutedEventArgs e) => await ListCameraSizesAsync();
    private async void CheckUpdateButton_Click(object sender, RoutedEventArgs e) => await CheckForUpdateAsync();
    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e) => ToggleTheme();

    private bool _isDarkMode;

    private void ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
        ApplyTheme(_isDarkMode);
        ThemeIcon.Text = _isDarkMode ? "\uE708" : "\uE706";
        ThemeLabel.Text = _isDarkMode ? "Light" : "Dark";
    }

    private void ApplyTheme(bool dark)
    {
        var res = Resources;
        if (dark)
        {
            res["WindowBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 18, 24));
            res["SidebarBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 24, 32));
            res["MainBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 18, 24));
            res["PanelBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 24, 32));
            res["CardBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 40));
            res["TextPrimary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 240));
            res["TextSecondary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 165, 180));
            res["TextMuted"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 105, 120));
            res["BorderLight"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 58));
            res["SidebarHover"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 52));
            res["SidebarActive"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 30, 50));
            res["ButtonSecondary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 52));
            res["LogBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 22, 30));
            res["TitleBarBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 24, 32));
            res["CardHover"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(35, 35, 48));
            res["IconBgBlue"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 30, 55));
            res["IconBgOrange"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 35, 15));
            res["IconBgPurple"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(35, 25, 55));
            res["IconBgNeutral"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(35, 35, 48));
            res["IconBgGreen"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 40, 25));
            res["StatusGreenBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 40, 25));
            res["StatusGreenText"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(74, 222, 128));
            res["UpdateBannerBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(13, 31, 45));
            res["UpdateBannerBorder"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 58, 80));
            res["UpdateBannerIconBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 45, 74));
            res["ProgressBarBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 65));
            res["WindowButtonHover"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 65));
            res["SecondaryBtnHover"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(55, 55, 70));
            res["SecondaryBtnPressed"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 75));
        }
        else
        {
            res["WindowBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 247, 250));
            res["SidebarBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            res["MainBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 247, 250));
            res["PanelBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            res["CardBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            res["TextPrimary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 35, 50));
            res["TextSecondary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 107, 127));
            res["TextMuted"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 156, 182));
            res["BorderLight"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(226, 232, 240));
            res["SidebarHover"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 244, 248));
            res["SidebarActive"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 245, 255));
            res["ButtonSecondary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 237, 242));
            res["LogBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 250, 252));
            res["TitleBarBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            res["CardHover"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 244, 248));
            res["IconBgBlue"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 245, 255));
            res["IconBgOrange"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 230));
            res["IconBgPurple"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 238, 255));
            res["IconBgNeutral"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 244, 248));
            res["IconBgGreen"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
            res["StatusGreenBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 253, 244));
            res["StatusGreenText"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 101, 52));
            res["UpdateBannerBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 248, 255));
            res["UpdateBannerBorder"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(179, 229, 252));
            res["UpdateBannerIconBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(219, 234, 254));
            res["ProgressBarBg"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224));
            res["WindowButtonHover"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 237, 242));
            res["SecondaryBtnHover"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(213, 221, 230));
            res["SecondaryBtnPressed"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(194, 204, 214));
        }
    }

    private void AddLog(string message)
    {
        Dispatcher.Invoke(() =>
        {
            LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            LogBox.ScrollToEnd();
        });
    }

    private async Task<string> RunToolAsync(string fileName, string args, int timeoutMs = 15000)
    {
        if (!File.Exists(fileName))
        {
            return $"ERROR: Missing tool: {fileName}";
        }

        var psi = new ProcessStartInfo(fileName, args)
        {
            WorkingDirectory = Path.GetDirectoryName(fileName) ?? _appDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            return "ERROR: Could not start process.";
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        var waitTask = process.WaitForExitAsync();
        var finished = await Task.WhenAny(waitTask, Task.Delay(timeoutMs));
        if (finished != waitTask)
        {
            try { process.Kill(true); } catch { }
            return "ERROR: Process timed out.";
        }

        return (await outputTask + await errorTask).Trim();
    }

    private async Task RefreshDevicesAsync()
    {
        AddLog("Refreshing ADB devices...");
        var output = await RunToolAsync(_adbPath, "devices -l");
        var devices = ParseDevices(output);
        _readyDevices = devices.Where(d => d.Status.Equals("device", StringComparison.OrdinalIgnoreCase)).ToList();

        DeviceList.ItemsSource = devices;
        DeviceList2.ItemsSource = devices;
        ScreenDeviceCombo.ItemsSource = _readyDevices;
        CameraDeviceCombo.ItemsSource = _readyDevices;
        CameraDeviceCombo2.ItemsSource = _readyDevices;
        if (ScreenDeviceCombo.SelectedIndex < 0 && _readyDevices.Count > 0) ScreenDeviceCombo.SelectedIndex = 0;
        if (CameraDeviceCombo.SelectedIndex < 0 && _readyDevices.Count > 1) CameraDeviceCombo.SelectedIndex = 1;
        else if (CameraDeviceCombo.SelectedIndex < 0 && _readyDevices.Count > 0) CameraDeviceCombo.SelectedIndex = 0;
        if (CameraDeviceCombo2.SelectedIndex < 0 && _readyDevices.Count > 1) CameraDeviceCombo2.SelectedIndex = 1;
        else if (CameraDeviceCombo2.SelectedIndex < 0 && _readyDevices.Count > 0) CameraDeviceCombo2.SelectedIndex = 0;

        Dispatcher.Invoke(() =>
        {
            var readyCount = _readyDevices.Count;
            ConnectionStatusText.Text = readyCount > 0
                ? $"{readyCount} device(s) connected"
                : "No devices connected";
        });

        if (devices.Count == 0)
        {
            AddLog("No Android devices found. Connect USB or connect TV over Wi-Fi.");
        }
        else
        {
            AddLog($"ADB sees {devices.Count} device(s), {_readyDevices.Count} ready.");
        }
    }

    private static List<AndroidDevice> ParseDevices(string output)
    {
        var result = new List<AndroidDevice>();
        foreach (var raw in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = Regex.Split(line, @"\s+");
            if (parts.Length < 2) continue;

            var model = parts.FirstOrDefault(p => p.StartsWith("model:", StringComparison.OrdinalIgnoreCase))?.Split(':').LastOrDefault() ?? "Android";
            var product = parts.FirstOrDefault(p => p.StartsWith("product:", StringComparison.OrdinalIgnoreCase))?.Split(':').LastOrDefault() ?? "";
            result.Add(new AndroidDevice(parts[0], parts[1], model, product));
        }

        return result;
    }

    private async Task FixAdbAsync()
    {
        AddLog("Restarting ADB...");
        await RunToolAsync(_adbPath, "kill-server", 10000);
        await RunToolAsync(_adbPath, "start-server", 10000);
        await RefreshDevicesAsync();
    }

    private async Task ScanTvsAsync(bool deep)
    {
        TvHelpText.Text = "Scanning for Android TV ADB targets on the same Wi-Fi...";
        AddLog(deep ? "Deep scanning Wi-Fi TVs..." : "Checking Wi-Fi TV discovery...");

        var targets = new List<TvTarget>();
        targets.AddRange(await DiscoverMdnsTvsAsync());
        if (deep)
        {
            targets.AddRange(await DiscoverPort5555TvsAsync());
        }

        _tvTargets = targets
            .GroupBy(t => t.Address, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(t => t.Name)
            .ToList();

        TvList.ItemsSource = _tvTargets;
        if (_tvTargets.Count > 0)
        {
            TvList.SelectedIndex = 0;
            TvHelpText.Text = $"Found {_tvTargets.Count} TV/Wi-Fi target(s). Select your TV name and click Connect TV.";
            AddLog($"Found {_tvTargets.Count} Wi-Fi target(s).");
        }
        else
        {
            TvHelpText.Text = "No TV found. Put PC and Android/Google TV on same Wi-Fi, enable Developer Options and Network/Wireless Debugging on TV, then scan again.";
            AddLog("No Wi-Fi TV found.");
        }
    }

    private async Task<List<TvTarget>> DiscoverMdnsTvsAsync()
    {
        var output = await RunToolAsync(_adbPath, "mdns services", 12000);
        var targets = new List<TvTarget>();
        foreach (var raw in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            if (!line.Contains("_adb", StringComparison.OrdinalIgnoreCase)) continue;

            var endpoint = Regex.Match(line, @"(?<ip>(?:\d{1,3}\.){3}\d{1,3}):(?<port>\d{2,5})");
            if (!endpoint.Success) continue;

            var name = Regex.Split(line, @"\s+").FirstOrDefault() ?? "Android TV";
            targets.Add(new TvTarget(name, $"{endpoint.Groups["ip"].Value}:{endpoint.Groups["port"].Value}", "mDNS"));
        }

        return targets;
    }

    private async Task<List<TvTarget>> DiscoverPort5555TvsAsync()
    {
        var targets = new List<TvTarget>();
        var bases = GetLocalSubnetBases();
        foreach (var subnetBase in bases)
        {
            AddLog($"Scanning {subnetBase}.1-254:5555...");
            var throttler = new SemaphoreSlim(64);
            var tasks = Enumerable.Range(1, 254).Select(async host =>
            {
                await throttler.WaitAsync();
                try
                {
                    var ip = $"{subnetBase}.{host}";
                    if (await CanConnectAsync(ip, 5555, 250))
                    {
                        var name = await TryGetHostNameAsync(ip);
                        lock (targets)
                        {
                            targets.Add(new TvTarget(string.IsNullOrWhiteSpace(name) ? "Android TV / ADB" : name, $"{ip}:5555", "Wi-Fi scan"));
                        }
                    }
                }
                finally
                {
                    throttler.Release();
                }
            });
            await Task.WhenAll(tasks);
        }

        return targets;
    }

    private static List<string> GetLocalSubnetBases()
    {
        var bases = new List<string>();
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
            {
                continue;
            }

            foreach (var address in ni.GetIPProperties().UnicastAddresses)
            {
                if (address.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                var parts = address.Address.ToString().Split('.');
                if (parts.Length == 4 && parts[0] != "127")
                {
                    bases.Add($"{parts[0]}.{parts[1]}.{parts[2]}");
                }
            }
        }

        return bases.Distinct().ToList();
    }

    private static async Task<bool> CanConnectAsync(string ip, int port, int timeoutMs)
    {
        using var client = new TcpClient();
        try
        {
            var connectTask = client.ConnectAsync(ip, port);
            var winner = await Task.WhenAny(connectTask, Task.Delay(timeoutMs));
            return winner == connectTask && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<string> TryGetHostNameAsync(string ip)
    {
        try
        {
            var entry = await System.Net.Dns.GetHostEntryAsync(ip);
            return entry.HostName;
        }
        catch
        {
            return "";
        }
    }

    private async Task ConnectSelectedTvAsync()
    {
        if (TvList.SelectedItem is not TvTarget target)
        {
            AddLog("Select a TV first. Click Scan Wi-Fi TVs if the list is empty.");
            return;
        }

        AddLog($"Connecting to {target.Name} ({target.Address})...");
        var output = await RunToolAsync(_adbPath, $"connect {target.Address}", 20000);
        AddLog(output.Length == 0 ? "ADB connect finished." : output);
        await RefreshDevicesAsync();
    }

    private void OpenChromecast()
    {
        var browser = FindBrowser();
        if (browser == null)
        {
            AddLog("Chrome or Edge was not found. Install Chrome, then use Menu > Cast.");
            MessageBox.Show(
                "Chrome or Edge was not found.\n\nInstall Google Chrome, then open Chrome menu > Cast.",
                "voidscreecopy",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(browser, "--new-window chrome://cast")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            });
            AddLog("Opened Chromecast. In Chrome: Sources > Cast screen > choose your TV.");
            MessageBox.Show(
                "Chrome Cast opened.\n\nNow click:\n1. Sources\n2. Cast screen\n3. Your TV name",
                "Chromecast TV",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            AddLog($"Could not open Chromecast: {ex.Message}");
        }
    }

    private static string? FindBrowser()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var candidates = new[]
        {
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            Path.Combine(local, @"Google\Chrome\Application\chrome.exe"),
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private async void StartObsShare()
    {
        if (ScreenDeviceCombo.SelectedItem is not AndroidDevice screen)
        {
            AddLog("Choose a ready screen device first.");
            return;
        }

        if (CameraDeviceCombo.SelectedItem is not AndroidDevice camera)
        {
            AddLog("Choose a ready camera device first.");
            return;
        }

        var screenPreset = ScreenQualityCombo.SelectedItem as ScreenPreset ?? ScreenPreset.All[0];
        var cameraPreset = CameraQualityCombo.SelectedItem as CameraPreset ?? CameraPreset.All[1];
        var facing = CameraFacingCombo.SelectedItem?.ToString() ?? "back";

        StartButton.IsEnabled = false;
        AddLog("Starting OBS share...");

        await StartScrcpyAsync($"--serial={Quote(screen.Serial)} --video-bit-rate={screenPreset.BitRate} --max-size={screenPreset.MaxSize} --max-fps={screenPreset.Fps} --video-buffer=0 --no-audio --no-mipmaps --render-driver=direct3d --window-title={ScreenTitle}", ScreenTitle);

        await Task.Delay(1500);

        await StartScrcpyAsync($"--serial={Quote(camera.Serial)} --video-source=camera --camera-facing={facing} --camera-size={cameraPreset.Size} --camera-fps={cameraPreset.Fps} --video-bit-rate={cameraPreset.BitRate} --video-buffer=0 --no-audio --no-mipmaps --render-driver=direct3d --window-title={CameraTitle}", CameraTitle);

        StartButton.IsEnabled = true;
    }

    private async void StartCameraShare()
    {
        if (CameraDeviceCombo2.SelectedItem is not AndroidDevice camera)
        {
            AddLog("Choose a camera device first.");
            return;
        }

        var cameraPreset = CameraQualityCombo2.SelectedItem as CameraPreset ?? CameraPreset.All[1];
        var facing = CameraFacingCombo.SelectedItem?.ToString() ?? "back";

        StartCameraButton.IsEnabled = false;
        AddLog("Starting camera share...");

        await StartScrcpyAsync($"--serial={Quote(camera.Serial)} --video-source=camera --camera-facing={facing} --camera-size={cameraPreset.Size} --camera-fps={cameraPreset.Fps} --video-bit-rate={cameraPreset.BitRate} --video-buffer=0 --no-audio --no-mipmaps --render-driver=direct3d --window-title={CameraTitle}", CameraTitle);

        StartCameraButton.IsEnabled = true;
    }

    private async Task StartScrcpyAsync(string args, string title)
    {
        if (!File.Exists(_scrcpyPath))
        {
            AddLog($"Missing scrcpy.exe: {_scrcpyPath}");
            return;
        }

        var psi = new ProcessStartInfo(_scrcpyPath, args)
        {
            WorkingDirectory = _toolsDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        try
        {
            var process = Process.Start(psi);
            if (process == null)
            {
                AddLog($"Could not start {title}: process is null.");
                return;
            }

            _launched.Add(process);

            var stderr = await process.StandardError.ReadToEndAsync();
            var stdout = await process.StandardOutput.ReadToEndAsync();

            if (!process.HasExited)
            {
                AddLog($"Opened {title}.");
            }
            else
            {
                var exitCode = process.ExitCode;
                var errorDetail = !string.IsNullOrWhiteSpace(stderr) ? stderr : stdout;
                AddLog($"{title} exited immediately (code {exitCode}). {errorDetail}");
            }
        }
        catch (Exception ex)
        {
            AddLog($"Could not open {title}: {ex.Message}");
        }
    }

    private void StopAll()
    {
        foreach (var process in _launched.ToArray())
        {
            try
            {
                if (!process.HasExited) process.Kill(true);
            }
            catch { }
        }
        _launched.Clear();

        foreach (var process in Process.GetProcessesByName("scrcpy"))
        {
            try
            {
                if (!process.HasExited) process.Kill(true);
            }
            catch { }
        }

        AddLog("Stopped scrcpy windows launched by voidscreecopy.");
    }

    private async Task ListCameraSizesAsync()
    {
        var camera = _activePage == "camera"
            ? CameraDeviceCombo2.SelectedItem as AndroidDevice
            : CameraDeviceCombo.SelectedItem as AndroidDevice;

        if (camera == null)
        {
            AddLog("Choose a camera device first.");
            return;
        }

        AddLog("Listing camera sizes...");
        var output = await RunToolAsync(_scrcpyPath, $"--serial={Quote(camera.Serial)} --video-source=camera --list-camera-sizes --no-window --no-audio", 20000);
        AddLog(string.IsNullOrWhiteSpace(output) ? "No camera size output received." : output);
    }

    private void UpdateQualityWarning()
    {
        var selected = _activePage == "camera"
            ? CameraQualityCombo2.SelectedItem as CameraPreset
            : CameraQualityCombo.SelectedItem as CameraPreset;

        if (selected is { Name: "4K" })
        {
            QualityWarningText.Text = "4K can lag or fail on low-end devices. Use 2K for smooth sharing.";
            QualityWarning.Visibility = Visibility.Visible;
        }
        else
        {
            QualityWarning.Visibility = Visibility.Collapsed;
        }
    }

    private async Task CheckForUpdateAsync()
    {
        try
        {
            var settings = await UpdateSettings.LoadAsync(_settingsPath);
            if (!settings.AutoUpdate || string.IsNullOrWhiteSpace(settings.ManifestUrl) || settings.ManifestUrl.Contains("YOUR_GITHUB", StringComparison.OrdinalIgnoreCase))
            {
                Dispatcher.Invoke(() => UpdateStatusText.Text = "Update check skipped");
                return;
            }

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("voidscreecopy-updater/3.0");
            var json = await http.GetStringAsync(settings.ManifestUrl);
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(json, JsonOptions.Default);
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.Version) || string.IsNullOrWhiteSpace(manifest.PackageUrl))
            {
                AddLog("Update manifest is invalid.");
                return;
            }

            if (!Version.TryParse(manifest.Version, out var latest)) return;
            var current = CurrentVersion;

            if (latest <= current)
            {
                Dispatcher.Invoke(() => UpdateStatusText.Text = "Up to date");
                AddLog("No update found. Running v" + current);
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.SkippedVersion) &&
                settings.SkippedVersion.Equals(manifest.Version, StringComparison.OrdinalIgnoreCase))
            {
                AddLog($"Update v{manifest.Version} skipped by user.");
                return;
            }

            if (settings.RemindLaterUntil.HasValue && settings.RemindLaterUntil.Value > DateTime.UtcNow)
            {
                AddLog($"Update v{manifest.Version} deferred. Will remind after {settings.RemindLaterUntil:yyyy-MM-dd HH:mm}.");
                return;
            }

            _pendingUpdate = manifest;
            Dispatcher.Invoke(() =>
            {
                UpdateBanner.Visibility = Visibility.Visible;
                UpdateTitleText.Text = $"Update Available: v{manifest.Version}";
                UpdateNotesText.Text = string.IsNullOrWhiteSpace(manifest.Notes) ? "A new version is ready to install." : manifest.Notes;
                UpdateStatusText.Text = $"v{manifest.Version} available";
                UpdateNowButton.IsEnabled = true;
                UpdateProgressBar.Visibility = Visibility.Collapsed;
                UpdateProgressText.Visibility = Visibility.Collapsed;
            });
            AddLog($"Update v{manifest.Version} available (current: v{current}). Notes: {manifest.Notes}");
        }
        catch (Exception ex)
        {
            AddLog($"Update check: {ex.Message}");
        }
    }

    private async void UpdateNowButton_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingUpdate == null) return;
        UpdateNowButton.IsEnabled = false;
        RemindLaterButton.IsEnabled = false;
        SkipVersionButton.IsEnabled = false;

        try
        {
            await DownloadAndInstallUpdateAsync(_pendingUpdate);
        }
        catch (Exception ex)
        {
            AddLog($"Update failed: {ex.Message}");
            Dispatcher.Invoke(() =>
            {
                UpdateNowButton.IsEnabled = true;
                RemindLaterButton.IsEnabled = true;
                SkipVersionButton.IsEnabled = true;
                UpdateProgressBar.Visibility = Visibility.Collapsed;
                UpdateProgressText.Visibility = Visibility.Collapsed;
                UpdateProgressText.Text = "";
                MessageBox.Show($"Update failed: {ex.Message}\n\nThe current version will continue to work.",
                    "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }
    }

    private async void RemindLaterButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = await UpdateSettings.LoadAsync(_settingsPath);
        settings.RemindLaterUntil = DateTime.UtcNow.AddHours(4);
        await settings.SaveAsync(_settingsPath);
        UpdateBanner.Visibility = Visibility.Collapsed;
        AddLog("Update deferred for 4 hours.");
    }

    private async void SkipVersionButton_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingUpdate == null) return;
        var settings = await UpdateSettings.LoadAsync(_settingsPath);
        settings.SkippedVersion = _pendingUpdate.Version;
        settings.RemindLaterUntil = null;
        await settings.SaveAsync(_settingsPath);
        UpdateBanner.Visibility = Visibility.Collapsed;
        AddLog($"Skipped version {_pendingUpdate.Version}.");
    }

    private async Task DownloadAndInstallUpdateAsync(UpdateManifest manifest)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "voidscreecopy-update");
        Directory.CreateDirectory(tempDir);
        var installerPath = Path.Combine(tempDir, $"voidscreecopy-v{manifest.Version}.exe");

        Dispatcher.Invoke(() =>
        {
            UpdateProgressBar.Visibility = Visibility.Visible;
            UpdateProgressText.Visibility = Visibility.Visible;
            UpdateProgressText.Text = "Downloading update...";
            UpdateProgressBar.Value = 0;
        });

        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("voidscreecopy-updater/3.0");

        using (var response = await http.GetAsync(manifest.PackageUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(installerPath);
            var buffer = new byte[81920];
            long downloadedBytes = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                downloadedBytes += bytesRead;

                if (totalBytes > 0)
                {
                    var progress = (int)(downloadedBytes * 100 / totalBytes);
                    Dispatcher.Invoke(() =>
                    {
                        UpdateProgressBar.Value = progress;
                        UpdateProgressText.Text = $"Downloading... {progress}% ({downloadedBytes / 1024 / 1024:F1} MB / {totalBytes / 1024 / 1024:F1} MB)";
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                        UpdateProgressText.Text = $"Downloading... {downloadedBytes / 1024 / 1024:F1} MB");
                }
            }
        }

        Dispatcher.Invoke(() =>
        {
            UpdateProgressText.Text = "Verifying download...";
            UpdateProgressBar.IsIndeterminate = true;
        });

        if (!string.IsNullOrWhiteSpace(manifest.Sha256))
        {
            using var sha = SHA256.Create();
            await using var verifyStream = File.OpenRead(installerPath);
            var hash = await sha.ComputeHashAsync(verifyStream);
            var hashHex = Convert.ToHexString(hash).ToLowerInvariant();

            if (!hashHex.Equals(manifest.Sha256.Trim().ToLowerInvariant()))
            {
                try { File.Delete(installerPath); } catch { }
                throw new InvalidOperationException(
                    $"Checksum verification failed!\nExpected: {manifest.Sha256}\nGot: {hashHex}\n\nThe download may be corrupted.");
            }
            AddLog("Checksum verified OK.");
        }
        else
        {
            AddLog("No checksum in manifest, skipping verification.");
        }

        Dispatcher.Invoke(() =>
        {
            UpdateProgressText.Text = "Launching installer... App will close.";
        });

        var installDir = _appDir.TrimEnd('\\');
        var psi = new ProcessStartInfo(installerPath, $"/SILENT /NORESTART \"{installDir}\"")
        {
            WorkingDirectory = tempDir,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(psi);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            psi.Verb = "";
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        await Dispatcher.InvokeAsync(async () =>
        {
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(500);
                try { StopAll(); } catch { }
            }
            Application.Current.Shutdown();
        });
    }

    private static string Quote(string text) => text.Contains(' ') ? $"\"{text}\"" : text;

    // ==================== AUTH SYSTEM ====================
    private const string UsersJsonUrl = "https://raw.githubusercontent.com/voidhunterx30/voidscreecopy/main/users.json";
    private const string AdminEmail = "voidhunterx@gmail.com";
    private const string AdminPassword = "1212";
    private string _authSettingsPath = "";
    private List<UserEntry> _users = new();
    private bool _adminNewUserEnabled = true;
    private bool _isAdmin;

    private void InitAuth()
    {
        _authSettingsPath = Path.Combine(_appDir, "auth-settings.json");
        LoginPage.Visibility = Visibility.Visible;
    }

    private async Task<UsersFile?> LoadUsersFromGitHubAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("voidscreecopy-auth/1.0");
            var json = await http.GetStringAsync(UsersJsonUrl);
            return JsonSerializer.Deserialize<UsersFile>(json, JsonOptions.Default);
        }
        catch (Exception ex)
        {
            AddLog($"Failed to load users from GitHub: {ex.Message}");
            return null;
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var email = LoginEmail.Text.Trim();
        var password = LoginPassword.Password.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            LoginError.Text = "Please enter email and password.";
            LoginError.Visibility = Visibility.Visible;
            return;
        }

        LoginError.Visibility = Visibility.Collapsed;
        LoginLoading.Visibility = Visibility.Visible;
        LoginButton.IsEnabled = false;

        try
        {
            if (email == AdminEmail && password == AdminPassword)
            {
                AddLog("Admin login successful.");
                _isAdmin = true;
                var usersFile = await LoadUsersFromGitHubAsync();
                if (usersFile != null)
                {
                    _users = usersFile.Users;
                }
                ShowAdminPage();
                RefreshAdminUserList();
                NavAdmin.Visibility = Visibility.Visible;
                AddLog($"Loaded {_users.Count} user(s) from GitHub.");
                return;
            }

            var loaded = await LoadUsersFromGitHubAsync();
            if (loaded == null)
            {
                LoginError.Text = "Could not reach server. Check your internet connection.";
                LoginError.Visibility = Visibility.Visible;
                return;
            }

            var user = loaded.Users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                LoginError.Text = "No account found. Contact admin.";
                LoginError.Visibility = Visibility.Visible;
                AddLog($"Login failed: no account for {email}.");
                return;
            }

            if (user.Password != password)
            {
                LoginError.Text = "Incorrect password. Try again.";
                LoginError.Visibility = Visibility.Visible;
                AddLog($"Login failed: wrong password for {email}.");
                return;
            }

            if (!user.Enabled)
            {
                var reason = string.IsNullOrWhiteSpace(user.DeniedReason)
                    ? "Your account has been disabled."
                    : user.DeniedReason;
                ShowAccessDeniedPage(reason);
                AddLog($"Login denied: {email} is disabled. Reason: {reason}");
                return;
            }

            AddLog($"Login successful: {email}");
            ShowMainApp();
            _ = Task.Run(() => CheckForUpdateAsync());
            await RefreshDevicesAsync();
            await ScanTvsAsync(false);
        }
        catch (Exception ex)
        {
            LoginError.Text = $"Error: {ex.Message}";
            LoginError.Visibility = Visibility.Visible;
            AddLog($"Login error: {ex.Message}");
        }
        finally
        {
            LoginLoading.Visibility = Visibility.Collapsed;
            LoginButton.IsEnabled = true;
        }
    }

    private void BackToLogin_Click(object sender, RoutedEventArgs e)
    {
        ShowLoginPage();
    }

    private void AdminSignOut_Click(object sender, RoutedEventArgs e)
    {
        _users.Clear();
        AdminUserList.ItemsSource = null;
        AdminNewEmail.Text = "";
        AdminNewPassword.Text = "";
        AdminDeniedReason.Text = "";
        AdminStatusText.Text = "";
        _isAdmin = false;
        NavAdmin.Visibility = Visibility.Collapsed;
        ShowLoginPage();
        AddLog("Admin signed out.");
    }

    private async void AdminGoToApp_Click(object sender, RoutedEventArgs e)
    {
        ShowMainApp();
        _ = Task.Run(() => CheckForUpdateAsync());
        await RefreshDevicesAsync();
        await ScanTvsAsync(false);
        AddLog("Switched to main app.");
    }

    private void NavAdmin_Click(object sender, RoutedEventArgs e)
    {
        AdminPage.Visibility = Visibility.Visible;
        ScreenPage.Visibility = Visibility.Collapsed;
        CameraPage.Visibility = Visibility.Collapsed;
        TvPage.Visibility = Visibility.Collapsed;
        SettingsPage.Visibility = Visibility.Collapsed;
        PageTitle.Text = "User Management";
        PageSubtitle.Text = "Manage user access to the application";
        RefreshAdminUserList();
    }

    private void ShowCreateAccount_Click(object sender, RoutedEventArgs e)
    {
        LoginForm.Visibility = Visibility.Collapsed;
        CreateAccountForm.Visibility = Visibility.Visible;
        LoginTitle.Text = "Create Account";
        LoginSubtitle.Text = "Sign up to start using voidscreecopy";
        LoginError.Visibility = Visibility.Collapsed;
        RegisterError.Visibility = Visibility.Collapsed;
        RegisterSuccess.Visibility = Visibility.Collapsed;
    }

    private void ShowLogin_Click(object sender, RoutedEventArgs e)
    {
        CreateAccountForm.Visibility = Visibility.Collapsed;
        LoginForm.Visibility = Visibility.Visible;
        LoginTitle.Text = "Welcome back";
        LoginSubtitle.Text = "Sign in to use voidscreecopy";
        RegisterError.Visibility = Visibility.Collapsed;
        RegisterSuccess.Visibility = Visibility.Collapsed;
        LoginError.Visibility = Visibility.Collapsed;
    }

    private async void CreateAccount_Click(object sender, RoutedEventArgs e)
    {
        var name = RegisterNameBox.Text.Trim();
        var email = RegisterEmail.Text.Trim();
        var password = RegisterPassword.Password.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            RegisterError.Text = "All fields are required.";
            RegisterError.Visibility = Visibility.Visible;
            RegisterSuccess.Visibility = Visibility.Collapsed;
            return;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            RegisterError.Text = "Please enter a valid email address.";
            RegisterError.Visibility = Visibility.Visible;
            RegisterSuccess.Visibility = Visibility.Collapsed;
            return;
        }

        if (password.Length < 4)
        {
            RegisterError.Text = "Password must be at least 4 characters.";
            RegisterError.Visibility = Visibility.Visible;
            RegisterSuccess.Visibility = Visibility.Collapsed;
            return;
        }

        RegisterError.Visibility = Visibility.Collapsed;
        RegisterLoading.Visibility = Visibility.Visible;
        RegisterButton.IsEnabled = false;

        try
        {
            var loaded = await LoadUsersFromGitHubAsync();
            if (loaded == null)
            {
                RegisterError.Text = "Could not reach server. Check your internet connection.";
                RegisterError.Visibility = Visibility.Visible;
                return;
            }

            if (loaded.Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                RegisterError.Text = "An account with this email already exists.";
                RegisterError.Visibility = Visibility.Visible;
                return;
            }

            loaded.Users.Add(new UserEntry
            {
                Email = email,
                Password = password,
                Enabled = true,
                DeniedReason = ""
            });

            var usersJson = JsonSerializer.Serialize(new UsersFile { Users = loaded.Users }, JsonOptions.Default);
            var contentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(usersJson));

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("voidscreecopy-register/1.0");

            var existingSha = "";
            try
            {
                var existing = await http.GetStringAsync(
                    "https://api.github.com/repos/voidhunterx30/voidscreecopy/contents/users.json");
                var existingDoc = JsonDocument.Parse(existing);
                if (existingDoc.RootElement.TryGetProperty("sha", out var shaProp))
                    existingSha = shaProp.GetString() ?? "";
            }
            catch { }

            var body = new
            {
                message = $"New user registration: {email} ({DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC)",
                content = contentBase64,
                sha = existingSha
            };

            var bodyJson = JsonSerializer.Serialize(body, JsonOptions.Default);
            var request = new HttpRequestMessage(HttpMethod.Put,
                "https://api.github.com/repos/voidhunterx30/voidscreecopy/contents/users.json")
            {
                Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
            };

            var response = await http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                RegisterSuccess.Text = "Account created! You can now sign in.";
                RegisterSuccess.Visibility = Visibility.Visible;
                RegisterError.Visibility = Visibility.Collapsed;
                AddLog($"New account created: {email}");
                RegisterNameBox.Text = "";
                RegisterEmail.Text = "";
                RegisterPassword.Password = "";
            }
            else
            {
                RegisterError.Text = $"Error: {response.StatusCode}";
                RegisterError.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            RegisterError.Text = $"Error: {ex.Message}";
            RegisterError.Visibility = Visibility.Visible;
        }
        finally
        {
            RegisterLoading.Visibility = Visibility.Collapsed;
            RegisterButton.IsEnabled = true;
        }
    }

    private async void AdminSave_Click(object sender, RoutedEventArgs e)
    {
        var token = AdminGithubToken.Text.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            AdminStatusText.Text = "Enter a GitHub PAT first.";
            AdminStatusText.Foreground = FindResource("AccentRed") as Brush;
            return;
        }

        AdminSaveBtn.IsEnabled = false;
        AdminStatusText.Text = "Saving to GitHub...";
        AdminStatusText.Foreground = FindResource("AccentBlue") as Brush;

        try
        {
            var usersFile = new UsersFile { Users = _users };
            var contentJson = JsonSerializer.Serialize(usersFile, JsonOptions.Default);
            var contentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(contentJson));

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("voidscreecopy-admin/1.0");
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var existingSha = "";
            try
            {
                var existing = await http.GetStringAsync(
                    "https://api.github.com/repos/voidhunterx30/voidscreecopy/contents/users.json");
                var existingDoc = JsonDocument.Parse(existing);
                if (existingDoc.RootElement.TryGetProperty("sha", out var shaProp))
                    existingSha = shaProp.GetString() ?? "";
            }
            catch { }

            var body = new
            {
                message = $"Update users.json via admin panel ({DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC)",
                content = contentBase64,
                sha = existingSha
            };

            var bodyJson = JsonSerializer.Serialize(body, JsonOptions.Default);
            var request = new HttpRequestMessage(HttpMethod.Put,
                "https://api.github.com/repos/voidhunterx30/voidscreecopy/contents/users.json")
            {
                Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
            };

            var response = await http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                AdminStatusText.Text = "Saved successfully!";
                AdminStatusText.Foreground = FindResource("AccentGreen") as Brush;
                AddLog("Users saved to GitHub successfully.");
            }
            else
            {
                var errBody = await response.Content.ReadAsStringAsync();
                AdminStatusText.Text = $"Error {response.StatusCode}: {errBody[..Math.Min(120, errBody.Length)]}";
                AdminStatusText.Foreground = FindResource("AccentRed") as Brush;
                AddLog($"GitHub save failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            AdminStatusText.Text = $"Error: {ex.Message}";
            AdminStatusText.Foreground = FindResource("AccentRed") as Brush;
            AddLog($"GitHub save error: {ex.Message}");
        }
        finally
        {
            AdminSaveBtn.IsEnabled = true;
        }
    }

    private void AdminAddUser_Click(object sender, RoutedEventArgs e)
    {
        var email = AdminNewEmail.Text.Trim();
        var password = AdminNewPassword.Text.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            AdminStatusText.Text = "Email and password are required.";
            AdminStatusText.Foreground = FindResource("AccentRed") as Brush;
            return;
        }

        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            AdminStatusText.Text = "A user with this email already exists.";
            AdminStatusText.Foreground = FindResource("AccentRed") as Brush;
            return;
        }

        _users.Add(new UserEntry
        {
            Email = email,
            Password = password,
            Enabled = _adminNewUserEnabled,
            DeniedReason = AdminDeniedReason.Text.Trim()
        });

        RefreshAdminUserList();
        AdminNewEmail.Text = "";
        AdminNewPassword.Text = "";
        AdminDeniedReason.Text = "";
        _adminNewUserEnabled = true;
        AdminStatusToggleText.Text = "Enabled";
        AdminStatusDot.Background = FindResource("AccentGreen") as Brush;
        AdminStatusText.Text = $"User '{email}' added. Click Save to GitHub.";
        AdminStatusText.Foreground = FindResource("AccentGreen") as Brush;
        AddLog($"Added user: {email}");
    }

    private void AdminToggleStatus_Click(object sender, RoutedEventArgs e)
    {
        _adminNewUserEnabled = !_adminNewUserEnabled;
        AdminStatusToggleText.Text = _adminNewUserEnabled ? "Enabled" : "Disabled";
        AdminStatusDot.Background = _adminNewUserEnabled
            ? FindResource("AccentGreen") as Brush
            : FindResource("AccentRed") as Brush;
    }

    private void AdminUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AdminUserList.SelectedItem is UserDisplayItem selected)
        {
            var user = _users.FirstOrDefault(u => u.Email == selected.Email);
            if (user != null)
            {
                AdminNewEmail.Text = user.Email;
                AdminNewPassword.Text = user.Password;
                AdminDeniedReason.Text = user.DeniedReason;
                _adminNewUserEnabled = user.Enabled;
                AdminStatusToggleText.Text = user.Enabled ? "Enabled" : "Disabled";
                AdminStatusDot.Background = user.Enabled
                    ? FindResource("AccentGreen") as Brush
                    : FindResource("AccentRed") as Brush;
                AdminStatusText.Text = "";
            }
        }
    }

    private void DeleteUser_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock tb && tb.DataContext is UserDisplayItem item)
        {
            var user = _users.FirstOrDefault(u => u.Email == item.Email);
            if (user != null)
            {
                _users.Remove(user);
                RefreshAdminUserList();
                AdminStatusText.Text = $"User '{item.Email}' removed. Click Save to GitHub.";
                AdminStatusText.Foreground = FindResource("AccentGreen") as Brush;
                AddLog($"Removed user: {item.Email}");
            }
        }
    }

    private void ShowLoginPage()
    {
        LoginPage.Visibility = Visibility.Visible;
        AccessDeniedPage.Visibility = Visibility.Collapsed;
        AdminPage.Visibility = Visibility.Collapsed;
        LoginError.Visibility = Visibility.Collapsed;
    }

    private void ShowAccessDeniedPage(string reason)
    {
        LoginPage.Visibility = Visibility.Collapsed;
        AccessDeniedPage.Visibility = Visibility.Visible;
        AdminPage.Visibility = Visibility.Collapsed;
        DeniedReasonText.Text = reason;
    }

    private void ShowAdminPage()
    {
        LoginPage.Visibility = Visibility.Collapsed;
        AccessDeniedPage.Visibility = Visibility.Collapsed;
        AdminPage.Visibility = Visibility.Visible;
    }

    private void ShowMainApp()
    {
        LoginPage.Visibility = Visibility.Collapsed;
        AccessDeniedPage.Visibility = Visibility.Collapsed;
        AdminPage.Visibility = Visibility.Collapsed;
    }

    private void RefreshAdminUserList()
    {
        var items = _users.Select(u => new UserDisplayItem
        {
            Email = u.Email,
            StatusText = u.Enabled ? "Active" : "Disabled",
            StatusColor = u.Enabled ? Brushes.Green : Brushes.Red
        }).ToList();
        AdminUserList.ItemsSource = items;
    }
}

public sealed record AndroidDevice(string Serial, string Status, string Model, string Product)
{
    public string DisplayName => $"{Model}  |  {Status}  |  {Serial}";
}

public sealed record TvTarget(string Name, string Address, string Source)
{
    public string DisplayName => $"{Name}  |  {Address}  |  {Source}";
}

public sealed record ScreenPreset(string Name, int MaxSize, int Fps, string BitRate)
{
    public override string ToString() => Name;
    public static readonly List<ScreenPreset> All =
    [
        new("Low lag - 960 / 30fps / 4M", 960, 30, "4M"),
        new("Balanced - 1280 / 30fps / 6M", 1280, 30, "6M"),
        new("High quality - 1280 / 60fps / 8M", 1280, 60, "8M")
    ];
}

public sealed record CameraPreset(string Name, string Size, int Fps, string BitRate)
{
    public override string ToString() => $"{Name} - {Size}";
    public static readonly List<CameraPreset> All =
    [
        new("1080p", "1920x1080", 30, "12M"),
        new("2K", "2560x1440", 30, "20M"),
        new("4K", "3840x2160", 30, "40M")
    ];
}

public sealed class UpdateSettings
{
    public bool AutoUpdate { get; set; } = true;
    public string ManifestUrl { get; set; } = "https://raw.githubusercontent.com/voidhunterx30/voidscreecopy/main/update-manifest.json";
    public string? SkippedVersion { get; set; }
    public DateTime? RemindLaterUntil { get; set; }

    public static async Task<UpdateSettings> LoadAsync(string path)
    {
        if (!File.Exists(path))
        {
            var settings = new UpdateSettings();
            await settings.SaveAsync(path);
            return settings;
        }

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<UpdateSettings>(json, JsonOptions.Default) ?? new UpdateSettings();
    }

    public async Task SaveAsync(string path)
    {
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(this, JsonOptions.Default));
    }
}

public sealed class UpdateManifest
{
    public string Version { get; set; } = "";
    public string PackageUrl { get; set; } = "";
    public string Notes { get; set; } = "";
    public string? Sha256 { get; set; }
}

public class UserEntry
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public string DeniedReason { get; set; } = "";
}

public class UsersFile
{
    public List<UserEntry> Users { get; set; } = new();
}

public class UserDisplayItem
{
    public string Email { get; set; } = "";
    public string StatusText { get; set; } = "";
    public Brush StatusColor { get; set; } = Brushes.Green;
}

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
