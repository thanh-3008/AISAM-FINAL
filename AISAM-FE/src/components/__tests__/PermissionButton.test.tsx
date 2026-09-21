import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import PermissionButton from "@/components/ui/PermissionButton";
import { ToastProvider } from "@/contexts/ToastContext";

afterEach(cleanup);

it("explains a denied action without running it", () => {
  const action = vi.fn();
  render(<ToastProvider><PermissionButton allowed={false} onClick={action}>Delete</PermissionButton></ToastProvider>);
  fireEvent.click(screen.getByRole("button", { name: "Delete" }));
  expect(action).not.toHaveBeenCalled();
  expect(screen.getByText("Không đủ quyền hạn")).toBeTruthy();
  expect(screen.getByText("Bạn không đủ quyền để thực hiện thao tác này.")).toBeTruthy();
});

it("runs an allowed action and keeps operational disabled state", () => {
  const action = vi.fn();
  const { rerender } = render(<ToastProvider><PermissionButton allowed onClick={action}>Edit</PermissionButton></ToastProvider>);
  fireEvent.click(screen.getByRole("button", { name: "Edit" }));
  expect(action).toHaveBeenCalledOnce();
  rerender(<ToastProvider><PermissionButton allowed disabled onClick={action}>Edit</PermissionButton></ToastProvider>);
  expect((screen.getByRole("button", { name: "Edit" }) as HTMLButtonElement).disabled).toBe(true);
});
