using System.Reflection;
using Octokit;


using IHost host = Host.CreateDefaultBuilder(args)
    //.ConfigureServices((_, services) => services.AddGitHubActionServices())
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
           
       var listOfPr = await GetAssociatedPullRequest(inputs);
       await PrintReleaseSummaryAsync(inputs,host, listOfPr);

    });

static async ValueTask PrintReleaseSummaryAsync(ActionInputs inputs, IHost host, IEnumerable<dynamic> listOfPr)
{
    
        string line = null;
        try
      {
    //Pass the file path and file name to the StreamReader constructor
    var assembly = Assembly.GetExecutingAssembly();

    string resourcePath = "demo_action.release.md";

    using (Stream stream = assembly.GetManifestResourceStream(resourcePath))

    
    using(StreamReader sr = new StreamReader(stream)){
          line = sr.ReadToEnd();
          Console.WriteLine(line);
    }
    
    // replace release version
    line = line.Replace("{{release}}",inputs.Release);
    line = line.Replace("{{author}}",inputs.Owner);
     
     var prContent = "";
     foreach(var pr in listOfPr)
     {
         prContent = ""

     }

    line = line.Replace("{{pr}}")
    
    string outputFile = "release.md";
    if (File.Exists(outputFile))    
    {    
        File.Delete(outputFile);    
    }    
    

    // Create a new file     
    using (FileStream fs = File.Create(outputFile))     
    {    
        // Add some text to file    
        Byte[] summarContent = new UTF8Encoding(true).GetBytes(line);    
        fs.Write(summarContent, 0, summarContent.Length); 
    } 

}
catch(Exception e)
{
    Console.WriteLine("Exception: " + e.Message);
    Environment.Exit(1);
}

        await ValueTask.CompletedTask;
        Console.WriteLine($" Owner is : Name is {inputs.Owner}");
        Environment.Exit(0);

}


static async Task<IEnumerable<dynamic>> GetAssociatedPullRequest(ActionInputs inputs)
{
    var tokenAuth = new Credentials(inputs.AccessToken); // This can be a PAT or an OAuth token.

     var client = new GitHubClient(new Octokit.ProductHeaderValue("github_release_summary_pull_request"));
     client.Credentials = tokenAuth;
      var query = await client
                             .Repository
                             .Commit
                             .PullRequests(owner:inputs.Owner, name: inputs.RepoName, sha1: inputs.CommitId);

     return  query.Select(pr => new {
               
               PRquestURL = pr.Url,
               Body = pr.Body,
               Title = pr.Title,
               State = pr.State,
             }).ToList();                             
}


await host.RunAsync();