using MobilDwg.Core.Diagnostics;
using MobilDwg.Core.Documents;
using MobilDwg.Core.Reading;

var handle = new FakeHandle();
var session = new CadDocumentSession(
    handle,
    new CadDocumentMetadata(CadFormat.Dwg, "AC1032", "fixture.dwg"),
    [new CadDiagnostic("parser.notice", DiagnosticSeverity.Info, "notice")],
    [new CadCompatibilityIssue(CompatibilityIssueKind.MissingFont, "missing-font", "font fallback")]);

Assert(!session.IsDisposed, "new session must be live");
Assert(session.Handle == handle, "session must expose the owned abstract handle");
Assert(session.Diagnostics.Count == 1, "diagnostics must be retained");
Assert(session.CompatibilityIssues.Count == 1, "compatibility issues must be retained");

await session.DisposeAsync();
await session.DisposeAsync();

Assert(session.IsDisposed, "session must be disposed");
Assert(handle.DisposeCount == 1, "owned handle must be disposed exactly once");

var disposedThrows = false;
try
{
    _ = session.Handle;
}
catch (ObjectDisposedException)
{
    disposedThrows = true;
}

Assert(disposedThrows, "disposed session must not expose the parser handle");

var capabilities = new CadReaderCapabilities(
    CancellationSupport.BeforeStartOnly,
    ProgressSupport.StagesOnly);
Assert(capabilities.Cancellation != CancellationSupport.Cooperative,
    "BeforeStartOnly must not claim cooperative parser cancellation");
Assert(capabilities.Progress != ProgressSupport.Fractional,
    "StagesOnly must not claim fractional progress");

_ = new CadReadProgress(CadReadStage.Parsing, null, "fraction unknown");

var invalidFractionThrows = false;
try
{
    _ = new CadReadProgress(CadReadStage.Parsing, 1.1);
}
catch (ArgumentOutOfRangeException)
{
    invalidFractionThrows = true;
}

Assert(invalidFractionThrows, "invalid fractional progress must be rejected");

Console.WriteLine("STAGE04_CORE_CONTRACT_TESTS_PASS");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

file sealed class FakeHandle : ICadDocumentHandle
{
    public int DisposeCount { get; private set; }

    public ValueTask DisposeAsync()
    {
        DisposeCount++;
        return ValueTask.CompletedTask;
    }
}
