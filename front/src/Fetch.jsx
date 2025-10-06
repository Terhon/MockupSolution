import {useCookies} from "react-cookie";
import React, {useState} from "react";


function Fetch() {
    const [cookies, setCookie] = useCookies(['user'])
    const [requestId, setRequestId] = useState(null);
    const [result, setResult] = useState(null);
    const [loading, setLoading] = useState(false);
    
    setCookie('user', 'test')
    
    return (<div>
        <p>Cookie: {cookies.user}</p>
        
        <h1>Data</h1>
        <button /*onClick={fetchData} disabled={loading}*/>
            {loading ? 'Loading...' : 'Request Data'}
        </button>

        {result && (
            <div>
                <h2>Result</h2>
                <pre>{JSON.stringify(result)}</pre>
            </div>
        )}
    </div>)
}

export default Fetch;