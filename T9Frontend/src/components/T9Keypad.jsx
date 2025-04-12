import { useState, useEffect } from 'react';
import axios from 'axios';
import '../styles/Keypad.css';
function T9Keypad() {
  const [input, setInput] = useState('');
  const [words, setWords] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showKeypad, setShowKeypad] = useState(false);
  const keys = [
    ['1', ''],
    ['2', 'ABC'],
    ['3', 'DEF'],
    ['4', 'GHI'],
    ['5', 'JKL'],
    ['6', 'MNO'],
    ['7', 'PQRS'],
    ['8', 'TUV'],
    ['9', 'WXYZ'],
    ['C', ''],
    ['0', '+'],
    ['←', '']
  ]
  // Handle special key functionality
  const handleKeyPress = (key) => {
    switch(key) {
      case 'C':
        // Clear all input
        setInput('');
        break;
      case '←':
        // Remove last character (backspace)
        setInput(prev => prev.slice(0, -1));
        break;
      default:
        // Add digit to input
        setInput(prev => prev + key);
        break;
    }
  };
  useEffect(() => {
    const fetchWords = async () => {
      if (!input) {
        setWords([]);
        return;
      }

      if (input === '0' || input.startsWith('0')) {
        setError('Input cannot be only 0 or start with 0');
        setWords([]);
        setLoading(false);
        return;
      }

      if ((input.match(/0/g) || []).length > 1) {
        setError('More than one 0 is not allowed');
        setWords([]);
        setLoading(false);
        return;
      }

      if (input.includes('1')) {
        setError('Key 1 is not allowed in T9 input');
        setWords([]);
        setLoading(false);
        return;
      }


      if (!/[2-9]/.test(input)) {
        setError('At least one key between 2-9 is required');
        setWords([]);
        setLoading(false);
        return;
      }

      setLoading(true);
      setError('');

      try {
        const response = await axios.get('/api/words/match', {
          params: {
            digits: input
          }
        });

        setWords(response.data.words || []);
      } catch (err) {
        console.error('Error fetching words:', err);
        setError('Failed to fetch matching words');
      } finally {
        setLoading(false);
      }
    };

    // Debounce API calls while typing
    const timeoutId = setTimeout(fetchWords, 300);
    return () => clearTimeout(timeoutId);
  }, [input]);

  const handleInputChange = (e) => {
    const value = e.target.value;
    // Only allow digits, ensuring input is valid for T9
    setInput(value.replace(/[^0-9]/g, ''));
  };

  return (
    <div className="t9-container">
      <h1>T9 Word Matcher</h1>

      <div className="input-section">
        <label htmlFor="t9input">Enter digits:</label>
        <input
          id="t9input"
          type="text"
          value={input}
          onChange={handleInputChange}
          placeholder="Enter digits (end with 0 for strict matching)"
          className="t9-input"
          maxLength={45}
        />
      </div>

      <div className="toggle-button-wrapper">
        <button
          className={`toggle-keypad ${showKeypad ? 'keypad-showing' : 'keypad-hidden'}`}
          onClick={() => setShowKeypad(prev => !prev)}
        >
          {showKeypad ? 'Hide Keypad' : 'Show Keypad'}
        </button>
      </div>

      {showKeypad && (
        <div className="keypad">
          {keys.map(([key, letters]) => (
            <button
              key={key}
              className={`keypad-button ${key === 'C' ? 'clear-key' : key === '←' ? 'backspace-key' : ''}`}
              onClick={() => handleKeyPress(key)}
              disabled={key === '1'} // Disable button for key '1'
            >
              <div className="key">{key}</div>
              <div className="letters">{letters}</div>
            </button>
          ))}
        </div>
      )}

      <div className="info-box">
        <p>Type digits to see matching words.</p>
        <p>End with 0 for strict matching (e.g., "46630" will match exactly "4663").</p>
      </div>

      <div className="results-section">
        <h2>Matching Words {words.length > 0 ? `(${words.length})` : ''}</h2>

        {loading ? (
          <div className="loading">Loading...</div>
        ) : error ? (
          <div className="error">{error}</div>
        ) : words.length > 0 ? (
          <ul className="word-list">
            {words.map((word, index) => (
              <li key={index}>{word}</li>
            ))}
          </ul>
        ) : input ? (
          <div className="no-results">No matching words found</div>
        ) : (
          <div className="empty-state">Enter digits to find matching words</div>
        )}
      </div>
    </div>
  );
}

export default T9Keypad;