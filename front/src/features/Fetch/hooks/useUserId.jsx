import { useCookies } from "react-cookie";
import { useEffect } from "react";

export function useUserId() {
    const [cookies, setCookie] = useCookies(["userId"]);
    const userId = cookies.userId;

    useEffect(() => {
        if (!userId) {
            const newId = crypto.randomUUID();
            const expires = new Date(Date.now() + 60 * 60 * 1000);
            
            setCookie("userId", newId, { path: "/", expires });
            console.log("New userId cookie set:", newId);
        } else {
            console.log("Existing userId:", userId);
        }
    }, [userId, setCookie]);

    return userId;
}
