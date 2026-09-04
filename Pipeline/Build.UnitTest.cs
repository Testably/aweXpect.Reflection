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
			DotNetTest(s => s
					.SetConfiguration(Configuration)
					.SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
					.EnableNoBuild()
					.SetResultsDirectory(TestResultsDirectory)
					.CombineWith(
						UnitTestProjects,
						(settings, project) => settings
							.SetProjectFile(project)
							.CombineWith(
								project.GetTargetFrameworks()?.Except(excludedFrameworks),
								(frameworkSettings, framework) => frameworkSettings
									.SetFramework(framework)
									.When(s => s.Framework != "net48",
										// https://github.com/dotnet/reactive/issues/984
										c => c
											.SetDataCollector("XPlat Code Coverage")
											.AddLoggers($"trx;LogFileName={project.Name}_{framework}.trx"))
							)
					), completeOnFailure: true
			);
		});
}
