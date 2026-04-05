namespace AppUser.Models
{
    public class AppLanguageDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool HasAudio { get; set; }
    }
}
