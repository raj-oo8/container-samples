public interface IPreviewService
{
    bool IsPreviewEnabled { get; set; }
}

public class PreviewService : IPreviewService
{
    public bool IsPreviewEnabled { get; set; }

    public PreviewService(bool isEnabled)
    {
        IsPreviewEnabled = isEnabled;
    }
}
