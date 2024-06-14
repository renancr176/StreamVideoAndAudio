namespace StreamApi.Models;

public class PlayListModel
{
    public string Name { get; set; }
    public FileModel File { get; set; }
    public IEnumerable<string> TsFiles { get; set; }
}