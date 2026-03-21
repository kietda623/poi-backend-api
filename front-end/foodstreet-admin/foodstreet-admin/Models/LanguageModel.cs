namespace foodstreet_admin.Models;

public class LanguageModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "vi"; // vi, en, kr, jp, cn
    public string Name { get; set; } = "Tiếng Việt";
    public bool IsActive { get; set; } = true;
}
