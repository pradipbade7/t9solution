import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import T9Keypad from './components/T9Keypad';
import NotFound from './components/NotFound';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<T9Keypad />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;