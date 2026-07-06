# AISAM Documentation

Tài liệu được chia thành ba nhóm để tránh lẫn tài liệu đang áp dụng với kế hoạch cũ.

## Tài liệu chính

Các file trong [`main`](./main/) là nguồn nên đọc và cập nhật trước:

- [Requirements](./main/requirements.md) — yêu cầu nghiệp vụ hiện hành.
- [Setup Guide](./main/setup-guide.md) — hướng dẫn thiết lập và chạy dự án.
- [Development Guardrails](./main/development-guardrails.md) — quy tắc khi sửa hệ thống.
- [User Story List](./main/user-story-list.md) — danh sách user story tổng.
- [Workspace Subscription Expiry Policy](./main/workspace-subscription-expiry-policy.md) — chính sách subscription/credit chuẩn.
- [AI Content Automation Proposal](./main/ai-content-automation-proposal.md) — đề xuất tính năng đang nghiên cứu.

## Tài liệu phụ và tra cứu

Các file trong [`reference`](./reference/) cung cấp bối cảnh, phân tích và chi tiết triển khai:

- [Workspace Subscription & Credit Analysis](./reference/workspace-subscription-credit-analysis.md).
- [Backend Progress vs SRS](./reference/backend-progress-vs-srs.md).
- [Specification Answers](./reference/specification-answers.md).
- [`reference/backend`](./reference/backend/) — bản mô tả codebase backend.
- [`reference/user-stories`](./reference/user-stories/) — chi tiết US-01 đến US-68.

## Tài liệu đã đóng

[`archive`](./archive/) chứa kế hoạch/spec đã hoàn thành, legacy hoặc đã được tài liệu mới thay thế:

- [`archive/plans`](./archive/plans/) — kế hoạch triển khai cũ.
- [`archive/specs`](./archive/specs/) — thiết kế/spec theo các phase cũ.
- [`archive/legacy`](./archive/legacy/) — file trùng hoặc không còn là nguồn chính.

Không sử dụng tài liệu archive làm quyết định hiện hành nếu nó mâu thuẫn với `docs/main`.

## Tài liệu giữ tại module

Một số file được giữ nguyên vị trí vì công cụ hoặc lập trình viên thường đọc trực tiếp tại module:

- [`../README.md`](../README.md) — tổng quan repository.
- [`../AISAM-FE/README.md`](../AISAM-FE/README.md) — hướng dẫn frontend.
- [`../AISAM-FE/DESIGN_SYSTEM.md`](../AISAM-FE/DESIGN_SYSTEM.md) — design system frontend.
- `AISAM-FE/AGENTS.md` và `AISAM-FE/CLAUDE.md` — chỉ dẫn dành cho coding agents.
