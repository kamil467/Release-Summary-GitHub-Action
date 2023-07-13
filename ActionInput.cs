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
         
        // read commit sha from environment variable.
        if(Environment.GetEnvironmentVariable("GITHUB_SHA") is {Length: > 0} commitsha)
             this.CommitId = commitsha;


        if(Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER") is { Length: > 0} repoOwner)
          this.Owner = repoOwner;
    

        if(Environment.GetEnvironmentVariable("GITHUB_REPO_READ_TOKEN") is { Length: > 0} accessToken)
          this.AccessToken = accessToken;
 
        if(Environment.GetEnvironmentVariable("GITHUB_ACTOR") is { Length: > 0} actor)
        this.CommitActor = actor;


    }

    public string CommitActor {get; private set;} 
    public string Owner { get; private set; }

    public string RepoName { get; private set; }

    public string CommitId { get; private set; }

    public string AccessToken {get; private set;}

    [Option('r', "release",
        Required = true,
        HelpText = "release name for example - v1, v2 .")]
    public string Release { get; set; } = null!;

    public string CommitMessage { get;set;} = null!;
}