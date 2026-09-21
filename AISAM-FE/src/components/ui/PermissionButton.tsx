"use client";

import { forwardRef, type ButtonHTMLAttributes, type MouseEvent } from "react";
import { useToast } from "@/contexts/ToastContext";

type PermissionButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  allowed: boolean;
  deniedMessage?: string;
};

const DEFAULT_MESSAGE = "Bạn không đủ quyền để thực hiện thao tác này.";

const PermissionButton = forwardRef<HTMLButtonElement, PermissionButtonProps>(function PermissionButton(
  { allowed, deniedMessage = DEFAULT_MESSAGE, disabled, onClick, className = "", title, ...props },
  ref,
) {
  const { showToast } = useToast();

  const handleClick = (event: MouseEvent<HTMLButtonElement>) => {
    if (!allowed) {
      event.preventDefault();
      event.stopPropagation();
      showToast({ type: "warning", title: "Không đủ quyền hạn", message: deniedMessage });
      return;
    }
    onClick?.(event);
  };

  return <button
    {...props}
    ref={ref}
    disabled={allowed ? disabled : false}
    aria-disabled={!allowed || disabled || undefined}
    data-permission-denied={!allowed || undefined}
    onClick={handleClick}
    title={!allowed ? deniedMessage : title}
    className={`${className} ${!allowed ? "cursor-not-allowed opacity-50" : ""}`.trim()}
  />;
});

export default PermissionButton;
