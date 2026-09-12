import React from "react";
import { afterEach,expect,it,vi } from "vitest";
import { cleanup,fireEvent,render,screen,waitFor } from "@testing-library/react";
import Page from "@/app/(dashboard)/team/performance/page";
import {apiClient} from "@/lib/apiClient";
vi.mock("@/lib/apiClient",()=>({apiClient:vi.fn()}));
vi.mock("next/link",()=>({default:({children}:{children:React.ReactNode})=><span>{children}</span>}));
afterEach(()=>{cleanup();vi.clearAllMocks();});
it("shows missing insight values instead of fabricated zero and sends scoped filters",async()=>{
  vi.mocked(apiClient).mockResolvedValue({success:true,data:{items:[{memberId:"c",name:"Creator",contentsCreated:2,creatorPublishedPosts:1,publisherPublishedPosts:0,approvalRate:null,turnaroundHours:null,onTimeRate:null,failedPublishRate:null,pendingSchedules:0,completedSchedules:0,failedSchedules:0,postsWithInsights:0,engagement:null,impressions:null,reach:null,engagementRate:null,insightsUpdatedAt:null}],total:1,updatedAt:"2026-09-08T00:00:00Z",unattributedContents:null,brands:[{id:"alpha",name:"Alpha"}],teams:[],members:[{id:"c",name:"Creator"}],metricDefinitions:{period:"UTC"}}});
  render(<Page/>);await screen.findByText("Chưa đồng bộ");expect(screen.getAllByText("Chưa đủ dữ liệu").length).toBeGreaterThan(1);
  fireEvent.change(screen.getByLabelText("Brand"),{target:{value:"alpha"}});
  await waitFor(()=>expect(apiClient).toHaveBeenLastCalledWith(expect.stringContaining("brandId=alpha")));
});
it("settles loading on an API error",async()=>{
  vi.mocked(apiClient).mockRejectedValue(new Error("Không có quyền"));render(<Page/>);
  expect((await screen.findByRole("alert")).textContent).toContain("Không có quyền");expect(screen.queryByRole("status")).toBeNull();
});
it("renders API JSON with omitted nullable metrics without crashing", async () => {
  vi.mocked(apiClient).mockResolvedValue({success:true,data:{
    items:[{memberId:"c",name:"Creator",contentsCreated:2,creatorPublishedPosts:0,
      publisherPublishedPosts:0,reviewedSubmissions:0,pendingSchedules:0,
      completedSchedules:0,failedSchedules:0,postsWithInsights:0}],
    total:1,updatedAt:"2026-09-08T00:00:00Z",brands:[],teams:[],members:[],metricDefinitions:{}
  }});
  render(<Page/>);
  await screen.findByText("Chưa đồng bộ");
  expect(screen.getAllByText("Chưa đủ dữ liệu")).toHaveLength(8);
  expect(screen.queryByText(/Nội dung chưa xác định Creator:/)).toBeNull();
  expect(screen.queryByRole("alert")).toBeNull();
  expect(screen.getByRole("meter").getAttribute("value")).toBe("2");
});
