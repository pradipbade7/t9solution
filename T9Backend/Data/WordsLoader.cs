
using System.Text.RegularExpressions;

namespace T9Backend.Data
{
    public class WordsLoader
    {
        private readonly List<string> _words;
        private static readonly Regex ValidWordPattern = new Regex("^[a-z]+$", RegexOptions.Compiled);

        public WordsLoader(string filePath, ILogger<WordsLoader>? logger = null)
        {
            if (!File.Exists(filePath))
            {
                logger?.LogError("Dictionary file not found: {FilePath}", filePath);
                throw new FileNotFoundException("Dictionary file not found", filePath);
            }

            try
            {
                _words = File.ReadAllLines(filePath)
                    .Select(word => word.ToLowerInvariant().Trim())
                    .Where(word => !string.IsNullOrEmpty(word) && 
                                  word.Length >= 2 && 
                                   word.Length <= 45 &&
                                  ValidWordPattern.IsMatch(word))
                    .ToList();

                logger?.LogInformation("Loaded {Count} valid words from dictionary", _words.Count);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error loading dictionary: {Message}", ex.Message);
                throw;
            }
        }

        public List<string> GetVocabulary() => _words;

        public int Count => _words.Count;
    }
}