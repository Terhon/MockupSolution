import React from "react";

export function DataDisplay({ userId, loading, result, onRequest }) {
    return (
        <div>
            <h5>Welcome user {userId}</h5>
            <h1>Data</h1>

            <button onClick={onRequest} disabled={loading}>
                {loading ? "Loading..." : "Request Data"}
            </button>

            {result && (
                <div>
                    <h2>Result</h2>
                    <pre>{JSON.stringify(result, null, 2)}</pre>
                </div>
            )}
        </div>
    );
}
