namespace Loupedeck.ClusterControlPlugin
{
    using System;
    using System.Diagnostics;
    using System.Timers;
    using System.Text.RegularExpressions;

    // This class implements an example adjustment that counts the rotation ticks of a dial.

    public class GPUpowerAdjustment : PluginDynamicAdjustment
    {
        // This variable holds the current value of the counter.
        private Int32 _counter = 0;
        private Int32 _max_power = 350;
        private Timer _timer;
        private String _sshCommand = "nthuscc@192.168.176.33";

        // Initializes the adjustment class.
        // When `hasReset` is set to true, a reset command is automatically created for this adjustment.
        public GPUpowerAdjustment()
            : base(displayName: "GPU power", description: "GPU power adjustment", groupName: "Adjustments", hasReset: true)
        {
            this._timer = new Timer(800);
            this._timer.Elapsed += new ElapsedEventHandler(this.CheckTimer);
            this._timer.AutoReset = false;
            // this.MakeProfileAction("text;Enter shell command:");

            this._max_power = this.GetMaxPower();
        }

        private Int32 GetMaxPower()
        {
            String pattern = @"Max Power Limit\s*:\s*(\d+)\.\d{2}\s*W";
            String output = "";
            Process process = new Process();

            // Configure the process to start the terminal
            process.StartInfo.FileName = GetTerminalPath(); // Determines the terminal based on OS
            PluginLog.Info($"terminal path: {process.StartInfo.FileName}");
            process.StartInfo.Arguments = $"ssh -t {this._sshCommand} \"nvidia-smi -q -d POWER | grep 'Max Power Limit'\""; // Command to run (e.g., "dir" in cmd)

            // Ensure we can interact with the terminal
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true; // Capture terminal output
            process.StartInfo.RedirectStandardInput = true; // Allow sending input
            process.StartInfo.RedirectStandardError = true; // Capture errors
            process.StartInfo.CreateNoWindow = true; // Don't open a separate window

            // Start the process
            process.Start();
            PluginLog.Info($"process started");
            // while (output == "")
            // {
                output = process.StandardOutput.ReadToEnd();
            // }

            // Ensure the process exits
            process.WaitForExit();

            // Use Regex to match the pattern and extract the numeric value
            Match match = Regex.Match(output, pattern);
            if (match.Success)
            {
                // Extract the integer part of the match
                String maxPowerLimit = match.Groups[1].Value;

                PluginLog.Info($"max power limit: {maxPowerLimit}");
                return Int32.Parse(maxPowerLimit);
            }
            else
            {
                return Int32.Parse("350");
            }
        }

        // This method is called when the adjustment is executed.
        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            PluginLog.Info($"Apply Adjustment called with diff: {diff}");
            this._counter += diff; // Increase or decrease the counter by the number of ticks.

            if (this._counter < 1)
            {
                this._counter = 1;
            }
            else if (this._counter > this._max_power)
            {
                this._counter = this._max_power;
            }

            this.AdjustmentValueChanged(); // Notify the plugin service that the adjustment value has changed.
            
            PluginLog.Info($"Counter value is {this._counter}"); // Write the current counter value to the log file.
            
            // reset timer
            this._timer.Stop();
            this._timer.Start();
        }

        protected void CheckTimer(Object source, ElapsedEventArgs e)
        {
            // config fan speed
            this.AdjustGPUPower(this._sshCommand);
        }

        // This method is called when the reset command related to the adjustment is executed.
        protected override void RunCommand(String actionParameter)
        {
            PluginLog.Info($"Counter value is {this._max_power}"); // Write the current counter value to the log file.
            this._counter = 0; // Reset the counter.

            this._timer.Stop();
            this.AdjustmentValueChanged(); // Notify the plugin service that the adjustment value has changed.
            this.AdjustGPUPower(this._sshCommand);
        }

        // Returns the adjustment value that is shown next to the dial.
        protected override String GetAdjustmentValue(String actionParameter) => this._counter.ToString();

        private void AdjustGPUPower(String sshCommand)
        {
            // check whether max power is set
            if (this._max_power < 0)
            {
                return;     // no gpu found
            }

            // Create a new process to run the terminal (bash on Linux/macOS, cmd on Windows)
            Process process = new Process();

            // Configure the process to start the terminal
            process.StartInfo.FileName = GetTerminalPath(); // Determines the terminal based on OS
            PluginLog.Info($"terminal path: {process.StartInfo.FileName}");
            process.StartInfo.Arguments = $"ssh -t {sshCommand}"; // Command to run (e.g., "dir" in cmd)
            // ssh {server}

            // config fan speed
            if (this._counter >= 1)
            {
                process.StartInfo.Arguments += $" 'sudo nvidia-smi -pl {this._counter}'";
            }
            else
            {
                process.StartInfo.Arguments += $" 'sudo nvidia-smi -pl {this._max_power}'";    // auto
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
