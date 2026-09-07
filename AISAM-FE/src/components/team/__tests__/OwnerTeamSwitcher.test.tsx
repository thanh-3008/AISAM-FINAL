import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import OwnerTeamSwitcher, { TeamSwitcher } from "../OwnerTeamSwitcher";
import { storeActiveTeam, getStoredActiveTeam } from "@/stores/team-store";
import type { Team } from "@/services/teamService";

const mockTeams: Team[] = [
  {
    id: "11111111-1111-1111-1111-111111111111",
    name: "Engineering Team",
    description: "Core eng",
    brandCount: 2,
    brandIds: ["b1", "b2"],
    memberIds: ["u1", "u2"],
    activity: 80,
    createdAt: "2026-01-01T00:00:00Z",
    updatedAt: "2026-01-01T00:00:00Z",
  },
  {
    id: "22222222-2222-2222-2222-222222222222",
    name: "Marketing Team",
    description: "Growth & ads",
    brandCount: 1,
    brandIds: ["b1"],
    memberIds: ["u3"],
    activity: 60,
    createdAt: "2026-01-01T00:00:00Z",
    updatedAt: "2026-01-01T00:00:00Z",
  },
];

describe("OwnerTeamSwitcher / TeamSwitcher", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("renders null when teams list is empty", () => {
    const { container } = render(
      <OwnerTeamSwitcher
        workspaceId="ws-1"
        isOwner={false}
        teams={[]}
      />
    );
    expect(container.firstChild).toBeNull();
  });

  it("renders team switcher button for Owner", () => {
    render(
      <OwnerTeamSwitcher
        workspaceId="ws-1"
        isOwner={true}
        role="Owner"
        teams={mockTeams}
      />
    );

    expect(screen.getByText("Owner")).toBeDefined();
    expect(screen.getByText("Engineering Team")).toBeDefined();
  });

  it("renders team switcher button for Manager", () => {
    render(
      <OwnerTeamSwitcher
        workspaceId="ws-1"
        isOwner={false}
        role="Manager"
        teams={mockTeams}
      />
    );

    expect(screen.getByText("Manager")).toBeDefined();
    expect(screen.getByText("Engineering Team")).toBeDefined();
  });

  it("renders team switcher button for ContentCreator", () => {
    render(
      <OwnerTeamSwitcher
        workspaceId="ws-1"
        isOwner={false}
        role="ContentCreator"
        teams={mockTeams}
      />
    );

    expect(screen.getByText("Creator")).toBeDefined();
    expect(screen.getByText("Engineering Team")).toBeDefined();
  });

  it("renders team switcher button for Viewer", () => {
    render(
      <OwnerTeamSwitcher
        workspaceId="ws-1"
        isOwner={false}
        role="Viewer"
        teams={mockTeams}
      />
    );

    expect(screen.getByText("Viewer")).toBeDefined();
    expect(screen.getByText("Engineering Team")).toBeDefined();
  });

  it("allows non-owner to open dropdown and switch active team", () => {
    const onSwitched = vi.fn();
    render(
      <TeamSwitcher
        workspaceId="ws-1"
        isOwner={false}
        role="Manager"
        teams={mockTeams}
        onTeamSwitched={onSwitched}
      />
    );

    // Open dropdown
    const trigger = screen.getByTitle("Chuyển đổi Team đang làm việc");
    fireEvent.click(trigger);

    // Check dropdown contents
    expect(screen.getByText("Chuyển đổi Team")).toBeDefined();
    expect(screen.getByText("Marketing Team")).toBeDefined();

    // Click Marketing Team
    fireEvent.click(screen.getByText("Marketing Team"));

    // Verify team was switched in store
    const stored = getStoredActiveTeam("ws-1");
    expect(stored?.id).toBe("22222222-2222-2222-2222-222222222222");
    expect(stored?.name).toBe("Marketing Team");
    expect(onSwitched).toHaveBeenCalledWith({
      id: "22222222-2222-2222-2222-222222222222",
      name: "Marketing Team",
      workspaceId: "ws-1",
    });
  });
});
