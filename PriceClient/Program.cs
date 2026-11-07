using System.Text;
using A2A;

Console.OutputEncoding = Encoding.UTF8;

var agentBaseUrl = new Uri("http://127.0.0.1:5100");
var product = args.Length == 0 ? "domates" : args[0];

try
{
    var resolver = new A2ACardResolver(agentBaseUrl);
    var card = await resolver.GetAgentCardAsync();
    Console.WriteLine($"Ajan: {card.Name}");
    foreach (var skill in card.Skills)
    {
        Console.WriteLine($"Beceri: {skill.Name} ({skill.Id})");
    }

    var client = A2AClientFactory.Create(card);
    var response = await client.SendMessageAsync(new SendMessageRequest
    {
        Message = new Message
        {
            MessageId = Guid.NewGuid().ToString("N"),
            Role = Role.User,
            Parts = [Part.FromText(product)]
        }
    });

    if (response.Message?.Parts.FirstOrDefault()?.Text is not string reply)
    {
        Console.Error.WriteLine("Fiyat Ajanı beklenen metin yanıtını döndürmedi.");
        return 1;
    }

    Console.WriteLine(reply);
    return 0;
}
catch (Exception exception) when (exception is HttpRequestException
    or A2AException { InnerException: HttpRequestException })
{
    Console.Error.WriteLine($"Fiyat Ajanı’na ulaşılamadı ({agentBaseUrl}). Sunucunun çalıştığını kontrol edin.");
    return 1;
}
catch (A2AException exception)
{
    Console.Error.WriteLine($"Fiyat Ajanı çağrısı başarısız oldu. {exception.Message}");
    return 1;
}
