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
    
    // replace release version
    line = line.Replace("{{release}}",inputs.Release);
    line = line.Replace("{{author}}",inputs.Owner);
    
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



await host.RunAsync();