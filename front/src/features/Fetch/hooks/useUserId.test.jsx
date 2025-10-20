import { renderHook, act } from "@testing-library/react";
import { useUserId } from "./useUserId";
import { CookiesProvider, Cookies } from "react-cookie";

describe("useUserId", () => {
    let cookies;

    beforeEach(() => {
        cookies = new Cookies();
    });

    it("creates a new userId if not set", () => {
        const wrapper = ({ children }) => (
            <CookiesProvider cookies={cookies}>{children}</CookiesProvider>
        );

        const { result } = renderHook(() => useUserId(), { wrapper });

        expect(result.current).toBeDefined();
        expect(result.current).toMatch(
            /^[0-9a-fA-F-]{36}$/ // UUID
        );
    });

    it("reuses existing userId if present", () => {
        const existing = "abc-123";
        cookies.set("userId", existing);

        const wrapper = ({ children }) => (
            <CookiesProvider cookies={cookies}>{children}</CookiesProvider>
        );

        const { result } = renderHook(() => useUserId(), { wrapper });

        expect(result.current).toBe(existing);
    });
});
