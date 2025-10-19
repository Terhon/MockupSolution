import {useCookies} from "react-cookie";
import React, {useEffect, useState, useRef} from "react";


function Fetch() {
    const [cookies, setCookie] = useCookies(['userId'])
    const [requestId, setRequestId] = useState(null);
    const [result, setResult] = useState(null);
    const [loading, setLoading] = useState(false);
    const pollingRef = useRef(null);

    useEffect(() => {
        if (!cookies.userId) {
            const newId = crypto.randomUUID();

            const expires = new Date();
            expires.setTime(expires.getTime() + 60 * 60 * 1000);

            setCookie("userId", newId, {path: "/", expires});
            console.log("New userId cookie set:", newId);
        } else {
            console.log("Existing userId:", cookies.userId);
        }
    }, [cookies, setCookie]);

    const requestData = async () => {
        setLoading(true);
        setResult(null);

        try {
            const response = await fetch("https://localhost:5001/api/data/request", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify({userId: cookies.userId})
            });

            if (!response.ok)
                throw new Error("Failed to request data");

            const {requestId} = await response.json();
            console.log("Request started with ID:", requestId);
            setRequestId(requestId);

            startPolling(requestId);
        } catch (err) {
            console.error("Error:", err);
            setLoading(false);
        }
    };

    const startPolling = (id) => {
        pollingRef.current = setInterval(async () => {
            try {
                const res = await fetch(`https://localhost:5001/api/data/status/${id}`);
                if (!res.ok)
                    throw new Error("Polling failed");

                const json = await res.json();
                console.log("Polling response:", json);

                if (json.status === "Completed") {
                    setResult(json.data);
                    setLoading(false);
                    clearInterval(pollingRef.current);
                    pollingRef.current = null;
                }
            } catch (err) {
                console.error(err);
                clearInterval(pollingRef.current);
                pollingRef.current = null;
                setLoading(false);
            }
        }, 3000);
    };

    useEffect(() => {
        return () => {
            if (pollingRef.current) 
                clearInterval(pollingRef.current);
        };
    }, []);


    return (<div>
        <h5>Welcome user {cookies.userId}</h5>
        <h1>Data</h1>
        <button onClick={requestData} disabled={loading}>
            {loading ? 'Loading...' : 'Request Data'}
        </button>

        {result && (
            <div>
                <h2>Result</h2>
                <pre>{JSON.stringify(result, null, 2)}</pre>
            </div>
        )}
    </div>)
}

export default Fetch;