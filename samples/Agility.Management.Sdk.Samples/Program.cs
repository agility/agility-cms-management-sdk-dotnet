// A read-only tour of an instance. Run it with:
//
//   AGILITY_TOKEN=<personal access token> dotnet run --project samples/Agility.Management.Sdk.Samples -- <instance guid> [locale]
//
// The other files in this project are the code from the guides in docs/, kept here so the build checks them.

using Agility.Management.Sdk;

if (args.Length < 1 || Environment.GetEnvironmentVariable("AGILITY_TOKEN") is not { Length: > 0 } token)
{
    Console.Error.WriteLine("Usage: AGILITY_TOKEN=<token> dotnet run -- <instance guid> [locale]");
    return 1;
}

var locale = args.Length > 1 ? args[1] : "en-us";

using var client = new AgilityManagementClient(new AgilityManagementOptions
{
    AccessToken = token,
    ApplicationName = "agility-sdk-sample/1.0",
});
var instance = client.ForInstance(args[0]);
Console.WriteLine($"Instance {instance.InstanceGuid} at {instance.BaseUrl}");

try
{
    var locales = await instance.Locales.GetLocalesAsync();
    Console.WriteLine($"Locales: {string.Join(", ", locales.Select(l => l.LocaleCode))}");

    var models = await instance.Models.GetContentModelsAsync();
    Console.WriteLine($"{models.Count} content models");

    var containers = await instance.Containers.GetContainerListAsync();
    Console.WriteLine($"{containers.Count} containers");

    foreach (var channel in await instance.Pages.GetSitemapAsync(locale))
        Console.WriteLine($"Channel {channel.Name}: {channel.Pages?.Count ?? 0} top-level pages");

    var status = await instance.SyncStatus.GetFetchApiStatusAsync();
    Console.WriteLine(status.InProgress ? "The Fetch API is syncing." : "The Fetch API is up to date.");
    return 0;
}
catch (AgilityManagementException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}
