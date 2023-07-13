using System.Reflection;
using Octokit;


using IHost host = Host.CreateDefaultBuilder(args)
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
           
       var listOfPr = await GetAssociatedPullRequest(inputs, host);
        inputs.CommitMessage =  await GetCommitMessageById( inputs,host);
       await PrintReleaseSummaryAsync(inputs,host, listOfPr);

    });

static async ValueTask PrintReleaseSummaryAsync(ActionInputs inputs, IHost host, dynamic pr)
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
    line = line.Replace("{{release}}",inputs.Release);
    line = line.Replace("{{actor}}",inputs.CommitActor);
    line = line.Replace("{{time}}",DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
    line = line.Replace("{{commitid}}",inputs.CommitId);
    
    if(!(inputs.CommitMessage is null))
     line = line.Replace("{{message}}",inputs.CommitMessage);
     
   if(pr is null)
        Console.WriteLine("Unable to reterive PR associated with this commit");
      else
        line = line.Replace("{{pr}}",$"- [{pr.Number}]({pr.PRquestURL}) <br />  - Title: {pr.Title} <br /> - Message: {pr.Body} ");
    
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
    Console.WriteLine("Exception: " + e.Message);
    exitCode = 1;
}

        await ValueTask.CompletedTask;
   
        Environment.Exit(exitCode);

}


static async Task<dynamic?> GetAssociatedPullRequest(ActionInputs inputs, IHost host)
{
    try{
    var tokenAuth = new Credentials(inputs.AccessToken); // This can be a PAT or an OAuth token.

     var client = new GitHubClient(new Octokit.ProductHeaderValue("github_release_summary_pull_request"));
     client.Credentials = tokenAuth;
      var query = await client
                             .Repository
                             .Commit
                             .PullRequests(owner:inputs.Owner, name: inputs.RepoName, sha1: inputs.CommitId);

     return  query.Select(pr => new {
               
               PRquestURL = pr.HtmlUrl,
               Body = pr.Body,
               Title = pr.Title,
               State = pr.State,
               IsMerged =  pr.Merged,
                Number =  pr.Number
             }).Where( pr => pr.State == ItemState.Closed && pr.IsMerged == true).FirstOrDefault();                          
    }
    catch(Exception e)
    {
         Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program")
            .LogError(string.Join(Environment.NewLine, e.Message,e.StackTrace));
        return null;
    }
}

static async Task<string?> GetCommitMessageById( ActionInputs inputs, IHost host)
{
    try{
    var tokenAuth = new Credentials(inputs.AccessToken); // This can be a PAT or an OAuth token.

     var client = new GitHubClient(new Octokit.ProductHeaderValue("github_release_summary_pull_request"));
     client.Credentials = tokenAuth;
      var query = await client.Repository.Commit.Get(owner: inputs.Owner, name:inputs.RepoName, inputs.CommitId).ConfigureAwait(false);
  
      return query?.Commit?.Message;

    }
    catch(Exception e)
    {
         Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program")
            .LogError(string.Join(Environment.NewLine, e.Message,e.StackTrace));
        return null;
    }
}


await host.RunAsync();