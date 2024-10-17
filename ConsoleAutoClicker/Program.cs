using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Diagnostics;
using System.Threading;

static class Program
{
    public static int activeKey = VK_F;
    public static double targetCPS = 16.66;
    public static bool minecraftOnly = true;
    public static bool exitRequested = false;

    public const int VK_F = 0x46;
    public const int VK_TAB = 0x09;

    private static MainForm mainForm;
    private static Thread clickThread;
    private static Random random = new Random();
    private static Stopwatch stopwatch = new Stopwatch();
    private static long lastClickTicks = 0;

    [STAThread]
    static void Main()
    {
        try
        {
            
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            // to set timer resolution, but don't fail if it's not available
            try
            {
                TimeBeginPeriod(1);
            }
            catch (EntryPointNotFoundException)
            {
                // timer functions not available ffs
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadConfig();
            InitializeClickThread();
            mainForm = new MainForm();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            // Log the exception to a file
            File.WriteAllText("error_log.txt", ex.ToString());
            MessageBox.Show($"An error occurred: {ex.Message}\nCheck error_log.txt for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Try to reset timer resolution, but don't fail if it's not available
            try
            {
                TimeEndPeriod(1);
            }
            catch (EntryPointNotFoundException)
            {
                // timer functions not available, continue without them
            }
        }
    }

    private static void InitializeClickThread()
    {
        clickThread = new Thread(ClickLoop);
        clickThread.IsBackground = true;
        clickThread.Priority = ThreadPriority.Highest;
        clickThread.Start();
    }

    private static void ClickLoop()
    {
        stopwatch.Start();
        while (!exitRequested)
        {
            if (!minecraftOnly || IsMinecraftActive())
            {
                PerformClick();
            }
            Thread.Yield();
        }
    }

    private static void PerformClick()
    {
        if (GetAsyncKeyState(activeKey) < 0)
        {
            long currentTicks = stopwatch.ElapsedTicks;
            double elapsedSeconds = (currentTicks - lastClickTicks) / (double)Stopwatch.Frequency;
            double currentCPS = 1.0 / elapsedSeconds;

            if (currentCPS <= targetCPS)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                lastClickTicks = currentTicks;
            }
        }
    }

    private static bool IsMinecraftActive()
    {
        IntPtr foregroundWindow = GetForegroundWindow();
        StringBuilder windowTitle = new StringBuilder(256);
        GetWindowText(foregroundWindow, windowTitle, 256);
        string title = windowTitle.ToString();

        // Check for specific Minecraft window titles
        return title.Equals("Minecraft", StringComparison.OrdinalIgnoreCase) ||
               title.StartsWith("Minecraft*", StringComparison.OrdinalIgnoreCase) ||
               title.EndsWith("Minecraft", StringComparison.OrdinalIgnoreCase);
    }

    public static void ResetToDefault()
    {
        activeKey = VK_F;
        targetCPS = 16.00;
        minecraftOnly = true;
        SaveConfig();
    }

    public static void SwitchActiveKey()
    {
        activeKey = (activeKey == VK_F) ? VK_TAB : VK_F;
        SaveConfig();
    }

    public static void UpdateClickInterval()
    {
        SaveConfig();
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

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
    public static extern uint TimeBeginPeriod(uint uPeriod);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
    public static extern uint TimeEndPeriod(uint uPeriod);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
}

public class Config
{
    public int ActiveKey { get; set; }
    public double TargetCPS { get; set; }
    public bool MinecraftOnly { get; set; }
}
