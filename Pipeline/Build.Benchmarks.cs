using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fallout.Common;
using Fallout.Common.IO;
using Fallout.Common.Tools.DotNet;
using Octokit;
using Serilog;
using static Fallout.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	Target BenchmarkDotNet => _ => _
		.Executes(() =>
		{
			AbsolutePath benchmarkDirectory = ArtifactsDirectory / "Benchmarks";
			benchmarkDirectory.CreateOrCleanDirectory();

			DotNetBuild(s => s
				.SetProjectFile(Solution.Benchmarks.aweXpect_Reflection_Benchmarks)
				.SetConfiguration(Configuration.Release)
				.EnableNoLogo());

			DotNet(
				$"{Solution.Benchmarks.aweXpect_Reflection_Benchmarks.Name}.dll --exporters json --filter * --artifacts \"{benchmarkDirectory}\"",
				Solution.Benchmarks.aweXpect_Reflection_Benchmarks.Directory / "bin" / "Release");
		});

	Target BenchmarkResult => _ => _
		.After(BenchmarkDotNet)
		.Executes(async () =>
		{
			string fileContent = await File.ReadAllTextAsync(ArtifactsDirectory / "Benchmarks" / "results" /
			                                                 "aweXpect.Reflection.Benchmarks.HappyCaseBenchmarks-report-github.md");
			Log.Information("Report:\n {FileContent}", fileContent);
		});

	Target BenchmarkComment => _ => _
		.Executes(async () =>
		{
			// The artifact is untrusted, because the pull request controls the code that produces it, so the pull
			// request to comment on comes from the event of the workflow run.
			int? prId = await BuildExtensions.ResolvePullRequestOfWorkflowRun(GithubToken);
			if (prId == null)
			{
				Log.Information("Skip writing a comment, as the workflow run belongs to no pull request.");
				return;
			}

			await "Benchmarks".DownloadArtifactTo(ArtifactsDirectory, GithubToken);
			if (!File.Exists(ArtifactsDirectory / "Benchmarks" / "results" /
			                 "aweXpect.Reflection.Benchmarks.HappyCaseBenchmarks-report-github.md"))
			{
				Log.Information("Skip writing a comment, as the workflow run produced no benchmark report.");
				return;
			}

			string body = CreateBenchmarkCommentBody().AsUntrustedCommentContent();
			GitHubClient gitHubClient = new(new ProductHeaderValue("Fallout"));
			Credentials tokenAuth = new(GithubToken);
			gitHubClient.Credentials = tokenAuth;
			IReadOnlyList<IssueComment> comments =
				await gitHubClient.Issue.Comment.GetAllForIssue("Testably", "aweXpect.Reflection", prId.Value);
			long? commentId = null;
			Log.Information($"Found {comments.Count} comments");
			foreach (IssueComment comment in comments)
			{
				if (comment.IsPipelineComment("## :rocket: Benchmark Results"))
				{
					Log.Information($"Found comment: {comment.Body}");
					commentId = comment.Id;
				}
			}

			if (commentId == null)
			{
				Log.Information($"Create comment:\n{body}");
				await gitHubClient.Issue.Comment.Create("Testably", "aweXpect.Reflection", prId.Value, body);
			}
			else
			{
				Log.Information($"Update comment:\n{body}");
				await gitHubClient.Issue.Comment.Update("Testably", "aweXpect.Reflection", commentId.Value, body);
			}
		});

	Target Benchmarks => _ => _
		.DependsOn(BenchmarkDotNet)
		.DependsOn(BenchmarkResult);

	string CreateBenchmarkCommentBody()
	{
		string[] fileContent = File.ReadAllLines(ArtifactsDirectory / "Benchmarks" / "results" /
		                                         "aweXpect.Reflection.Benchmarks.HappyCaseBenchmarks-report-github.md");
		StringBuilder sb = new();
		sb.AppendLine("## :rocket: Benchmark Results");
		sb.AppendLine("<details>");
		sb.AppendLine("<summary>Details</summary>");
		int count = 0;
		foreach (string line in fileContent)
		{
			if (line.StartsWith("```"))
			{
				count++;
				if (count == 1)
				{
					sb.AppendLine("<pre>");
				}
				else if (count == 2)
				{
					sb.AppendLine("</pre>");
					sb.AppendLine("</details>");
					sb.AppendLine();
				}

				continue;
			}

			if (line.StartsWith('|') && line.Contains("_aweXpect") && line.EndsWith('|'))
			{
				MakeLineBold(sb, line);
				continue;
			}

			sb.AppendLine(line);
		}

		string body = sb.ToString();
		return body;
	}

	static void MakeLineBold(StringBuilder sb, string line)
	{
		string[] tokens = line.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		sb.Append('|');
		foreach (string token in tokens)
		{
			sb.Append(" **");
			sb.Append(token);
			sb.Append("** |");
		}

		sb.AppendLine();
	}
}
