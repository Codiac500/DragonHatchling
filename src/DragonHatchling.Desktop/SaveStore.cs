using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonHatchling.Desktop;

// Position is the window origin in physical virtual-desktop pixels, including negative coordinates.
public sealed record SaveData(int SchemaVersion, PetStage Stage, double X, double Y, bool AlwaysOnTop);
public sealed record SaveLoadResult(SaveData? Data, string? Message);

public sealed class SaveStore(string path)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true,
        Converters = { new JsonStringEnumConverter<PetStage>(allowIntegerValues: false) }
    };
    private bool loaded;
    private bool protectPrimary;
    private bool protectBackup;

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DragonHatchling", "save.json");

    public SaveLoadResult Load()
    {
        loaded = true;
        protectPrimary = protectBackup = false;
        var primary = Read(path);
        var backup = Read(path + ".bak");
        protectBackup = backup.Exists && backup.Data is null;
        if (primary.Data is not null)
            return new(primary.Data, protectBackup ? "Backup unreadable; save paused." : null);
        protectPrimary = primary.Exists;
        if (backup.Data is not null)
            return new(backup.Data, "Save recovered from backup.");
        return new(null, primary.Exists || backup.Exists ? "Save unreadable; using an egg." : null);
    }

    public string? Save(SaveData data)
    {
        if (!loaded) Load();
        if (!Valid(data)) return "Progress could not be saved.";
        // Keep suspect/newer files at their original paths. Never replace a valid backup with them.
        if (protectPrimary || protectBackup)
            return "Save paused; recovery needed.";
        string? temporary = null;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, data, Options);
                stream.Flush(flushToDisk: true);
            }
            // Revalidate before replacement, including files changed after launch.
            var current = Read(path);
            if (current.Exists && current.Data is null)
            {
                protectPrimary = true;
                return "Save paused; recovery needed.";
            }
            var backup = Read(path + ".bak");
            if (backup.Exists && backup.Data is null)
            {
                protectBackup = true;
                return "Save paused; recovery needed.";
            }
            if (current.Exists) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
            return null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        { return "Progress could not be saved."; }
        finally
        {
            if (temporary is not null)
                try { File.Delete(temporary); }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        }
    }

    private static bool Valid(SaveData data) => data.SchemaVersion == 1 && Enum.IsDefined(data.Stage)
        && double.IsFinite(data.X) && double.IsFinite(data.Y)
        && Math.Abs(data.X) <= 1_000_000 && Math.Abs(data.Y) <= 1_000_000;

    private static (SaveData? Data, bool Exists) Read(string file)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            var root = document.RootElement;
            // Require every field; constructor defaults must not turn partial JSON into a valid egg.
            foreach (var name in new[] { "schemaVersion", "stage", "x", "y", "alwaysOnTop" })
                if (!root.TryGetProperty(name, out _)) return (null, true);
            var data = root.Deserialize<SaveData>(Options);
            return (data is not null && Valid(data) ? data : null, true);
        }
        catch (FileNotFoundException) { return (null, false); }
        catch (DirectoryNotFoundException) { return (null, false); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        { return (null, true); }
    }
}
