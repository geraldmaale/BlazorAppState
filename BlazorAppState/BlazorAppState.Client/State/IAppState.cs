namespace BlazorAppState.Client.State;

public interface IAppState
{
    string Message { get; set; }

    int Count { get; set; }
    CounterState Counter { get; set; }

    DateTime LastStorageSaveTime { get; set; }
}
