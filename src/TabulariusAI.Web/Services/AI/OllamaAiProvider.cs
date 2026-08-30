using System.Net.Http.Json;
using System.Text.Json;

namespace TabulariusAI.Web.Services.AI;

public sealed class OllamaAiProvider(IHttpClientFactory httpClientFactory) : IAiProvider
{
    public string Name => "Ollama";
    public async Task<AiProviderResponse> CompleteAsync(string endpoint,string model,string? apiKey,decimal temperature,string systemPrompt,IReadOnlyList<AiMessage> messages,IReadOnlyList<AiToolDefinition> tools,CancellationToken cancellationToken)
    {
        var client=httpClientFactory.CreateClient(); client.BaseAddress=new Uri(endpoint.TrimEnd('/')+"/");
        var payload=new { model, stream=false, options=new { temperature }, messages=new[]{new { role="system",content=systemPrompt }}.Concat(messages.Select(m=>new { role=m.Role,content=m.Content })).ToArray(), tools=tools.Select(t=>new { type="function", function=new { name=t.Name,description=t.Description,parameters=t.Parameters } }).ToArray() };
        using var response=await client.PostAsJsonAsync("api/chat",payload,cancellationToken); response.EnsureSuccessStatusCode(); using var document=JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken)); var message=document.RootElement.GetProperty("message"); var content=message.TryGetProperty("content",out var c)?c.GetString():null; var calls=new List<AiToolCall>();
        if(message.TryGetProperty("tool_calls",out var toolCalls)) foreach(var call in toolCalls.EnumerateArray()){var function=call.GetProperty("function"); calls.Add(new AiToolCall(Guid.NewGuid().ToString("N"),function.GetProperty("name").GetString()??string.Empty,function.GetProperty("arguments").Clone()));}
        return new AiProviderResponse(content,calls);
    }
}
