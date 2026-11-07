using A2A;
using A2A.AspNetCore;

const string baseUrl = "http://127.0.0.1:5100";
const string agentPath = "/a2a";

var builder = WebApplication.CreateBuilder(args);

var card = new AgentCard
{
    Name = "Fiyat Ajanı",
    Description = "Örnek ürünlerin kilogram fiyatını verir.",
    Version = "1.0.0",
    SupportedInterfaces =
    [
        new AgentInterface
        {
            Url = baseUrl + agentPath,
            ProtocolBinding = ProtocolBindingNames.JsonRpc,
            ProtocolVersion = "1.0"
        }
    ],
    DefaultInputModes = ["text/plain"],
    DefaultOutputModes = ["text/plain"],
    Capabilities = new AgentCapabilities { Streaming = false },
    Skills =
    [
        new AgentSkill
        {
            Id = "price-lookup",
            Name = "Fiyat sorgulama",
            Description = "Domates veya patates için örnek kilogram fiyatını verir.",
            Tags = ["fiyat"],
            Examples = ["domates", "patates"]
        }
    ]
};

builder.Services.AddA2AAgent<PriceLookupAgent>(card);

var app = builder.Build();
app.MapWellKnownAgentCard(card);
app.MapA2A(agentPath);
app.Run(baseUrl);

sealed class PriceLookupAgent : IAgentHandler
{
    public async Task ExecuteAsync(
        RequestContext context,
        AgentEventQueue eventQueue,
        CancellationToken cancellationToken)
    {
        var reply = context.UserText?.Trim().ToLowerInvariant() switch
        {
            "domates" => "Domates: 30 TL/kg",
            "patates" => "Patates: 20 TL/kg",
            _ => "Ürün bulunamadı. Desteklenen ürünler: domates, patates."
        };

        var responder = new MessageResponder(eventQueue, context.ContextId);
        await responder.ReplyAsync(reply, cancellationToken: cancellationToken);
    }
}
