namespace BlazorAppState.Client.State;

public sealed class AppState : IAppState
{
    public string Message { get; set; } = string.Empty;
    public int Count { get; set; }
    public CounterState Counter { get; set; } = new();
    public DateTime LastStorageSaveTime { get; set; }
}