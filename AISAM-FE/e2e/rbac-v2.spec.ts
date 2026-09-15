import { expect, test } from "./fixtures/aisam";

for (const role of ["Owner", "WorkspaceManager", "Member"] as const) {
  test(`v2 Team administration respects ${role} contract`, async ({ userPage }) => {
    await userPage.route("**/api/permissions/context", route => route.fulfill({json:{success:true,data:{
      contractVersion:2,revision:"v2-test",workspaceRole:role,teams:[],scopes:[],
      actions:role === "Member" ? [] : ["team.manage"],
    }}}));
    await userPage.route("**/api/teams/manage", route => route.fulfill({headers:{"X-HR-Revision":"hr-test","Access-Control-Expose-Headers":"X-HR-Revision"},json:{success:true,data:{items:[],totalCount:0}}}));
    await userPage.goto("/team");
    await expect(userPage.getByRole("heading",{name:"Team và thành viên"})).toBeVisible();
    const invite=userPage.getByRole("button",{name:"Mời vào workspace"});
    if(role === "Member") {
      await expect(invite).toHaveCount(0);
      await expect(userPage.getByRole("button",{name:"Tạo Team",exact:true})).toHaveCount(0);
    } else {
      await expect(invite).toBeVisible();
      await expect(userPage.getByLabel("Vai trò workspace",{exact:true}).locator("option")).toHaveCount(role === "Owner" ? 2 : 1);
      let calls=0;
      await userPage.route("**/api/workspace-invitations", async route => {
        if(route.request().method() !== "POST") return route.fallback();
        calls++;
        expect(route.request().postDataJSON()).toMatchObject({workspaceRole:3});
        expect(route.request().postDataJSON()).not.toHaveProperty("role");
        expect(route.request().headers()["x-rbac-contract-version"]).toBe("2");
        expect(route.request().headers()["if-match"]).toBe("hr-test");
        await route.fulfill({status:403,json:{success:false,message:"Permission revoked",errorCode:"ACTION_NOT_ALLOWED"}});
      });
      await userPage.getByLabel("Email mời thành viên").fill("member@example.test");
      await invite.click();
      await expect(userPage.getByRole("alert").filter({hasText:"Bạn không còn quyền thực hiện thao tác này"})).toBeVisible();
      expect(calls).toBe(1);
      await expect(userPage).toHaveURL(/\/team$/);
    }
  });
}
