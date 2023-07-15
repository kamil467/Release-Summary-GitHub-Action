using System;
using CommandLine; //  nuget package for processing command line arguments
namespace DotNet.GitHubAction;
// class for getting action inputs 
public class ActionInputs
{

    public ActionInputs()
    {

        if (Environment.GetEnvironmentVariable("GITHUB_REPOSITORY") is { Length: > 0 } repoName)
            this.RepoName = repoName.Contains("/") ? repoName.Split("/")[1] : repoName;
         
        // read commit sha from environment variable.
        if(Environment.GetEnvironmentVariable("GITHUB_SHA") is {Length: > 0} commitsha)
             this.CommitId = commitsha;


        if(Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER") is { Length: > 0} repoOwner)
          this.Owner = repoOwner;
 
        if(Environment.GetEnvironmentVariable("GITHUB_ACTOR") is { Length: > 0} actor)
        this.CommitActor = actor;


    }

   // set by GITHUB_ACTOR
    public string? CommitActor {get; private set;}

    // set by GITHUB_REPOSITORY_OWNER 
    public string? Owner { get; private set; }

    // set by GITHUB_REPOSITORY
    public string? RepoName { get; private set; }

    // set by GITHUB_SHA
    public string? CommitId { get; private set; }

    [Option('r', "release",
        Required = true,
        HelpText = "release name for example - v1, v2 .")]
    public string Release { get; set; } = null!;
     
     // retrieved from API
    public string CommitMessage { get;set;} = null!;

   // retrieved from API 
    public string ReleaseUrl{ get; set;} = null;
   // do not check-in this function


}