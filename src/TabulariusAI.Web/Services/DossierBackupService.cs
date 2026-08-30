using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TabulariusAI.Web.Data;
using TabulariusAI.Web.Data.Entities;

namespace TabulariusAI.Web.Services;

public interface IDossierBackupService
{
    Task<byte[]> ExportAsync(int dossierId, string applicationVersion, CancellationToken cancellationToken = default);
    Task<DossierRestoreResult> RestoreAsync(Stream json, CancellationToken cancellationToken = default);
}

public sealed record DossierRestoreResult(int DossierId, int Imports, int Records);

public sealed class DossierBackupService(TabulariusDbContext db) : IDossierBackupService
{
    private const string Format = "TabulariusAI.DossierBackup";
    private const int SchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly MethodInfo SetMethod = typeof(DbContext).GetMethods().Single(x => x.Name == nameof(DbContext.Set) && x.IsGenericMethodDefinition && x.GetParameters().Length == 0);

    public async Task<byte[]> ExportAsync(int dossierId, string applicationVersion, CancellationToken cancellationToken = default)
    {
        var dossier = await db.AnalysisDossiers.AsNoTracking().Include(x => x.AccountingEntity).SingleOrDefaultAsync(x => x.Id == dossierId, cancellationToken)
            ?? throw new KeyNotFoundException("Dossier não encontrado.");
        var importIds = await db.SaftImports.AsNoTracking().Where(x => x.DossierId == dossierId).Select(x => x.Id).ToArrayAsync(cancellationToken);
        var tables = new Dictionary<string, List<Dictionary<string, JsonElement>>>(StringComparer.Ordinal);
        foreach (var type in DossierEntityTypes())
        {
            var rows = new List<Dictionary<string, JsonElement>>();
            foreach (var entity in Query(type.ClrType).Cast<object>().Where(entity => BelongsToDossier(type, entity, dossierId, importIds)))
            {
                var entry = db.Entry(entity);
                var row = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
                foreach (var property in type.GetProperties().OrderBy(x => x.Name)) row[property.Name] = JsonSerializer.SerializeToElement(entry.Property(property.Name).CurrentValue, property.ClrType, JsonOptions);
                rows.Add(row);
            }
            tables[Key(type)] = rows;
        }
        var document = new DossierBackupDocument(Format, SchemaVersion, DateTimeOffset.UtcNow, applicationVersion,
            new DossierBackupEntity(dossier.AccountingEntity.Name, dossier.AccountingEntity.TaxRegistrationNumber), tables);
        return JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
    }

    public async Task<DossierRestoreResult> RestoreAsync(Stream json, CancellationToken cancellationToken = default)
    {
        DossierBackupDocument document;
        try { document = await JsonSerializer.DeserializeAsync<DossierBackupDocument>(json, JsonOptions, cancellationToken) ?? throw new InvalidDataException("O ficheiro está vazio."); }
        catch (JsonException ex) { throw new InvalidDataException("O ficheiro não contém um backup JSON válido.", ex); }
        if (document.Format != Format || document.SchemaVersion != SchemaVersion) throw new InvalidDataException("O formato ou a versão deste backup não é suportado.");
        if (string.IsNullOrWhiteSpace(document.Entity.TaxRegistrationNumber)) throw new InvalidDataException("O backup não identifica a entidade contabilística.");
        var types = DossierEntityTypes().ToDictionary(Key, StringComparer.Ordinal);
        foreach (var type in types) if (!document.Tables.ContainsKey(type.Key)) throw new InvalidDataException($"Falta a tabela {type.Key}.");

        var dossierKey = typeof(AnalysisDossier).FullName!;
        if (document.Tables[dossierKey].Count != 1) throw new InvalidDataException("O backup deve conter exatamente um dossier.");
        var sourceDossierId = document.Tables[dossierKey][0][nameof(AnalysisDossier.Id)].Deserialize<int>(JsonOptions);
        var sourceImportIds = document.Tables[typeof(SaftImport).FullName!].Select(x => x[nameof(SaftImport.Id)].Deserialize<int>(JsonOptions)).ToHashSet();
        ValidateRelationships(document, sourceDossierId, sourceImportIds);

        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(cancellationToken) : null;
        try
        {
            var entity = await db.AccountingEntities.SingleOrDefaultAsync(x => x.TaxRegistrationNumber == document.Entity.TaxRegistrationNumber, cancellationToken);
            if (entity is null) { entity = new AccountingEntity { Name = document.Entity.Name, TaxRegistrationNumber = document.Entity.TaxRegistrationNumber }; db.AccountingEntities.Add(entity); await db.SaveChangesAsync(cancellationToken); }
            var fiscalYear = document.Tables[dossierKey][0][nameof(AnalysisDossier.FiscalYear)].Deserialize<int>(JsonOptions);
            if (await db.AnalysisDossiers.AnyAsync(x => x.AccountingEntityId == entity.Id && x.FiscalYear == fiscalYear, cancellationToken)) throw new InvalidDataException($"Já existe um dossier para {entity.Name} no exercício {fiscalYear}. Elimine-o antes de restaurar este backup.");

            var idMaps = new Dictionary<Type, Dictionary<int, int>>();
            var newDossier = new AnalysisDossier { AccountingEntityId = entity.Id, Name = document.Tables[dossierKey][0][nameof(AnalysisDossier.Name)].Deserialize<string>(JsonOptions) ?? $"Exercício {fiscalYear}", FiscalYear = fiscalYear, CreatedAtUtc = document.Tables[dossierKey][0][nameof(AnalysisDossier.CreatedAtUtc)].Deserialize<DateTime>(JsonOptions) };
            db.AnalysisDossiers.Add(newDossier); await db.SaveChangesAsync(cancellationToken); idMaps[typeof(AnalysisDossier)] = new() { [sourceDossierId] = newDossier.Id };

            foreach (var type in InsertOrder(types.Values).Where(x => x.ClrType != typeof(AnalysisDossier)))
            {
                var map = new Dictionary<int, int>();
                foreach (var row in document.Tables[Key(type)])
                {
                    var oldId = row.TryGetValue("Id", out var idValue) ? idValue.Deserialize<int>(JsonOptions) : 0;
                    var instance = Create(type.ClrType); var entry = db.Entry(instance);
                    foreach (var property in type.GetProperties())
                    {
                        if (property.IsPrimaryKey() || !row.TryGetValue(property.Name, out var raw)) continue;
                        object? value = raw.Deserialize(property.ClrType, JsonOptions);
                        if (property.Name == nameof(SaftImport.DossierId) && idMaps[typeof(AnalysisDossier)].TryGetValue((int)value!, out var dossierId)) value = dossierId;
                        else if (property.IsForeignKey()) value = RemapForeignKey(property, value, idMaps);
                        entry.Property(property.Name).CurrentValue = value;
                    }
                    entry.State = EntityState.Added; await db.SaveChangesAsync(cancellationToken);
                    if (oldId != 0) map[oldId] = (int)entry.Property("Id").CurrentValue!;
                }
                idMaps[type.ClrType] = map;
            }
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return new(newDossier.Id, sourceImportIds.Count, document.Tables.Sum(x => x.Value.Count));
        }
        catch { if (transaction is not null) await transaction.RollbackAsync(cancellationToken); throw; }
    }

    private IReadOnlyList<IEntityType> DossierEntityTypes() => db.Model.GetEntityTypes().Where(x => x.ClrType == typeof(AnalysisDossier) || x.ClrType == typeof(SaftImport) || HasPathToImport(x)).OrderBy(Key).ToList();
    private static bool HasPathToImport(IEntityType type) => type.ClrType != null && (type.GetForeignKeys().Any(x => x.PrincipalEntityType.ClrType == typeof(SaftImport)) || type.GetForeignKeys().Any(x => HasPathToImport(x.PrincipalEntityType)));
    private IEnumerable Query(Type type) => (IEnumerable)SetMethod.MakeGenericMethod(type).Invoke(db, null)!;
    private bool BelongsToDossier(IEntityType type, object entity, int dossierId, int[] importIds)
    {
        var entry = db.Entry(entity);
        if (type.ClrType == typeof(AnalysisDossier)) return (int)entry.Property(nameof(AnalysisDossier.Id)).CurrentValue! == dossierId;
        if (type.ClrType == typeof(SaftImport)) return (int)entry.Property(nameof(SaftImport.DossierId)).CurrentValue! == dossierId;
        var direct = type.GetForeignKeys().FirstOrDefault(x => x.PrincipalEntityType.ClrType == typeof(SaftImport));
        if (direct is not null) return importIds.Contains((int)entry.Property(direct.Properties.Single().Name).CurrentValue!);
        foreach (var fk in type.GetForeignKeys())
        {
            var principal = fk.PrincipalEntityType;
            var directImport = principal.GetForeignKeys().FirstOrDefault(x => x.PrincipalEntityType.ClrType == typeof(SaftImport));
            if (directImport is null) continue;
            var principalId = (int)entry.Property(fk.Properties.Single().Name).CurrentValue!;
            var principalEntity = Query(principal.ClrType).Cast<object>().FirstOrDefault(x => (int)db.Entry(x).Property("Id").CurrentValue! == principalId);
            if (principalEntity is not null && importIds.Contains((int)db.Entry(principalEntity).Property(directImport.Properties.Single().Name).CurrentValue!)) return true;
        }
        return false;
    }
    private static object? RemapForeignKey(IProperty property, object? value, Dictionary<Type, Dictionary<int, int>> maps)
    {
        if (value is not int oldId) return value;
        var principal = property.GetContainingForeignKeys().Single().PrincipalEntityType.ClrType;
        return maps.TryGetValue(principal, out var map) && map.TryGetValue(oldId, out var newId) ? newId : value;
    }
    private static IReadOnlyList<IEntityType> InsertOrder(IEnumerable<IEntityType> types)
    {
        var list = types.ToList(); var set = list.ToHashSet(); var depth = new Dictionary<IEntityType, int>();
        int D(IEntityType t, HashSet<IEntityType> path) { if (depth.TryGetValue(t, out var d)) return d; if (!path.Add(t)) return 0; d = t.GetForeignKeys().Where(x => set.Contains(x.PrincipalEntityType)).Select(x => D(x.PrincipalEntityType, path) + 1).DefaultIfEmpty(0).Max(); path.Remove(t); return depth[t] = d; }
        return list.OrderBy(x => D(x, [])).ThenBy(Key).ToList();
    }
    private static string Key(IEntityType type) => type.ClrType.FullName ?? type.Name;
    private static object Create(Type type) { try { return Activator.CreateInstance(type, true) ?? RuntimeHelpers.GetUninitializedObject(type); } catch { return RuntimeHelpers.GetUninitializedObject(type); } }
    private static void ValidateRelationships(DossierBackupDocument doc, int dossierId, HashSet<int> importIds)
    {
        if (doc.Tables[typeof(SaftImport).FullName!].Any(x => x[nameof(SaftImport.DossierId)].Deserialize<int>(JsonOptions) != dossierId)) throw new InvalidDataException("O backup contém importações de outro dossier.");
        if (importIds.Count != doc.Tables[typeof(SaftImport).FullName!].Count) throw new InvalidDataException("O backup contém identificadores de importação duplicados.");
    }
    private sealed record DossierBackupDocument(string Format, int SchemaVersion, DateTimeOffset ExportedAtUtc, string ApplicationVersion, DossierBackupEntity Entity, Dictionary<string, List<Dictionary<string, JsonElement>>> Tables);
    private sealed record DossierBackupEntity(string Name, string TaxRegistrationNumber);
}
