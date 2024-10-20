namespace Loupedeck.ClusterControlPlugin
{
    using System;
    using System.Diagnostics;

    // This class implements an example command that counts button presses.

    public class TmuxDynamicFolder : PluginDynamicFolder
    {
        private String userName = "chihyu18";
        private String serverIp = "192.168.176.33";

        // Initializes the command class.
        public TmuxDynamicFolder()
        {
            this.DisplayName = "tmux";
            this.GroupName = "System";
            // this.MakeProfileAction("text;Enter ssh command:");
            // "Back" button is automatically inserted at the top of the left encoder page area.
            // This is possible only if the dynamic folder does not define any encoder actions
            // (neither rotation nor reset ones)
            this.Navigation = PluginDynamicFolderNavigation.EncoderArea;
        }

        // public override String GetButtonDisplayName(PluginImageSize imageSize) => $"{this.ChannelCount} Channels";

        public void NumpadDynamicFolder()
        {
            this.DisplayName = "Tmux Sessions";
            // navigation is fully done by the plugin developer.
            this.Navigation = PluginDynamicFolderNavigation.None;
        }

        public override IEnumerable<String> GetButtonPressActionNames()
        {
            return new[]
            {
                PluginDynamicFolder.NavigateUpActionName,
                this.CreateCommandName("0"),
                this.CreateCommandName("1"),
                this.CreateCommandName("2"),
                this.CreateCommandName("3"),
                this.CreateCommandName("4"),
                this.CreateCommandName("5"),
                this.CreateCommandName("6"),
                this.CreateCommandName("7"),
            };
        }

        public override void RunCommand(String actionParameter)
        {
            String sshCommand = "";
            // execute the selected command here
            switch (actionParameter)
            {
                case "0":
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux attach-session -t 0 || tmux new-session -s 0'");
                    PluginLog.Info($"Switch to session 0!");
                    break;
                case "1":
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux attach-session -t 1 || tmux new-session -s 1'");
                    PluginLog.Info($"Switch to session 1!");
                    break;
                case "2":
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux attach-session -t 2 || tmux new-session -s 2'");
                    PluginLog.Info($"Switch to session 2!");
                    break;
                case "3":
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux attach-session -t 3 || tmux new-session -s 3'");
                    PluginLog.Info($"Switch to session 3!");
                    break;
                case "4":
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux attach-session -t 4 || tmux new-session -s 4'");
                    PluginLog.Info($"Switch to session 4!");
                    break;
                case "5":
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux attach-session -t 5 || tmux new-session -s 5'");
                    PluginLog.Info($"Switch to session 5!");
                    break;
                case "6":
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux attach-session -t 6 || tmux new-session -s 6'");
                    PluginLog.Info($"Switch to session 6!");
                    break;
                case "7":
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux attach-session -t 7 || tmux new-session -s 7'");
                    PluginLog.Info($"Switch to session 7!");
                    break;
                default:
                    // String[] parsedArg = actionParameter.Split(' ');
                    // this.userName = parsedArg[0];
                    // this.serverIp = parsedArg[1];
                    OpenTerminal($"ssh -t {this.userName}@{this.serverIp} 'tmux'");
                    PluginLog.Info($"You pressed tmux!");
                    break;
            }
            // close the folder after the command
            this.Close();
        }

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

            // Interactive loop to send commands and get output
            StreamWriter inputWriter = process.StandardInput;
            StreamReader outputReader = process.StandardOutput;
            StreamReader errorReader = process.StandardError;

            // Ensure the process exits
            process.WaitForExit();

            inputWriter.Close();
            outputReader.Close();
        }

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
