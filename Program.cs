using Ical.Net;
using Microsoft.Playwright;

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

var calendario = Calendar.Load(conteudo);
var eventos    = calendario.Events.ToList();

Console.WriteLine($"\n{eventos.Count} evento(s) encontrado(s):\n");

foreach (var evento in eventos)
{
    Console.WriteLine($"Titul\t: {evento.Summary}");
    Console.WriteLine($"Data\t: {evento.DtStart}");
    Console.WriteLine($"UID\t\t: {evento.Uid}");
    Console.WriteLine();
}