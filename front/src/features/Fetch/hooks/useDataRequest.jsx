import {useState, useEffect, useRef} from "react";

export function useDataRequest(userId) {
    const [requestId, setRequestId] = useState(localStorage.getItem("lastRequestId"));
    const [result, setResult] = useState(null);
    const [loading, setLoading] = useState(false);
    const pollingRef = useRef(null);

    const startPolling = (id) => {
        if (pollingRef.current) clearInterval(pollingRef.current);
        setLoading(true);

        pollingRef.current = setInterval(async () => {
            try {
                const res = await fetch(`http://localhost:5223/api/Polling/${id}`);
                if (!res.ok)
                    throw new Error("Polling failed");

                if (res.status === 200) {
                    setLoading(false);
                    const result = await res.json();
                    console.log("Request result:", result.data);
                    setResult(result.data);
                    clearInterval(pollingRef.current);
                    pollingRef.current = null;
                    setLoading(false);
                }
            } catch (err) {
                console.error(err);
                clearInterval(pollingRef.current);
                pollingRef.current = null;
                setLoading(false);
            }
        }, 3000);
    };

    const requestData = async () => {
        setLoading(true);
        setResult(null);

        try {
            const response = await fetch(`http://localhost:5223/api/Polling/${userId}`);

            if (!response.ok)
                throw new Error("Failed to request data");
            if (response.status === 200) {
                setLoading(false);
                const result = await response.json();
                console.log("Request result:", result.data);
                setResult(result.data);
            } else if (response.status === 202)
                startPolling(requestId);
        } catch (err) {
            console.error("Error:", err);
            setLoading(false);
        }
    };

    useEffect(() => {
        if (requestId && !result) {
            console.log("Resuming polling for request:", requestId);
            startPolling(requestId);
        }
        return () => {
            if (pollingRef.current) clearInterval(pollingRef.current);
        };
    }, []);

    return {loading, result, requestData};
}
