import React from "react";
import { useUserId } from "./hooks/useUserId";
import { useDataRequest } from "./hooks/useDataRequest";
import { DataDisplay } from "./components/DataDisplay";

function Fetch() {
    const userId = useUserId();
    const { loading, result, requestData } = useDataRequest(userId);

    return (
        <DataDisplay
            userId={userId}
            loading={loading}
            result={result}
            onRequest={requestData}
        />
    );
}

export default Fetch;
