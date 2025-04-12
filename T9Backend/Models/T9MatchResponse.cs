namespace T9Backend.Models
{
    public class T9MatchResponse
    {
        public string? Digits { get; set; }
        public bool IsStrict { get; set; }
        public List<string> Words { get; set; } = new List<string>();
    }
    
}