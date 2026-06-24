# Profile Workspace Endpoint Matrix

## Workspace-only endpoints

- `GET /api/workspaces`
- `GET /api/brands`
- `POST /api/brands`
- `GET /api/workspace-members`
- `GET /api/dashboard`
- `GET /api/conversations`
- `GET /api/notifications`
- `GET /api/posts`
- `GET /api/products`

## Workspace + profile endpoints

- `POST /api/content`
- `POST /api/content-schedules`
- `POST /api/social-auth/{provider}/callback`
- `POST /api/ai/chat`
- `POST /api/dev/scheduler/run-now`
- `GET /api/social-accounts`

## Verified from code

- `AISAM.API/Controllers/ContentController.cs`
- `AISAM.API/Controllers/ContentSchedulesController.cs`
- `AISAM.API/Controllers/GeminiController.cs`
- `AISAM.API/Controllers/SocialAccountsController.cs`
- `AISAM.API/Controllers/SocialAuthController.cs`
- `AISAM.API/Controllers/DevSchedulerController.cs`
