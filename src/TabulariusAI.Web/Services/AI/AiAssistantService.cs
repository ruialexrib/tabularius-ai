using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TabulariusAI.Web.Data;

namespace TabulariusAI.Web.Services.AI;

public sealed class AiAssistantService(TabulariusDbContext dbContext,IEnumerable<IAiProvider> providers,IEnumerable<IAiTool> tools) : IAiAssistantService
{
    private const string AccountingLocaleRules="\n\nRegras obrigatórias do Tabularius AI: responde em português europeu e usa terminologia contabilística e fiscal portuguesa. Nunca escolhas ou infiras uma moeda. Usa exclusivamente a moeda indicada nos resultados das tools. Quando a tool indicar CurrencyCode=EUR, apresenta os montantes em euros (€) e nunca em reais, R$, dólares ou outra moeda. Respeita as designações de documentos fornecidas pelas tools e não traduzas SalesInvoices como 'notas de venda'.";

    public async Task<string> AskAsync(int dossierId,string question,IReadOnlyList<AiMessage>? history,CancellationToken cancellationToken)
    {
        var settings=await dbContext.AiSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("As definições de inteligência artificial ainda não foram configuradas.");
        if(!settings.IsEnabled) throw new InvalidOperationException("O assistente AI está desativado.");
        var provider=providers.SingleOrDefault(item=>string.Equals(item.Name,settings.Provider,StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Provider AI '{settings.Provider}' não suportado.");
        var toolList=tools.ToArray();
        var messages=(history??[]).Where(item=>item.Role is "user" or "assistant").TakeLast(12).Select(item=>new AiMessage(item.Role,item.Content)).ToList();
        messages.Add(new("user",question));
        var effectiveSystemPrompt=settings.SystemPrompt.TrimEnd()+AccountingLocaleRules;
        for(var round=0;round<6;round++)
        {
            var result=await provider.CompleteAsync(settings.Endpoint,settings.Model,settings.ApiKey,settings.Temperature,effectiveSystemPrompt,messages,toolList.Select(item=>item.Definition).ToArray(),cancellationToken);
            if(result.ToolCalls.Count==0) return string.IsNullOrWhiteSpace(result.Content)?"O modelo não devolveu uma resposta.":result.Content;
            messages.Add(new("assistant",result.Content??string.Empty,ToolCalls:result.ToolCalls));
            foreach(var call in result.ToolCalls)
            {
                var tool=toolList.SingleOrDefault(item=>item.Definition.Name==call.Name) ?? throw new InvalidOperationException($"O modelo tentou utilizar uma tool não autorizada: {call.Name}.");
                var output=await tool.ExecuteAsync(dossierId,call.Arguments,cancellationToken);
                messages.Add(new("tool",JsonSerializer.Serialize(output),call.Id,call.Name));
            }
        }
        throw new InvalidOperationException("O modelo excedeu o número máximo de chamadas de tools permitido para uma pergunta.");
    }
}
