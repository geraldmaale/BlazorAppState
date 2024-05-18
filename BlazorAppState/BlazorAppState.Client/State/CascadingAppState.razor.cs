using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace BlazorAppState.Client.State;

public partial class CascadingAppState : ComponentBase, IAppState
{
    private readonly string _storageKey = "AppStateKey";
    private readonly int _storageTimeoutInSeconds = 60;
    private bool _isLoaded = false;

    public DateTime LastStorageSaveTime { get; set; }

    [Inject] private ILocalStorageService LocalStorageService { get; set; } = default!;

    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    #region Property Changed
    private List<EventCallback<StatePropertyChangedArgs>> _callbacks = [];

    public void RegisterCallback(EventCallback<StatePropertyChangedArgs> callback)
    {
        // Only add the callback if it's not already registered
        if (!_callbacks.Contains(callback))
        {
            _callbacks.Add(callback);
        }
    }

    public void NotifyPropertyChanged(StatePropertyChangedArgs args)
    {
        foreach (var callback in _callbacks)
        {
            // Ignore exceptions thrown by the callback
            // This is important because we don't want to break the app
            try
            {
                callback.InvokeAsync(args);
            }
            catch (Exception)
            {
                Console.WriteLine("Error invoking callback");
            }
        }
    }
    #endregion

    private string _message = string.Empty;

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            StateHasChanged();

            NotifyPropertyChanged(new("Message", value));

            //Save the state to local storage
            Save();
        }
    }

    private void Save()
    {
        new Task(async () =>
        {
            if (_isLoaded)
            {
                await SaveStateToLocalStorage();
            }
        }).Start();
    }

    private int _count = 0;

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            StateHasChanged();
            NotifyPropertyChanged(new("Count", value));

            // Save the state to local storage
            Save();
        }
    }

    private CounterState _counter = new();
    public CounterState Counter
    {
        get => _counter;
        set
        {
            _counter = value;
            StateHasChanged();
            NotifyPropertyChanged(new("Counter", value));

            // Save the state to local storage
            Save();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load the state from local storage
            await LoadStateFromLocalStorage();
            _isLoaded = true;
            StateHasChanged();
        }
    }

    private async Task LoadStateFromLocalStorage()
    {
        try
        {
            var json = await LocalStorageService.GetItemAsStringAsync(_storageKey);
            if (string.IsNullOrEmpty(json)) return;

            var state = JsonSerializer.Deserialize<AppState>(json)!;

            // Only load the state if it was saved within the last 30 seconds
            var current = TimeProvider.System.GetLocalNow()
                .Subtract(state.LastStorageSaveTime)
                .TotalSeconds;

            if (current <= _storageTimeoutInSeconds)
            {
                // Set the state properties
                var type = typeof(IAppState);
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    if (property.Name != nameof(IAppState.LastStorageSaveTime))
                    {
                        property.SetValue(this, property.GetValue(state), null);
                    }
                }
            }
        }
        catch (Exception)
        {
            throw new("Error loading state from local storage");
        }
    }

    private async Task SaveStateToLocalStorage()
    {
        if (!_isLoaded) return; // Don't save the state if it hasn't been loaded yet (i.e. the app just started

        // Set the last save time to now
        LastStorageSaveTime = DateTime.Now;

        // Save the state to local storage
        var state = (IAppState)this;
        await LocalStorageService.SetItemAsync(_storageKey, state);
    }
}