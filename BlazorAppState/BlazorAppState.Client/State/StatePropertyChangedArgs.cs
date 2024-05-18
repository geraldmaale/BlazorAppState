namespace BlazorAppState.Client.State;

/// <summary>
/// Event args for when a property in the state changes.
/// </summary>
/// <param name="PropertyName"></param>
/// <param name="NewValue"></param>
public record StatePropertyChangedArgs(string PropertyName, object? NewValue);
