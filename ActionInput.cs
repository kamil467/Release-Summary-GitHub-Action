using System;
using CommandLine; //  nuget package for processing command line arguments
namespace DotNet.GitHubAction;
// class for getting action inputs 
public class ActionInputs
{

    public ActionInputs()
    {
        if (Environment.GetEnvironmentVariable("GITHUB_REPOSITORY") is { Length: > 0 } repoName)
          this.RepoName = repoName;
        else
         this.RepoName = "devops_playground";  // remove after testing 

        // read commit sha from environment variable.
        if(Environment.GetEnvironmentVariable("GITHUB_SHA") is {Length: > 0} commitsha)
             this.CommitId = commitsha;
             else 
             this.CommitId = ""; 

        if(Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER") is { Length: > 0} repoOwner)
          this.Owner = repoOwner;
          else 
          this.Owner ="kamil467"; // remove after testing


        if(Environment.GetEnvironmentVariable("GITHUB_REPO_READ_TOKEN") is { Length: > 0} accessToken)
          this.AccessToken = accessToken;
          else 
          this.AccessToken =""; // remove after testing

    }

    public string Owner { get; private set; }

    public string RepoName { get; private set; }

    public string CommitId { get; private set; }

    public string AccessToken {get; private set;}

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