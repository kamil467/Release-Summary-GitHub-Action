using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Octokit;

var token = "default";
// read token from environment variable for api authentication
if(Environment.GetEnvironmentVariable("GITHUB_REPO_READ_TOKEN") is { Length: > 0} accessToken)
      token = accessToken;

using IHost host = Host.CreateDefaultBuilder(args)
                   .ConfigureServices(services => {
                    services.AddScoped<IGitHubClient>(provider => {
                       return new GitHubClient(new Octokit.ProductHeaderValue("github_release_summary_pull_request")
                       )
                       {
                        Credentials = new Credentials(token)
                       };
                    });
                   }) 
                    .Build();


static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();


var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
   errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program")
            .LogError(
                string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

         Environment.Exit(2);
    }).WithParsed( async inputs =>{
           
      Console.WriteLine($"Repository Name:{inputs.RepoName}");
      Console.WriteLine($"Owner:{inputs.Owner}");

       var listOfPr = await GetAssociatedPullRequest(inputs, host);
        inputs.CommitMessage =  await GetCommitMessageById( inputs,host);
        inputs.ReleaseUrl = await GetTagUrl(inputs, host);
       await PrintReleaseSummaryAsync(inputs,host, listOfPr);

    });

static async ValueTask PrintReleaseSummaryAsync(ActionInputs inputs, IHost host, CommitPullRequest pr)
{
    
       var exitCode = 0;
        try
      {
         string? line = null;
    //Pass the file path and file name to the StreamReader constructor
    var assembly = Assembly.GetExecutingAssembly();

    string resourcePath = "DotNet.GitHubAction.Template.release-summary.md";

    await using (Stream stream = assembly.GetManifestResourceStream(resourcePath))

    
    using(StreamReader sr = new StreamReader(stream)){
          line = sr.ReadToEnd();
          
    }
    
    // replace release version
    if(inputs.ReleaseUrl is null)
       line = line.Replace("{{release}}",inputs.Release);
      else
         line = line.Replace("{{release}}",$"[{inputs.Release}]({inputs.ReleaseUrl})");

    line = line.Replace("{{actor}}",inputs.CommitActor);
    line = line.Replace("{{time}}",DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
    line = line.Replace("{{commitid}}",inputs.CommitId);
    
    if(!(inputs.CommitMessage is null))
     line = line.Replace("{{message}}",inputs.CommitMessage.RemoveLineBreaks());
     
   if(pr is null)
        Console.WriteLine("Unable to reterive PR associated with this commit");
      else
        line = line.Replace("{{pr}}",$"- [{pr.Number}]({pr.HtmlUrl}) <br />  - Title: {pr.Title.RemoveLineBreaks()} <br /> - Message: <p>{pr.Body.RemoveLineBreaks()} </p>");
    
    string outputFile = "release-summary.md";
    if (File.Exists(outputFile))    
    {    
        Console.WriteLine("Deleting existing release-summary.md file");
        File.Delete(outputFile);    
    }    
    

    // Create a new file     
    using (FileStream fs = File.Create(outputFile))     
    {    
        // Add some text to file    
        Byte[] summarContent = new UTF8Encoding(true).GetBytes(line);    
        fs.Write(summarContent, 0, summarContent.Length); 
    } 

    Console.WriteLine("$EXIT CODE:{exitCode} - release summary written to release-summary.md file and available at runner workspace. github/workspace/release-summary.md");

    Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", line);

    Console.WriteLine("release summary written to GITHUB_STEP_SUMMARY ENV variable");

}
catch(Exception e)
{
    Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program.PrintReleaseSummaryAsync")
            .LogError(string.Join(Environment.NewLine, e.Message,e.StackTrace));
    exitCode = 1;
}

        await ValueTask.CompletedTask;
   
        Environment.Exit(exitCode);

}


static async Task<CommitPullRequest?> GetAssociatedPullRequest(ActionInputs inputs, IHost host)
{
    
    try{
      var client = Get<IGitHubClient>(host);
      var query = await client
                             .Repository
                             .Commit
                             .PullRequests(owner:inputs.Owner, name: inputs.RepoName, sha1: inputs.CommitId);

     return  query.Where( pr => pr.State == ItemState.Closed && pr.Merged == true).FirstOrDefault();                          
    }
    catch(Exception e)
    {
         Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program.GetAssociatedPullRequest")
            .LogError(string.Join(Environment.NewLine, e.Message,e.StackTrace));
        return null;
    }
}

static async Task<string?> GetCommitMessageById( ActionInputs inputs, IHost host)
{
    try{
     var client = Get<IGitHubClient>(host);
      var query = await client.Repository.Commit.Get(owner: inputs.Owner, name:inputs.RepoName, inputs.CommitId).ConfigureAwait(false);
      Console.WriteLine("commit message successfully restrieved.");
      return query?.Commit?.Message;

    }
    catch(Exception e)
    {
         Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program.GetCommitMessageById")
            .LogError(string.Join(Environment.NewLine, e.Message,e.StackTrace));
        return null;
    }
}

static async Task<string?> GetTagUrl(ActionInputs inputs, IHost host)
{
    try{
       var client = Get<IGitHubClient>(host);
       var query = await client.Repository.
                 Release.Get(inputs.Owner, inputs.RepoName, inputs.Release)
                  .ConfigureAwait(false);
       return query?.HtmlUrl;
    }
    catch(Exception e )
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program.GetTagUrl")
            .LogError(string.Join(Environment.NewLine, e.Message,e.StackTrace));
        return null;
    }
}




await host.RunAsync();