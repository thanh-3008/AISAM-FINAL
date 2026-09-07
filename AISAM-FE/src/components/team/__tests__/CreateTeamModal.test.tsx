import { describe, expect, it, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import CreateTeamModal from "../CreateTeamModal";
import * as teamService from "@/services/teamService";
import * as brandService from "@/services/brandService";

vi.mock("@/services/brandService", () => ({
  fetchBrands: vi.fn(),
}));

vi.mock("@/services/teamService", async (importOriginal) => {
  const actual = await importOriginal<typeof teamService>();
  return {
    ...actual,
    fetchMembers: vi.fn(),
  };
});

const mockMembers: teamService.TeamMember[] = [
  {
    id: "m-creator",
    name: "Creator User",
    email: "creator@example.com",
    avatar: null,
    role: "ContentCreator",
    status: "Active",
    teamIds: [],
    lastActive: "2026-01-01",
    createdAt: "2026-01-01",
    quotaMode: "SharedPool",
    creditLimit: null,
    creditUsed: 0,
    canViewCredit: true,
  },
  {
    id: "m-owner",
    name: "Owner User",
    email: "owner@example.com",
    avatar: null,
    role: "Owner",
    status: "Active",
    teamIds: [],
    lastActive: "2026-01-01",
    createdAt: "2026-01-01",
    quotaMode: "SharedPool",
    creditLimit: null,
    creditUsed: 0,
    canViewCredit: true,
  },
  {
    id: "m-manager",
    name: "Manager User",
    email: "manager@example.com",
    avatar: null,
    role: "Manager",
    status: "Active",
    teamIds: [],
    lastActive: "2026-01-01",
    createdAt: "2026-01-01",
    quotaMode: "SharedPool",
    creditLimit: null,
    creditUsed: 0,
    canViewCredit: true,
  },
];

describe("CreateTeamModal", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(brandService.fetchBrands).mockResolvedValue([
      { id: "b1", name: "Brand 1" },
    ]);
    vi.mocked(teamService.fetchMembers).mockResolvedValue({
      data: mockMembers,
      total: mockMembers.length,
    });
  });

  it("pre-selects Owner as the first member and displays Mặc định badge", async () => {
    render(
      <CreateTeamModal
        open={true}
        onClose={vi.fn()}
        onCreate={vi.fn()}
        isLoading={false}
      />
    );

    await waitFor(() => {
      expect(screen.getByText("Owner User")).toBeDefined();
    });

    // Verify "Mặc định" badge is displayed
    expect(screen.getByText("Mặc định")).toBeDefined();
  });

  it("does not allow unselecting Owner and submits Owner first", async () => {
    const onCreate = vi.fn();
    render(
      <CreateTeamModal
        open={true}
        onClose={vi.fn()}
        onCreate={onCreate}
        isLoading={false}
      />
    );

    await waitFor(() => {
      expect(screen.getByText("Owner User")).toBeDefined();
    });

    const ownerButton = screen.getByTitle("Owner luôn là thành viên mặc định của team");
    expect(ownerButton).toBeDefined();

    // Click owner button to attempt unselect (should be blocked)
    fireEvent.click(ownerButton);

    // Click another member to select them
    const creatorButton = screen.getByText("Creator User");
    fireEvent.click(creatorButton);

    const nameInput = screen.getByPlaceholderText("e.g., Creative Explorers");
    fireEvent.change(nameInput, { target: { value: "New Alpha Team" } });

    const submitBtn = screen.getByText("Create Team");
    fireEvent.click(submitBtn);

    expect(onCreate).toHaveBeenCalledWith(
      expect.objectContaining({
        name: "New Alpha Team",
        memberIds: expect.arrayContaining(["m-owner", "m-creator"]),
      })
    );
    expect(onCreate.mock.calls[0][0].memberIds[0]).toBe("m-owner");
  });
});
