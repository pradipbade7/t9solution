using System.Text;
using T9Backend.Data;

namespace T9Backend.Services
{
    public class WordService:IWordService
    {
        private readonly WordsLoader _wordsLoader;
        private readonly ILogger<WordService> _logger;
        private readonly Dictionary<char, char> _keypadMap;
        private Dictionary<string, List<string>> _t9Cache;
        private Dictionary<string, List<string>> _t9PrefixCache;
        public WordService(WordsLoader wordsLoader, ILogger<WordService> logger)
        {
            _logger = logger;
            _wordsLoader = wordsLoader;
            _t9Cache = new Dictionary<string, List<string>>();
            _t9PrefixCache = new Dictionary<string, List<string>>();

            // Initialize keypad mapping with lowercase letters only
            _keypadMap = new Dictionary<char, char>
            {
                { 'a', '2' }, { 'b', '2' }, { 'c', '2' },
                { 'd', '3' }, { 'e', '3' }, { 'f', '3' },
                { 'g', '4' }, { 'h', '4' }, { 'i', '4' },
                { 'j', '5' }, { 'k', '5' }, { 'l', '5' },
                { 'm', '6' }, { 'n', '6' }, { 'o', '6' },
                { 'p', '7' }, { 'q', '7' }, { 'r', '7' }, { 's', '7' },
                { 't', '8' }, { 'u', '8' }, { 'v', '8' },
                { 'w', '9' }, { 'x', '9' }, { 'y', '9' }, { 'z', '9' }
            };

            // Pre-compute T9 sequences for all words
            PrecomputeT9Sequences();
        }

        private void PrecomputeT9Sequences()
        {
            var vocabulary = _wordsLoader.GetVocabulary();
            _t9Cache.Clear();
            _t9PrefixCache.Clear();
            // Store strict matches
            _t9Cache = new Dictionary<string, List<string>>();

            // Also create a prefix cache
            _t9PrefixCache = new Dictionary<string, List<string>>();

            foreach (var word in vocabulary)
            {
                string t9Sequence = WordToT9Sequence(word);

                // Store for strict matching
                if (!_t9Cache.TryGetValue(t9Sequence, out var words))
                {
                    words = new List<string>();
                    _t9Cache[t9Sequence] = words;
                }
                words.Add(word);

                // Store prefixes for prefix matching
                for (int i = 1; i <= t9Sequence.Length; i++)
                {
                    string prefix = t9Sequence.Substring(0, i);
                    if (!_t9PrefixCache.TryGetValue(prefix, out var prefixWords))
                    {
                        prefixWords = new List<string>();
                        _t9PrefixCache[prefix] = prefixWords;
                    }
                    prefixWords.Add(word);
                }
            }

            // Sort using case-insensitive comparison
            foreach (var key in _t9Cache.Keys.ToList())
            {
                _t9Cache[key] = _t9Cache[key].OrderBy(w => w, StringComparer.OrdinalIgnoreCase).ToList();
            }

            foreach (var key in _t9PrefixCache.Keys.ToList())
            {
                _t9PrefixCache[key] = _t9PrefixCache[key].OrderBy(w => w, StringComparer.OrdinalIgnoreCase).ToList();
            }
        }
        private string WordToT9Sequence(string word)
        {
            var builder = new StringBuilder(word.Length);
            foreach (char c in word.ToLowerInvariant())
            {
                if (_keypadMap.TryGetValue(c, out char digit))
                {
                    builder.Append(digit);
                }
            }
            return builder.ToString();
        }

        public List<string> Match(string input, bool strict = false)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<string>();
            }

            // For strict matching
            if (strict)
            {
                return _t9Cache.TryGetValue(input, out var matches)
                    ? matches
                    : new List<string>();
            }

            // For prefix matching
            return _t9PrefixCache.TryGetValue(input, out var prefixMatches)
                ? prefixMatches
                : new List<string>();
        }
    
    }
}