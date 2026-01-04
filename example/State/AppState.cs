namespace example.State;

public abstract class AppState{
    public static AppStateService Global { get; } = new AppStateService();
}