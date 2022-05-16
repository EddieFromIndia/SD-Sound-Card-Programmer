namespace SD_Sound_Card_Programmer.Models;

public class Sound
{
    public string Name { get; set; } = default!;
    public string Path { get; set; } = default!;
    public string ThumbnailSource { get; set; } = default!;
    public long Size { get; set; } = 0;
    public bool IsSelected { get; set; } = false;
}
