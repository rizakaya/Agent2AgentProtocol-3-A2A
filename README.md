# .NET ile A2A — Fiyat Ajanı

Bu örnekte bir konsol uygulaması, Fiyat Ajanı’nı keşfeder ve ona bir ürün adı gönderir. Ajan iki örnek kilogram fiyatını kod içinde tutar. Örneği çalıştırmak için .NET 10 SDK yeterlidir. İki proje de resmî A2A .NET SDK’nın `1.0.0-preview4` sürümünü kullanır. [A2A .NET SDK](https://github.com/a2aproject/a2a-dotnet/releases/tag/v1.0.0-preview4)

## Çalıştır

Proje klasöründe ilk terminali aç ve ajanı başlat.

```powershell
dotnet run --project PriceAgent --no-launch-profile
```

İkinci terminalde aynı klasörden fiyatı sor.

```powershell
dotnet run --project PriceClient -- domates
```

```text
Ajan: Fiyat Ajanı
Beceri: Fiyat sorgulama (price-lookup)
Domates: 30 TL/kg
```

Ürün adı verilmezse istemci `domates` kullanır. `patates` için `Patates: 20 TL/kg` döner. `salatalık` gönderirsen ajan desteklediği ürünleri listeler. Ajanı durdurup istemciyi tekrar çalıştırırsan bağlantı hatasını görebilirsin.

## A2A akışını incele

Ajan çalışırken [PriceAgent.http](PriceAgent/PriceAgent.http) dosyasını Visual Studio’da aç. Agent Card ve üç ürün sorgusunu, ilgili isteğin üstündeki **Send Request** düğmesiyle gönderebilirsin.

### 1. Agent Card ile keşfet

Ajan çalışırken kartı PowerShell’de görüntüle.

```powershell
Invoke-RestMethod http://127.0.0.1:5100/.well-known/agent-card.json |
    ConvertTo-Json -Depth 8
```

Karttaki `skills` alanı ajanın fiyat sorgulayabildiğini, `supportedInterfaces` alanı ise hangi adreste ve hangi protokol üzerinden çağrılacağını söyler. İstemciye başlangıç adresini veririz. İstemci, mesajı göndereceği `/a2a` adresini karttan öğrenir.

### 2. SendMessage ile işi devret

SDK, kartta duyurulan adrese JSON-RPC isteği gönderir. Bu örnekte gövdenin yanında `Content-Type: application/json` ve `A2A-Version: 1.0` başlıkları kullanılır.

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "SendMessage",
  "params": {
    "message": {
      "messageId": "demo-request-1",
      "role": "ROLE_USER",
      "parts": [{ "text": "domates" }]
    }
  }
}
```

Fiyat Ajanı sonucu hemen bir mesaj olarak döndürür. Yanıttaki mesaj ve bağlam kimlikleri her yeni sorguda üretilir.

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "message": {
      "role": "ROLE_AGENT",
      "parts": [{ "text": "Domates: 30 TL/kg" }],
      "messageId": "989c3f02b31846649597ecf17ccf36a0",
      "contextId": "c52f76e8b4b44e5d84194ece2084561e"
    }
  }
}
```

İstemci fiyatların nerede tutulduğunu bilmeden bu işi ajana devreder. Başka bir dilde yazılan bir istemci de aynı kartı okuyup aynı A2A çağrısını yapabilir. [Google’ın A2A rehberi](https://developers.googleblog.com/developers-guide-to-ai-agent-protocols/#2-agent2agent-protocol-a2a) · [A2A JSON-RPC belirtimi](https://a2a-protocol.org/v1.0.1/specification/#9-json-rpc-protocol-binding)
