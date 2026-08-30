using System.Text.Json;

namespace TabulariusAI.Web.Services.AI;

public sealed record AiMessage(string Role, string Content, string? ToolCallId = null, string? Name = null);
public sealed record AiToolCall(string Id, string Name, JsonElement Arguments);
public sealed record AiProviderResponse(string? Content, IReadOnlyList<AiToolCall> ToolCalls);
public sealed record AiToolDefinition(string Name, string Description, object Parameters);

public interface IAiProvider
{
    string Name { get; }
    Task<AiProviderResponse> CompleteAsync(string endpoint, string model, string? apiKey, decimal temperature, string systemPrompt, IReadOnlyList<AiMessage> messages, IReadOnlyList<AiToolDefinition> tools, CancellationToken cancellationToken);
}

public interface IAiTool
{
    AiToolDefinition Definition { get; }
    Task<object> ExecuteAsync(int dossierId, JsonElement arguments, CancellationToken cancellationToken);
}

public interface IAiAssistantService
{
    Task<string> AskAsync(int dossierId, string question, CancellationToken cancellationToken);
}
