using System.Linq;
using Fallout.Common;
using Fallout.Solutions;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	Target UnitTests => _ => _
		.DependsOn(DotNetUnitTests);

	Project[] UnitTestProjects =>
	[
		Solution.Tests.aweXpect_Reflection_Tests,
		Solution.Tests.aweXpect_Reflection_Internal_Tests,
	];

	Target DotNetUnitTests => _ => _
		.Unlisted()
		.DependsOn(Compile)
		.Executes(() =>
		{
			string[] excludedFrameworks =
				EnvironmentInfo.IsWin
					? []
					: ["net48",];
			(Project Project, string Framework)[] testRuns = UnitTestProjects
				.SelectMany(project => (project.GetTargetFrameworks() ?? [])
					.Except(excludedFrameworks)
					.Select(framework => (project, framework)))
				.ToArray();

			Assert.NotEmpty(testRuns);

			// Each run is started with `dotnet run`, so that its TRX and coverage file get a name of their own.
			DotNetRun(s => s
					.SetConfiguration(Configuration)
					.SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
					.EnableNoBuild()
					.CombineWith(
						testRuns,
						(settings, testRun) => settings
							.SetProjectFile(testRun.Project)
							.SetFramework(testRun.Framework)
							.SetProcessAdditionalArguments(
								"--",
								"--disable-logo",
								$"--results-directory \"{TestResultsDirectory}\"",
								"--report-trx",
								$"--report-trx-filename {testRun.Project.Name}_{testRun.Framework}.trx",
								"--coverage",
								"--coverage-output-format cobertura",
								$"--coverage-output {testRun.Project.Name}_{testRun.Framework}.cobertura.xml")),
				completeOnFailure: true);
		});
}
