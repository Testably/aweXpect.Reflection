using System.Runtime.InteropServices;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Solutions;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable UnusedMember.Local
// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	Project[] AotSmokeTestProjects =>
	[
		Solution.Tests.Aot.aweXpect_Reflection_Aot,
	];

	/// <summary>
	///     Runs the smoke tests from the compiled output and again published with Native AOT for the current platform.
	/// </summary>
	/// <remarks>
	///     The trim analyzer checks annotations, not behaviour, so this is the only place that verifies that an
	///     expectation in a trimmed application returns the same result as under the JIT or fails naming the switch.
	/// </remarks>
	Target AotSmokeTests => _ => _
		.DependsOn(Compile)
		.Executes(() =>
		{
			foreach (Project project in AotSmokeTestProjects)
			{
				DotNetRun(s => s
					.SetProjectFile(project)
					.SetConfiguration(Configuration)
					.EnableNoBuild());

				AbsolutePath output = ArtifactsDirectory / "Aot" / project.Name;
				DotNetPublish(s => s
					.SetProject(project)
					.SetConfiguration(Configuration)
					.SetRuntime(RuntimeInformation.RuntimeIdentifier)
					.SetOutput(output));

				string executable = output / (project.Name + (EnvironmentInfo.IsWin ? ".exe" : ""));
				ProcessTasks.StartProcess(executable, string.Empty, output)
					.AssertZeroExitCode();
			}
		});
}
