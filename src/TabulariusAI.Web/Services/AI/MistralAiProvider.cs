using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TabulariusAI.Web.Services.AI;

public sealed class MistralAiProvider(IHttpClientFactory httpClientFactory) : IAiProvider
{
    public string Name => "Mistral";
    public async Task<AiProviderResponse> CompleteAsync(string endpoint,string model,string? apiKey,decimal temperature,string systemPrompt,IReadOnlyList<AiMessage> messages,IReadOnlyList<AiToolDefinition> tools,CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("A API key da Mistral não está configurada.");
        var client=httpClientFactory.CreateClient(); client.BaseAddress=new Uri(endpoint.TrimEnd('/')+"/"); client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",apiKey);
        var providerMessages=new List<object>{new { role="system",content=systemPrompt }};
        foreach(var message in messages)
        {
            if(message.Role=="tool") providerMessages.Add(new { role="tool",name=message.Name,content=message.Content,tool_call_id=message.ToolCallId });
            else if(message.Role=="assistant"&&message.ToolCalls is { Count:>0 }) providerMessages.Add(new { role="assistant",content=string.IsNullOrWhiteSpace(message.Content)?null:message.Content,tool_calls=message.ToolCalls.Select(call=>new { id=call.Id,type="function",function=new { name=call.Name,arguments=call.Arguments.GetRawText() } }).ToArray() });
            else providerMessages.Add(new { role=message.Role,content=message.Content });
        }
        var payload=new { model, temperature, messages=providerMessages, tools=tools.Select(t=>new { type="function", function=new { name=t.Name,description=t.Description,parameters=t.Parameters } }).ToArray(), tool_choice="auto" };
        using var response=await client.PostAsJsonAsync("v1/chat/completions",payload,cancellationToken); var responseBody=await response.Content.ReadAsStringAsync(cancellationToken); if(!response.IsSuccessStatusCode) throw new HttpRequestException($"Mistral devolveu {(int)response.StatusCode} ({response.ReasonPhrase}): {ExtractErrorMessage(responseBody)}",null,response.StatusCode);
        using var document=JsonDocument.Parse(responseBody); var message=document.RootElement.GetProperty("choices")[0].GetProperty("message"); var content=message.TryGetProperty("content",out var c)&&c.ValueKind!=JsonValueKind.Null?c.GetString():null; var calls=new List<AiToolCall>();
        if(message.TryGetProperty("tool_calls",out var toolCalls)&&toolCalls.ValueKind==JsonValueKind.Array) foreach(var call in toolCalls.EnumerateArray()){var function=call.GetProperty("function"); var arguments=function.GetProperty("arguments"); JsonElement args; if(arguments.ValueKind==JsonValueKind.String) args=JsonDocument.Parse(arguments.GetString()??"{}").RootElement.Clone(); else if(arguments.ValueKind==JsonValueKind.Object) args=arguments.Clone(); else args=JsonDocument.Parse("{}").RootElement.Clone(); calls.Add(new AiToolCall(call.TryGetProperty("id",out var id)&&id.ValueKind==JsonValueKind.String?id.GetString()??Guid.NewGuid().ToString("N"):Guid.NewGuid().ToString("N"),function.GetProperty("name").GetString()??string.Empty,args));}
        return new AiProviderResponse(content,calls);
    }

    private static string ExtractErrorMessage(string body)
    {
        try{using var document=JsonDocument.Parse(body);return document.RootElement.TryGetProperty("message",out var message)&&message.ValueKind==JsonValueKind.String?message.GetString()??"Pedido inválido.":"Pedido inválido.";}catch(JsonException){return "Pedido inválido.";}
    }
}
