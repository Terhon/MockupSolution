import { describe, it, expect, vi, beforeEach } from "vitest";
import { render } from "@testing-library/react";
import React from "react";
import Fetch from "./Fetch";

vi.mock("react-cookie", () => {
    return {
        useCookies: vi.fn(),
    };
});

import { useCookies } from "react-cookie";

describe("Fetch component", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });
    
    it("sets a new cookie when none exists", () => {
        const setCookie = vi.fn();

        useCookies.mockReturnValue([{}, setCookie]);

        render(<Fetch />);

        expect(setCookie).toHaveBeenCalledTimes(1);
        expect(setCookie).toHaveBeenCalledWith(
            "userId",
            expect.any(String),
            expect.objectContaining({
                path: "/",
                expires: expect.any(Date),
            })
        );
    });

    it("does not set cookie if one already exists", () => {
        const setCookie = vi.fn();

        useCookies.mockReturnValue([
            { userId: "abc-123" },
            setCookie,
        ]);

        render(<Fetch />);

        expect(setCookie).not.toHaveBeenCalled();
    });
});
