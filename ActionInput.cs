using System;
using CommandLine; //  nuget package for processing command line arguments
namespace DotNet.GitHubAction;
// class for getting action inputs 
public class ActionInputs
{
    string _repositoryName = null!;
    string _branchName = null!;

    string commitId = null!;

    public ActionInputs()
    {
        if (Environment.GetEnvironmentVariable("GREETINGS") is { Length: > 0 } greetings)
        {
            Console.WriteLine(greetings);
        }

        // read commit sha from environment variable.
        if(Environment.GetEnvironmentVariable("GITHUB_SHA") is {Length: > 0} commitsha)
        {
             this.commitId = commitsha;
        }
    }

    [Option('o', "owner",
        Required = true,
        HelpText = "The owner, for example: \"dotnet\". Assign from `github.repository_owner`.")]
    public string Owner { get; set; } = null!;

    [Option('r', "releasae",
        Required = true,
        HelpText = "release name for example - v1, v2 .")]
    public string Release { get; set; } = null!;


    static void ParseAndAssign(string? value, Action<string> assign)
    {
        if (value is { Length: > 0 } && assign is not null)
        {
            assign(value.Split("/")[^1]);
        }
    }
}