using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Serialization;

static class Program
{
    public static int activeKey = VK_F;
    public static double targetCPS = 16.66;
    public static bool minecraftOnly = true;
    public static bool exitRequested = false;

    public const int VK_F = 0x46;
    public const int VK_TAB = 0x09;

    private static MainForm mainForm;
    private static Timer clickTimer;
    private static Random random = new Random();

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        LoadConfig();
        InitializeClickTimer();
        mainForm = new MainForm();
        Application.Run(mainForm);
    }

    private static void InitializeClickTimer()
    {
        clickTimer = new Timer();
        clickTimer.Tick += ClickTimer_Tick;
        UpdateClickInterval();
        clickTimer.Start();
    }

    private static void ClickTimer_Tick(object sender, EventArgs e)
    {
        if (!exitRequested && (!minecraftOnly || IsMinecraftActive()))
        {
            PerformClick();
        }
    }

    private static void PerformClick()
    {
        if (GetAsyncKeyState(activeKey) < 0)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }

    private static bool IsMinecraftActive()
    {
        IntPtr foregroundWindow = GetForegroundWindow();
        int processId;
        GetWindowThreadProcessId(foregroundWindow, out processId);
        System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(processId);
        return process.ProcessName.ToLower().Contains("minecraft");
    }

    public static void ResetToDefault()
    {
        activeKey = VK_F;
        targetCPS = 16.66;
        minecraftOnly = true;
        UpdateClickInterval();
        SaveConfig();
    }

    public static void SwitchActiveKey()
    {
        activeKey = (activeKey == VK_F) ? VK_TAB : VK_F;
        SaveConfig();
    }

    public static void UpdateClickInterval()
    {
        double interval = 1000.0 / targetCPS;
        clickTimer.Interval = (int)interval;
    }

    public static void SaveConfig()
    {
        Config config = new Config
        {
            ActiveKey = activeKey,
            TargetCPS = targetCPS,
            MinecraftOnly = minecraftOnly
        };

        XmlSerializer serializer = new XmlSerializer(typeof(Config));
        using (StreamWriter writer = new StreamWriter("config.xml"))
        {
            serializer.Serialize(writer, config);
        }
    }

    private static void LoadConfig()
    {
        if (File.Exists("config.xml"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (StreamReader reader = new StreamReader("config.xml"))
            {
                Config config = (Config)serializer.Deserialize(reader);
                activeKey = config.ActiveKey;
                targetCPS = config.TargetCPS;
                minecraftOnly = config.MinecraftOnly;
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
}

public class Config
{
    public int ActiveKey { get; set; }
    public double TargetCPS { get; set; }
    public bool MinecraftOnly { get; set; }
}