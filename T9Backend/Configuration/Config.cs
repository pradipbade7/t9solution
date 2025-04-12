namespace T9Backend.Configuration
{
    public class AppSettings
    {
        public T9Settings T9 { get; set; } = new T9Settings();
        public RateLimitSettings RateLimit { get; set; } = new RateLimitSettings();

    }

    public class T9Settings
    {
        public string DictionaryPath { get; set; } = "Data/words.txt";
    }
    public class RateLimitSettings
    {
        public int RequestsPerWindow { get; set; } = 100;
        public int WindowSizeInSeconds { get; set; } = 60;
        public bool Enabled { get; set; } = true;
    }
}
