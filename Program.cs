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
    StreamReader sr = new StreamReader("release.md");
    //Read the first line of text
    line = sr.ReadLine();
    //Continue to read until you reach end of file
    while (line != null)
    {
        //Read the next line
        Console.WriteLine(line);
        line = sr.ReadLine();
    }
    //close the file
    sr.Close();

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