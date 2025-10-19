import { useState, useEffect, useRef } from "react";

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
                    localStorage.removeItem("lastRequestId");
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
            const response = await fetch("https://localhost:5001/api/data/request", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ userId })
            });

            if (!response.ok) 
                throw new Error("Failed to request data");

            const { requestId } = await response.json();
            console.log("Request started with ID:", requestId);
            setRequestId(requestId);
            localStorage.setItem("lastRequestId", requestId);
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

    return { loading, result, requestData };
}
