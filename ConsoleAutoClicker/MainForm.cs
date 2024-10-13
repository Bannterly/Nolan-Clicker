using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class MainForm : Form
{
    private Label activeKeyLabel;
    private Label cpsLabel;
    private NumericUpDown cpsNumericUpDown;
    private Button resetButton;
    private Button switchKeyButton;
    private CustomCheckBox minecraftOnlyCheckBox;

    private Color backgroundColor = ColorTranslator.FromHtml("#191414");
    private Color accentColor = ColorTranslator.FromHtml("#1db954");

    private Panel titleBar;
    private Label titleLabel;
    private Button closeButton;
    private Button minimizeButton;

    public MainForm()
    {
        this.Text = "Minecraft Autoclicker";
        this.Size = new Size(400, 230);
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = backgroundColor;
        this.ForeColor = Color.White;

        SetupTitleBar();
        SetupControls();

        this.FormClosing += MainForm_FormClosing;
    }

    private void SetupTitleBar()
    {
        titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = backgroundColor
        };
        this.Controls.Add(titleBar);

        titleLabel = new Label
        {
            Text = "Minecraft Autoclicker",
            ForeColor = Color.White,
            Location = new Point(10, 5),
            AutoSize = true
        };
        titleBar.Controls.Add(titleLabel);

        minimizeButton = new Button
        {
            Text = "_",
            Size = new Size(30, 30),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = backgroundColor,
            Dock = DockStyle.Right
        };
        minimizeButton.FlatAppearance.BorderSize = 0;
        minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
        titleBar.Controls.Add(minimizeButton);

        closeButton = new Button
        {
            Text = "X",
            Size = new Size(30, 30),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = backgroundColor,
            Dock = DockStyle.Right
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => this.Close();
        titleBar.Controls.Add(closeButton);

        titleBar.MouseDown += TitleBar_MouseDown;
        titleLabel.MouseDown += TitleBar_MouseDown;
    }

    private void SetupControls()
    {
        int yOffset = 30;

        activeKeyLabel = new Label
        {
            Location = new Point(10, 10 + yOffset),
            Size = new Size(200, 20),
            Text = $"Active Key: {(Program.activeKey == Program.VK_F ? "F" : "TAB")}",
            ForeColor = Color.White
        };
        this.Controls.Add(activeKeyLabel);

        cpsLabel = new Label
        {
            Location = new Point(10, 40 + yOffset),
            Size = new Size(50, 20),
            Text = "CPS:",
            ForeColor = Color.White
        };
        this.Controls.Add(cpsLabel);

        cpsNumericUpDown = new NumericUpDown
        {
            Location = new Point(70, 40 + yOffset),
            Size = new Size(70, 20),
            Minimum = 1,
            Maximum = 25,
            DecimalPlaces = 2,
            Increment = 0.01M,
            Value = (decimal)Math.Round(Program.targetCPS, 2),
            BackColor = backgroundColor,
            ForeColor = Color.White
        };
        cpsNumericUpDown.ValueChanged += CpsNumericUpDown_ValueChanged;
        this.Controls.Add(cpsNumericUpDown);

        resetButton = new Button
        {
            Location = new Point(10, 70 + yOffset),
            Size = new Size(100, 30),
            Text = "Reset to Default",
            BackColor = accentColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        resetButton.FlatAppearance.BorderSize = 0;
        resetButton.Click += ResetButton_Click;
        this.Controls.Add(resetButton);

        switchKeyButton = new Button
        {
            Location = new Point(120, 70 + yOffset),
            Size = new Size(100, 30),
            Text = "Switch Key",
            BackColor = accentColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        switchKeyButton.FlatAppearance.BorderSize = 0;
        switchKeyButton.Click += SwitchKeyButton_Click;
        this.Controls.Add(switchKeyButton);

        minecraftOnlyCheckBox = new CustomCheckBox
        {
            Location = new Point(230, 75 + yOffset),
            Size = new Size(150, 20),
            Text = "Minecraft Only",
            Checked = Program.minecraftOnly,
            ForeColor = Color.White,
            BackColor = backgroundColor
        };
        minecraftOnlyCheckBox.CheckedChanged += MinecraftOnlyCheckBox_CheckedChanged;
        this.Controls.Add(minecraftOnlyCheckBox);
    }

    private void TitleBar_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }

    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION = 0x2;

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    private void CpsNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        Program.targetCPS = Math.Round((double)cpsNumericUpDown.Value, 2);
        Program.UpdateClickInterval();
        Program.SaveConfig();
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
        Program.ResetToDefault();
        UpdateCPSNumericUpDown();
        UpdateActiveKeyLabel();
        UpdateMinecraftOnlyCheckBox();

        // Visual indicator for reset
        resetButton.Text = "Reset!";
        resetButton.BackColor = Color.Red;

        // Use a timer to revert the button after 1 second
        Timer resetTimer = new Timer();
        resetTimer.Interval = 1000;
        resetTimer.Tick += (s, args) =>
        {
            resetButton.Text = "Reset to Default";
            resetButton.BackColor = accentColor;
            resetTimer.Stop();
            resetTimer.Dispose();
        };
        resetTimer.Start();
    }

    private void SwitchKeyButton_Click(object sender, EventArgs e)
    {
        Program.SwitchActiveKey();
        UpdateActiveKeyLabel();
        Program.SaveConfig();
    }

    private void MinecraftOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        Program.minecraftOnly = minecraftOnlyCheckBox.Checked;
        Program.SaveConfig();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Program.exitRequested = true;
    }

    public void UpdateActiveKeyLabel()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(UpdateActiveKeyLabel));
        }
        else
        {
            activeKeyLabel.Text = $"Active Key: {(Program.activeKey == Program.VK_F ? "F" : "TAB")}";
        }
    }

    public void UpdateCPSNumericUpDown()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(UpdateCPSNumericUpDown));
        }
        else
        {
            cpsNumericUpDown.Value = (decimal)Math.Round(Program.targetCPS, 2);
        }
    }

    public void UpdateMinecraftOnlyCheckBox()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(UpdateMinecraftOnlyCheckBox));
        }
        else
        {
            minecraftOnlyCheckBox.Checked = Program.minecraftOnly;
        }
    }

    public class CustomCheckBox : Control
    {
        public bool Checked { get; set; }
        public event EventHandler CheckedChanged;

        private Color accentColor = ColorTranslator.FromHtml("#1db954");

        public CustomCheckBox()
        {
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            Rectangle boxRect = new Rectangle(0, 2, 16, 16);
            g.DrawRectangle(new Pen(Color.White), boxRect);

            if (Checked)
            {
                g.FillRectangle(new SolidBrush(accentColor), 2, 4, 12, 12);
            }

            using (StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center })
            {
                g.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), new Point(20, 10), sf);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
            Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}