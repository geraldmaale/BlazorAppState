namespace BlazorAppState.Client.State;

public record CounterState
{
    public int Count { get; set; }
    public string Message { get; set; } = string.Empty;
}
