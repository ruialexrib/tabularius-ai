using System.Net.Http.Json;
using System.Text.Json;

namespace TabulariusAI.Web.Services.AI;

public sealed class OllamaAiProvider(IHttpClientFactory httpClientFactory) : IAiProvider
{
    public string Name => "Ollama";

    public async Task<AiProviderResponse> CompleteAsync(string endpoint,string model,string? apiKey,decimal temperature,string systemPrompt,IReadOnlyList<AiMessage> messages,IReadOnlyList<AiToolDefinition> tools,CancellationToken cancellationToken)
    {
        var client=httpClientFactory.CreateClient();
        client.BaseAddress=new Uri(endpoint.TrimEnd('/')+"/");

        var providerMessages=new List<object>{new { role="system",content=systemPrompt }};
        foreach(var historyMessage in messages)
        {
            if(historyMessage.Role=="tool")
            {
                providerMessages.Add(new { role="tool",content=historyMessage.Content,tool_name=historyMessage.Name });
            }
            else if(historyMessage.Role=="assistant"&&historyMessage.ToolCalls is { Count:>0 })
            {
                providerMessages.Add(new
                {
                    role="assistant",
                    content=historyMessage.Content,
                    tool_calls=historyMessage.ToolCalls.Select(call=>new
                    {
                        function=new
                        {
                            name=call.Name,
                            arguments=JsonSerializer.Deserialize<object>(call.Arguments.GetRawText())
                        }
                    }).ToArray()
                });
            }
            else
            {
                providerMessages.Add(new { role=historyMessage.Role,content=historyMessage.Content });
            }
        }

        var payload=new
        {
            model,
            stream=false,
            options=new { temperature },
            messages=providerMessages,
            tools=tools.Select(t=>new
            {
                type="function",
                function=new { name=t.Name,description=t.Description,parameters=t.Parameters }
            }).ToArray()
        };

        using var response=await client.PostAsJsonAsync("api/chat",payload,cancellationToken);
        var responseBody=await response.Content.ReadAsStringAsync(cancellationToken);
        if(!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ollama devolveu {(int)response.StatusCode} ({response.ReasonPhrase}): {ExtractErrorMessage(responseBody)}",null,response.StatusCode);

        using var document=JsonDocument.Parse(responseBody);
        var message=document.RootElement.GetProperty("message");
        var content=message.TryGetProperty("content",out var c)&&c.ValueKind!=JsonValueKind.Null?c.GetString():null;
        var calls=new List<AiToolCall>();

        if(message.TryGetProperty("tool_calls",out var toolCalls)&&toolCalls.ValueKind==JsonValueKind.Array)
        {
            foreach(var call in toolCalls.EnumerateArray())
            {
                var function=call.GetProperty("function");
                var arguments=function.TryGetProperty("arguments",out var args)?args:default;
                JsonElement parsedArguments;
                if(arguments.ValueKind==JsonValueKind.Object)
                    parsedArguments=arguments.Clone();
                else if(arguments.ValueKind==JsonValueKind.String)
                    parsedArguments=JsonDocument.Parse(arguments.GetString()??"{}").RootElement.Clone();
                else
                    parsedArguments=JsonDocument.Parse("{}").RootElement.Clone();

                calls.Add(new AiToolCall(Guid.NewGuid().ToString("N"),function.GetProperty("name").GetString()??string.Empty,parsedArguments));
            }
        }

        return new AiProviderResponse(content,calls);
    }

    private static string ExtractErrorMessage(string body)
    {
        try
        {
            using var document=JsonDocument.Parse(body);
            if(document.RootElement.TryGetProperty("error",out var error)&&error.ValueKind==JsonValueKind.String)
                return error.GetString()??"Pedido inválido.";
            if(document.RootElement.TryGetProperty("message",out var message)&&message.ValueKind==JsonValueKind.String)
                return message.GetString()??"Pedido inválido.";
            return "Pedido inválido.";
        }
        catch(JsonException)
        {
            return string.IsNullOrWhiteSpace(body)?"Pedido inválido.":body[..Math.Min(body.Length,500)];
        }
    }
}
