using System.Diagnostics;

namespace CqrsScaffold.Tool.Validation
{
    public class SolutionValidator
    {
        public async Task<bool> ValidateAsync(string outputPath, string projectName)
        {
            var solutionPath = Path.Combine(outputPath, projectName);

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build",
                WorkingDirectory = solutionPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi);
            
            if (process == null)
            {
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrEmpty(output))
            {
                Console.WriteLine(output);
            }

            if (!string.IsNullOrEmpty(error))
            {
                Console.Error.WriteLine(error);
            }

            return process.ExitCode == 0;
        }
    }
}
