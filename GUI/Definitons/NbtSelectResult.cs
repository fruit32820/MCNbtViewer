namespace McNbtViewerGUI;

public class NbtSelectResult
{
    public string? Path { get; set; }
    public string? Version { get; set; }
    public bool IsCancelled => string.IsNullOrEmpty(Path);
}
