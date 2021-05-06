using System.Diagnostics;

namespace react_app.Utils
{
    public class CommandExecutor
    {
        public void Permission(string mode, string path)
        {
            Execute($"chmod {mode} {path}");
        }

        public void Execute(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
