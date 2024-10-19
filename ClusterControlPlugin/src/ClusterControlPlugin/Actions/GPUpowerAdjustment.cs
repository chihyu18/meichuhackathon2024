namespace Loupedeck.ClusterControlPlugin
{
    using System;
    using System.Diagnostics;

    // This class implements an example adjustment that counts the rotation ticks of a dial.

    public class GPUpowerAdjustment : PluginDynamicAdjustment
    {
        // This variable holds the current value of the counter.
        private Int32 _counter = 0;
        private String sshCommand = "ls";

        // Initializes the adjustment class.
        // When `hasReset` is set to true, a reset command is automatically created for this adjustment.
        public GPUpowerAdjustment()
            : base(displayName: "GPUpower", description: "GPUpower adjust", groupName: "Adjustments", hasReset: true)
        {
            // this.MakeProfileAction("text;Enter shell command:");
        }

        // This method is called when the adjustment is executed.
        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            PluginLog.Info($"ApplyAdjustment called with diff: {diff}");
            this._counter += diff; // Increase or decrease the counter by the number of ticks.
            this.AdjustmentValueChanged(); // Notify the plugin service that the adjustment value has changed.
            this.sshCommand = $"nvidia-smi -pl {this._counter}";
            OpenTerminal(this.sshCommand);
            PluginLog.Info($"Counter value is {this._counter}"); // Write the current counter value to the log file.
            this._counter = 0;
        }

        // This method is called when the reset command related to the adjustment is executed.
        protected override void RunCommand(String actionParameter)
        {
            this.sshCommand = $"nvidia-smi -pl 350";
            OpenTerminal(this.sshCommand);
            PluginLog.Info($"Counter value is 350"); // Write the current counter value to the log file.
            this._counter = 0; // Reset the counter.
            this.AdjustmentValueChanged(); // Notify the plugin service that the adjustment value has changed.
        }

        // Returns the adjustment value that is shown next to the dial.
        protected override String GetAdjustmentValue(String actionParameter) => this._counter.ToString();

        static void OpenTerminal(String sshCommand)
        {
            // Create a new process to run the terminal (bash on Linux/macOS, cmd on Windows)
            Process process = new Process();

            // Configure the process to start the terminal
            process.StartInfo.FileName = GetTerminalPath(); // Determines the terminal based on OS
            PluginLog.Info($"terminal path: {process.StartInfo.FileName}");
            process.StartInfo.Arguments = $"{sshCommand}"; // Command to run (e.g., "dir" in cmd)
            // ssh {server}

            // Ensure we can interact with the terminal
            process.StartInfo.UseShellExecute = true;
            // process.StartInfo.RedirectStandardOutput = true; // Capture terminal output
            // process.StartInfo.RedirectStandardInput = true; // Allow sending input
            // process.StartInfo.RedirectStandardError = true; // Capture errors
            process.StartInfo.CreateNoWindow = false; // Don't open a separate window

            // Start the process
            process.Start();
            PluginLog.Info($"process started");

            // // Read the terminal output (example: output of "dir" on Windows)
            // string output = process.StandardOutput.ReadToEnd();
            // // Console.WriteLine(output);
            // PluginLog.Info($"terminal output: {output}");

            // // You can also send further commands to the terminal:
            // StreamWriter inputWriter = process.StandardInput;
            // inputWriter.WriteLine("echo Hello from C#"); // Send command
            // inputWriter.Flush();

            // // Capture the new output after the command
            // string newOutput = process.StandardOutput.ReadToEnd();
            // Console.WriteLine(newOutput);

            // while (true)
            // {
            //     Console.Write("Command: ");
            //     string command = Console.ReadLine();
            //     if (string.IsNullOrEmpty(command))
            //         break;

            //     // Send command to the terminal
            //     inputWriter.WriteLine(command);
            //     inputWriter.Flush();

            //     // Read and print the output
            //     string output = outputReader.ReadToEnd();
            //     string error = errorReader.ReadToEnd();

            //     if (!string.IsNullOrEmpty(output))
            //         Console.WriteLine(output);

            //     if (!string.IsNullOrEmpty(error))
            //         Console.WriteLine("Error: " + error);
            // }();

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
