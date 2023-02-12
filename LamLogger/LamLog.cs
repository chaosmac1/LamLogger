using System.Runtime.CompilerServices;
using LamLibAllOver;

namespace LamLogger;

public class LamLog: IDisposable {
    private readonly DateUuid _uuid;
    private readonly Queue<LamLogTable> _logs;
    private readonly Option<Func<LamLogTable[], Task>> _dbActionAsync;
    private readonly LamLogSettings _settings;
    
    public LamLog(Option<Func<LamLogTable[], Task>> dbActionAsync) {
        _settings = LamLog.Settings;
        _uuid = DateUuid.NewDateTime;
        _logs = _settings!.LazyDbPrint? new Queue<LamLogTable>(8): new Queue<LamLogTable>(0);
        _dbActionAsync = dbActionAsync;
        _settings = Settings;
        
        if (_settings.UseDbAsPrint) return;
        if (_settings.DbTable is null)
            throw new NullReferenceException("DbTable Not Set In Setting But UseDbAsPrint Is True");
        if (_dbActionAsync.IsSet() == false)
            throw new Exception("DbAction is Not Set But UseDbAsPrint Is True");
    }

    public async Task FlushToDbAsync() {
        var logArr = this._logs.ToArray();
        this._logs.Clear();
        await this._dbActionAsync.Unwrap()(logArr);
    }

    public void Close() => FlushToDbAsync().Wait();
   
    
    public static LamLogSettings Settings = LamLogSettings.Default;
    
    public void Dispose() => Close();
    
    public async Task AddLogAsync(string message, ELamLoggerStatus status, Option<string> stack, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        await (status switch {
            ELamLoggerStatus.Ok => AddLogWithTriggerAsync(message, status, $"(Name: {caller})", stack),
            ELamLoggerStatus.Debug => AddLogWithTriggerAsync(message, status, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", stack),
            ELamLoggerStatus.Error => AddLogWithTriggerAsync(message, status, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", stack),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        });
    }

    public async Task AddLogOkAsync(string message, [CallerMemberName] string caller = null!) {
        await AddLogWithTriggerAsync(message, ELamLoggerStatus.Ok, $"(Name: {caller})", Option<string>.Empty);        
    }

    public async Task AddLogDebugAsync(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        await AddLogWithTriggerAsync(message, ELamLoggerStatus.Debug, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.Empty);
    }

    public async Task AddLogErrorAsync(string message, Option<string> stack = default, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        if (stack.IsSet()) {
            await AddLogWithTriggerAsync(message, ELamLoggerStatus.Error, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", stack);
            return;
        }
        
        System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
        await AddLogWithTriggerAsync(message, ELamLoggerStatus.Error, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.With(t.ToString()));
    }

    public void AddLogDebugStart(
        [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        AddLogWithTriggerAsync("Start", ELamLoggerStatus.Debug, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.Empty).Wait();
    }
    
    public async Task AddLogDebugStartAsync(
        [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        await AddLogWithTriggerAsync("Start", ELamLoggerStatus.Debug, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.Empty);
    }
    
    public void AddLogOkStart(
        [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        AddLogWithTriggerAsync("Start", ELamLoggerStatus.Ok, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.Empty).Wait();
    }
    
    public async Task AddLogOkStartAsync(
        [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        await AddLogWithTriggerAsync("Start", ELamLoggerStatus.Ok, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.Empty);
    }
    
    public void AddLog(string message, ELamLoggerStatus status, Option<string> stack, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        (status switch {
            ELamLoggerStatus.Ok => AddLogWithTriggerAsync(message, status, $"(Name: {caller})", stack),
            ELamLoggerStatus.Debug => AddLogWithTriggerAsync(message, status, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", stack),
            ELamLoggerStatus.Error => AddLogWithTriggerAsync(message, status, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", stack),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        }).Wait();
    }

    public void AddLogOk(string message, [CallerMemberName] string caller = null!) {
        AddLogWithTriggerAsync(message, ELamLoggerStatus.Ok, $"(Name: {caller})", Option<string>.Empty).Wait();        
    }

    public void AddLogDebug(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        AddLogWithTriggerAsync(message, ELamLoggerStatus.Debug, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.Empty).Wait();
    }

    public void AddLogError(string message, Option<string> stack = default, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        if (stack.IsSet()) {
            AddLogWithTriggerAsync(message, ELamLoggerStatus.Error, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", stack).Wait();
            return;
        }
        
        System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
        AddLogWithTriggerAsync(message, ELamLoggerStatus.Error, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.With(t.ToString())).Wait();
    }

    public ResultOk<T> AddResultAndTransform<T>(Result<T, string> result, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        if (result == EResult.Err) {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            AddLogWithTriggerAsync(result.Err(), ELamLoggerStatus.Error, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.With(t.ToString())).Wait();
        }

        return result;
    }
    
    public async Task<ResultOk<T>> AddResultAndTransformAsync<T>(Result<T, string> result, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        if (result == EResult.Err) {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            await AddLogWithTriggerAsync(result.Err(), ELamLoggerStatus.Error, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.With(t.ToString()));
        }

        return result;
    }
    
    public EResult AddResultAndTransform<T>(ResultErr<string> result, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        if (result == EResult.Err) {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            AddLogWithTriggerAsync(result.Err(), ELamLoggerStatus.Error, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.With(t.ToString())).Wait();
            return EResult.Err;
        }

        return EResult.Ok;
    }
    
    public async Task<EResult> AddResultAndTransformAsync<T>(ResultErr<string> result, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null!, [CallerFilePath] string filePath = null!) {
        if (result == EResult.Err) {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            await AddLogWithTriggerAsync(result.Err(), ELamLoggerStatus.Error, $"(Name: {caller}, Line: {lineNumber}, filePath: {filePath})", Option<string>.With(t.ToString()));
            return EResult.Err;
        }

        return EResult.Ok;
    }
    
    private async Task Trigger(LamLogTable table) {
        Print(table);
        
        if (_settings.UseDbAsPrint == false) return;
        if (_settings.LazyDbPrint) {
            _logs.Enqueue(table);
            return;
        }
        
        if ((_settings.PrintDBUseOk == false && table.Status == ELamLoggerStatus.Ok)
            || (_settings.PrintDBUseDebug == false && table.Status == ELamLoggerStatus.Debug)
            || (_settings.PrintDBUseError == false && table.Status == ELamLoggerStatus.Error))
            return;

        await this._dbActionAsync.Unwrap()(new [] { table });
    }
    
    private void Print(LamLogTable log) {
        if ((_settings.PrintUseOk == false && log.Status == ELamLoggerStatus.Ok)
            || (_settings.PrintUseDebug == false && log.Status == ELamLoggerStatus.Debug)
            || (_settings.PrintUseError == false && log.Status == ELamLoggerStatus.Error))
            return;

        var defaultColor = Console.ForegroundColor;
        ConsoleColor logStatusColor = log.Status switch {
            ELamLoggerStatus.Ok => ConsoleColor.Green,
            ELamLoggerStatus.Debug => ConsoleColor.Yellow,
            ELamLoggerStatus.Error => ConsoleColor.DarkRed,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        
        Console.ForegroundColor = logStatusColor;
        Console.Write($"{log.Status}, ");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"Id: { log.DateUuid }, ");
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Message: {log.Message},");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"Trigger: {log.Trigger}");
        
        if (log.Stack.IsSet()) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"\n Stack: {log.Stack}");
        }
        Console.Write('\n');

        Console.ForegroundColor = defaultColor;
    }
    
    private async Task AddLogWithTriggerAsync(string message, ELamLoggerStatus status, string triggerName, Option<string> stack) {
        await Trigger(new LamLogTable(_uuid, _uuid.GetDateTime(), status, triggerName, message, stack));
    }
}