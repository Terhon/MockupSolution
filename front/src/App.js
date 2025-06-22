import React, { useState, useEffect } from 'react';

const API_BASE = 'http://localhost:5223/api/polling';

const App = () => {
  const [requestId, setRequestId] = useState(null);
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    setResult(null);

    try {
      setRequestId(getClientId());
      const res = await fetch(`${API_BASE}/${requestId}`, {
        method: 'POST',
      });

      if (!res.ok) 
        throw new Error(`Request failed: ${res.status}`);
      //const { requestId } = await res.json();
      //setRequestId(requestId);

      // Step 2: Poll until result is ready
      const pollResult = async () => {
        const response = await fetch(`${API_BASE}/result/${requestId}`);
        if (response.status === 202) {
          setTimeout(pollResult, 1000); // Retry after 1s
        } else if (!response.ok) {
          throw new Error(`Error fetching result: ${response.status}`);
        } else {
          const data = await response.json();
          setResult(data.result);
          setLoading(false);
        }
      };

      pollResult();
    } catch (error) {
      console.error(error);
      setLoading(false);
    }
  };

  useEffect(() => {
    const savedId = localStorage.getItem('clientId');
    if (!savedId) {
      const newId = crypto.randomUUID();
      localStorage.setItem('clientId', newId);
    }
  }, []);

  const getClientId = () => localStorage.getItem('clientId');

  return (
    <div>
      <h1>Fetch Example</h1>
      <button onClick={fetchData} disabled={loading}>
        {loading ? 'Loading...' : 'Request Data'}
      </button>

      {result && (
        <div>
          <h2>Result</h2>
          <pre>{JSON.stringify(result, null, 2)}</pre>
        </div>
      )}
    </div>
  );
};

export default App;
