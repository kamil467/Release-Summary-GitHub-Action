using System.Reflection;

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
       
       await PrintReleaseSummaryAsync(inputs,host);

    });

static async ValueTask PrintReleaseSummaryAsync(ActionInputs inputs, IHost host)
{
  
 //    
        
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
    //Read the first line of text
  
    //Continue to read until you reach end of file
    
    //close the file

    var gitHubOutputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
    if (!string.IsNullOrWhiteSpace(gitHubOutputFile))
    {
        using StreamWriter textWriter = new(gitHubOutputFile, true, Encoding.UTF8);
        textWriter.WriteLine($"summary-details={line}");
    }

}
catch(Exception e)
{
    Console.WriteLine("Exception: " + e.Message);
}

        await ValueTask.CompletedTask;
        Console.WriteLine($" Owner is : Name is {inputs.Owner}");
        Environment.Exit(0);

}



await host.RunAsync();