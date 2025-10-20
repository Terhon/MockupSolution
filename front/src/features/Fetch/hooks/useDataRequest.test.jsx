import { renderHook, act } from "@testing-library/react";
import { useDataRequest } from "./useDataRequest";

describe("useDataRequest", () => {
    beforeEach(() => {
        localStorage.clear();
        vi.useFakeTimers();
        global.fetch = vi.fn();
    });

    afterEach(() => {
        vi.clearAllTimers();
        vi.useRealTimers();
    });

    it("starts a request and polls until complete", async () => {
        const userId = "user-1";

        fetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ requestId: "req-1" })
        });

        fetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ status: "Pending" })
        });
        
        fetch.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ status: "Completed", data: { msg: "done" } })
        });

        const { result } = renderHook(() => useDataRequest(userId));

        await act(async () => {
            await result.current.requestData();
        });

        await act(async () => {
            vi.advanceTimersByTime(6000);
        });

        expect(fetch).toHaveBeenCalledWith(
            "https://localhost:5001/api/data/request",
            expect.objectContaining({ method: "POST" })
        );
        expect(result.current.result).toEqual({ msg: "done" });
        expect(result.current.loading).toBe(false);
    });

    it("handles failed request gracefully", async () => {
        fetch.mockResolvedValueOnce({ ok: false });

        const { result } = renderHook(() => useDataRequest("user-2"));

        await act(async () => {
            await result.current.requestData();
        });

        expect(result.current.loading).toBe(false);
    });
});
