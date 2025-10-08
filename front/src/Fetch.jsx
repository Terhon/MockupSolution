import {useCookies} from "react-cookie";
import React, {useEffect, useState} from "react";


function Fetch() {
    const [cookies, setCookie] = useCookies(['userId'])
    const [requestId, setRequestId] = useState(null);
    const [result, setResult] = useState(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (!cookies.userId) {
            const newId = crypto.randomUUID();
            
            const expires = new Date();
            expires.setTime(expires.getTime() + 60 * 60 * 1000);
            
            setCookie("userId", newId, { path: "/", expires });
            console.log("New userId cookie set:", newId);
        } else {
            console.log("Existing userId:", cookies.userId);
        }
    }, [cookies, setCookie]);
    
    return (<div>
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