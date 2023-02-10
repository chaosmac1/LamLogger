using System.Text;
using LamLibAllOver;

namespace LamLogger;

public record struct LamLogTable(
    DateUuid DateUuid,
    DateTime DateTime,
    ELamLoggerStatus Status,
    string Trigger,
    string Message,
    Option<string> Stack
) {
    public bool Equals(LamLogTable? other) => other.HasValue && DateUuid == other.Value.DateUuid;
}