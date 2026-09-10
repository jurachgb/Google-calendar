using Ical.Net;
using Microsoft.Playwright;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

if (args.Length == 0)
{
    return;
}

string entrada  = args[0];
string conteudo = "";

if (entrada.StartsWith("http"))
{
    System.Console.WriteLine("Abrindo navegador para baixar o arquivo...");

    var playwright= await Playwright.CreateAsync();
    var browser= await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions{Headless = false});

    var contexto = await browser.NewContextAsync(new BrowserNewContextOptions
    {
        AcceptDownloads = true
    });

    var page = await contexto.NewPageAsync();
    IDownload? download = null;
    page.Download += (_, d) => download = d;

    try { await page.GotoAsync(entrada); } catch { }

    int tentativas = 0;
    while (download == null && tentativas < 10)
    {
        await Task.Delay(1000);
        tentativas++;
    }
    if (download == null)
    {
        Console.WriteLine("Erro: download não iniciou.");
        return;
    }
    
    await download.SaveAsAsync("calendario.ics");
    await browser.CloseAsync();

    System.Console.WriteLine("Arquivo salvo");
    conteudo=File.ReadAllText("calendario.ics");
}
else
{
    
    if (!File.Exists(entrada))
    {
        Console.WriteLine($"Arquivo não encontrado: {entrada}");
        return;
    }

    conteudo = File.ReadAllText(entrada);
}

// Etapa 2
var calendario= Ical.Net.Calendar.Load(conteudo);
var eventos=calendario.Events.ToList();

//Google

var credencial = ServiceAccountCredential.FromServiceAccountData(File.OpenRead("credentials.json")).ToGoogleCredential().CreateScoped(CalendarService.Scope.Calendar);

var googleCalendar=new CalendarService(new BaseClientService.Initializer
{
    HttpClientInitializer=credencial,
    ApplicationName="ICS sync"
});

var listaExistentes  = await googleCalendar.Events.List(Info.Dados.Id).ExecuteAsync();
var uidsJaInseridos  = listaExistentes.Items?
    .Where(ev => ev.ExtendedProperties?.Private__?.ContainsKey("icsUid") == true)
    .Select(ev => ev.ExtendedProperties.Private__["icsUid"])
    .ToHashSet() ?? [];



foreach (var evento in eventos.Where(e => !uidsJaInseridos.Contains(e.Uid ?? "")))
{
    var novoEvento = new Event
    {
        Summary     = evento.Summary,
        Description = evento.Description,
        Location    = evento.Location,
        ExtendedProperties = new Event.ExtendedPropertiesData
        {
            Private__ = new Dictionary<string, string> { { "icsUid", evento.Uid ?? "" } }
        }
    };

    if (evento.IsAllDay)
    {
        var data = evento.DtStart.Value.Date;
        novoEvento.Start = new EventDateTime { Date = data.ToString("yyyy-MM-dd") };
        novoEvento.End   = new EventDateTime { Date = data.AddDays(1).ToString("yyyy-MM-dd") };
    }
    else
    {
        var inicio = evento.DtStart.Value;
        var fim    = evento.DtEnd?.Value ?? inicio.AddHours(1);

        novoEvento.Start = new EventDateTime
        {
            DateTimeDateTimeOffset = new DateTimeOffset(inicio, TimeSpan.FromHours(-3)),
            TimeZone = "America/Sao_Paulo"
        };
        novoEvento.End = new EventDateTime
        {
            DateTimeDateTimeOffset = new DateTimeOffset(fim, TimeSpan.FromHours(-3)),
            TimeZone = "America/Sao_Paulo"
        };
    }

    await googleCalendar.Events.Insert(novoEvento, Info.Dados.Id).ExecuteAsync();
    Console.WriteLine($"+ {evento.Summary}");
}

Console.WriteLine("\nPronto!");
