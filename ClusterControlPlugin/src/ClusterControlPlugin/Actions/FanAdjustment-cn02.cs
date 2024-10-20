namespace Loupedeck.ClusterControlPlugin
{
    using System;
    using System.Diagnostics;
    using System.Timers;

    // This class implements an example adjustment that counts the rotation ticks of a dial.

    public class Fan2Adjustment : PluginDynamicAdjustment
    {
        // This variable holds the current value of the counter.
        private Int32 _counter = 0;
        private Timer _timer;
        private String _sshCommand = "-J nthuscc@192.168.176.33 s6u-cn02";

        // Initializes the adjustment class.
        // When `hasReset` is set to true, a reset command is automatically created for this adjustment.
        public Fan2Adjustment()
            : base(displayName: "Fan2 Speed", description: "Adjust fan2 speed percent", groupName: "Adjustments", hasReset: true)
        {
            // create adjustment confirm timer
            this._timer = new Timer(800);
            this._timer.Elapsed += new ElapsedEventHandler(this.CheckTimer);
            this._timer.AutoReset = false;

            // this.AddParameter("Enter ssh command", "ssh command", "Adjustments");
            // this.MakeProfileAction("text;Enter ssh command:");
        }

        // This method is called when the adjustment is executed.
        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            this._counter += diff; // Increase or decrease the counter by the number of ticks.

            if (this._counter < 0)
            {
                this._counter = 0;
            }
            else if (this._counter > 100)
            {
                this._counter = 100;
            }

            this.AdjustmentValueChanged(); // Notify the plugin service that the adjustment value has changed.
            // this._sshCommand = this._sshCommand == null ? actionParameter : this._sshCommand;

            PluginLog.Info($"counter: {this._counter}. Command: {actionParameter}");

            // reset timer
            this._timer.Stop();
            this._timer.Start();
        }

        // This method is called to check the timer and adjust the fan speed
        protected void CheckTimer(Object source, ElapsedEventArgs e)
        {
            // config fan speed
            this.AdjustFanOnSSH(this._sshCommand, this._counter);
        }

        // This method is called when the reset command related to the adjustment is executed.
        protected override void RunCommand(String actionParameter)
        {
            this._counter = -1; // Reset the counter.
            this.AdjustmentValueChanged(); // Notify the plugin service that the adjustment value has changed.

            this._timer.Stop(); // stop timer from triggering
            this.AdjustFanOnSSH(this._sshCommand, this._counter);

            PluginLog.Info($"command: {actionParameter}");
        }

        // Returns the adjustment value that is shown next to the dial.
        protected override String GetAdjustmentValue(String actionParameter) => 
            this._counter < 0 ? "auto" : this._counter.ToString();

        protected void AdjustFanOnSSH(String sshCommand, Int32 percent)
        {
            // Create a new process to run the terminal (bash on Linux/macOS, cmd on Windows)
            Process process = new Process();

            // Configure the process to start the terminal
            process.StartInfo.FileName = GetTerminalPath(); // Determines the terminal based on OS
            PluginLog.Info($"terminal path: {process.StartInfo.FileName}");
            process.StartInfo.Arguments = $"ssh -t {sshCommand}"; // Command to run (e.g., "dir" in cmd)
            
            // config fan speed
            if (percent >= 0)
            {
                process.StartInfo.Arguments += $" 'sudo /home/share/fan_ctrl.sh {percent}'";
            }
            else
            {
                process.StartInfo.Arguments += $" 'sudo /home/share/fan_ctrl.sh'";    // auto
            }

            // Ensure we can interact with the terminal
            process.StartInfo.UseShellExecute = true;
            // process.StartInfo.RedirectStandardOutput = true; // Capture terminal output
            // process.StartInfo.RedirectStandardInput = true; // Allow sending input
            // process.StartInfo.RedirectStandardError = true; // Capture errors
            process.StartInfo.CreateNoWindow = false; // Don't open a separate window

            // Start the process
            process.Start();
            PluginLog.Info($"process started");

            // Ensure the process exits
            process.WaitForExit();
        }

        // Helper method to get the terminal path based on the operating system
        static string GetTerminalPath()
        {
            if (OperatingSystem.IsWindows())
            {
                return "powershell.exe"; // Command prompt on Windows
            }
            else if (OperatingSystem.IsLinux())
            {
                return "/bin/bash"; // Bash terminal on Linux
            }
            else if (OperatingSystem.IsMacOS())
            {
                return "/bin/bash"; // Bash terminal on macOS
            }
            throw new PlatformNotSupportedException("Unsupported OS");
        }
    }
}
