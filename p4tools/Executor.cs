using Spectre.Console;
using System.Diagnostics;

namespace p4tools
{
    public class Executor
    {
        private string m_workingDir = string.Empty;
        public List<string> StdOut { get; } = new List<string>();
        public List<string> StdErr { get; } = new List<string>();
        private readonly bool m_showOutput = false;

        public Executor(string workingDir, bool showOutput = false)
        {
            m_workingDir = workingDir;
            m_showOutput = showOutput;
        }

        public int ExecuteProcess(string path, params string[] args)
        {
            return ExecuteProcess(path, (IEnumerable<string>)args);
        }

        public int ExecuteProcess(string path, IEnumerable<string> args)
        {
            string? tempfile = null;
            try
            {
                string arguments = string.Join(" ", args);
                if (arguments.Length > 8000)
                {
                    tempfile = Path.GetTempFileName();
                    File.WriteAllLines(tempfile, args);
                    arguments = $"@{tempfile}";
                }

                var startInfo = new ProcessStartInfo(path, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = m_workingDir,
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        process.OutputDataReceived += Process_OutputDataReceived;
                        process.ErrorDataReceived += Process_ErrorDataReceived;
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        return process.ExitCode;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            finally
            {
                if (tempfile != null)
                {
                    File.Delete(tempfile);
                }
            }
        }

        protected virtual void OnOutput(string message)
        {
            if (m_showOutput)
            {
                AnsiConsole.WriteLine(message);
            }
            StdOut.Add(message);
        }

        protected virtual void OnError(string message)
        {
            StdErr.Add(message);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!ReferenceEquals(e.Data, null))
            {
                OnOutput(e.Data);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!ReferenceEquals(e.Data, null))
            {
                OnError(e.Data);
            }
        }
    }
}
