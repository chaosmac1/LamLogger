namespace LamLogger; 

public sealed class LamLogSettings {
    public required bool PrintUseOk     { get; init; }
    public required bool PrintUseDebug  { get; init; }
    public required bool PrintUseError  { get; init; }
    public required bool PrintDBUseOk     { get; init; }
    public required bool PrintDBUseDebug  { get; init; }
    public required bool PrintDBUseError  { get; init; }
    
    public required bool UseDbAsPrint { get; init; }
    public required string? DbTable { get; init; }
    public required bool LazyDbPrint { get; init; }
    public required bool LazyTextWriterPrint { get; init; }

    public static LamLogSettings Default => new LamLogSettings() {
        PrintUseOk = true,
        PrintUseDebug = true,
        PrintUseError = true,
        PrintDBUseOk = false,
        PrintDBUseDebug = false,
        PrintDBUseError = false,
        UseDbAsPrint = false,
        LazyDbPrint = false,
        LazyTextWriterPrint = false,
        DbTable = null,
    };
}