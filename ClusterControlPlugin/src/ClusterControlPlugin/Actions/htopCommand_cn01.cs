/*
open a terminal; return
ssh to the given ip
*/

namespace Loupedeck.ClusterControlPlugin
{
    using System;
    using System.Diagnostics;

    // This class implements an example command that counts button presses.

    public class HtopCommand : PluginDynamicCommand
    {
        private String sshCommand = "pwd";
        private String userName = "";
        private String serverIp = "";

        // Initializes the command class.
        public HtopCommand()
            : base(
                displayName: "htop",
                description: "htop",
                groupName: "Commands"
            ) { 
                this.MakeProfileAction("text;Enter username & server ip:");
            }

        // This method is called when the user executes the command.
        protected override void RunCommand(String actionParameter)
        {
            // username, server ip
            String[] parsedArg = actionParameter.Split(' ');
            this.userName = parsedArg[0];
            this.serverIp = parsedArg[1];
            this.sshCommand = $"ssh -t {parsedArg[0]}@{parsedArg[1]} htop";
            OpenTerminal(this.sshCommand);
            this.ActionImageChanged(); // Notify the plugin service that the command display name and/or image has changed.
        }

        // This method is called when Loupedeck needs to show the command on the console or the UI.
        protected override String GetCommandDisplayName(
            String actionParameter,
            PluginImageSize imageSize
        ) => $"connect to {Environment.NewLine}{this.serverIp}";

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
            process.StartInfo.CreateNoWindow = false; // Don't open a separate window

            // Start the process
            process.Start();
            PluginLog.Info($"process started");


            // Wait for the process to finish
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
