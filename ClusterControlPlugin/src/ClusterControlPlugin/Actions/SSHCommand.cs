/*
open a terminal; return
ssh to the given ip
*/

namespace Loupedeck.ClusterControlPlugin
{
    using System;
    using System.Diagnostics;

    // This class implements an example command that counts button presses.

    public class SSHCommand : PluginDynamicCommand
    {
        private Int32 _counter = 0;
        private String sshCommand = "ls";

        // Initializes the command class.
        public SSHCommand()
            : base(
                displayName: "ssh",
                description: "SSH button presses",
                groupName: "Commands"
            ) { 
                this.MakeProfileAction("text;Enter ssh command:");
            }

        // This method is called when the user executes the command.
        protected override void RunCommand(String actionParameter)
        {
            this.sshCommand = actionParameter;
            OpenTerminal(this.sshCommand);
            this.ActionImageChanged(); // Notify the plugin service that the command display name and/or image has changed.
            PluginLog.Info($"Counter value is {this._counter}"); // Write the current counter value to the log file.
        }

        // This method is called when Loupedeck needs to show the command on the console or the UI.
        protected override String GetCommandDisplayName(
            String actionParameter,
            PluginImageSize imageSize
        ) => $"Press Counter{Environment.NewLine}{this._counter}";

        static void OpenTerminal(String sshCommand)
        {
            // Create a new process to run the terminal (bash on Linux/macOS, cmd on Windows)
            Process process = new Process();

            // Configure the process to start the terminal
            process.StartInfo.FileName = GetTerminalPath(); // Determines the terminal based on OS
            PluginLog.Info($"terminal path: {process.StartInfo.FileName}");
            process.StartInfo.Arguments = $"{sshCommand}"; // Command to run (e.g., "dir" in cmd)

            // Ensure we can interact with the terminal
            process.StartInfo.UseShellExecute = true;
            // process.StartInfo.RedirectStandardOutput = true; // Capture terminal output
            // process.StartInfo.RedirectStandardInput = true; // Allow sending input
            // process.StartInfo.RedirectStandardError = true; // Capture errors
            process.StartInfo.CreateNoWindow = false; // Don't open a separate window

            // Start the process
            process.Start();
            PluginLog.Info($"process started");

            /* if redirect?
            // Read the terminal output (example: output of "dir" on Windows)
            string output = process.StandardOutput.ReadToEnd();
            // Console.WriteLine(output);
            PluginLog.Info($"terminal output: {output}");

            // You can also send further commands to the terminal:
            StreamWriter inputWriter = process.StandardInput;
            inputWriter.WriteLine("echo Hello from C#"); // Send command
            inputWriter.Flush();

            // Capture the new output after the command
            string newOutput = process.StandardOutput.ReadToEnd();
            Console.WriteLine(newOutput);
            */
            
            // Interactive loop to send commands and get output
            StreamWriter inputWriter = process.StandardInput;
            StreamReader outputReader = process.StandardOutput;
            StreamReader errorReader = process.StandardError;
            

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
            // }
            

            // Ensure the process exits
            process.WaitForExit();

            inputWriter.Close();
            outputReader.Close();
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
