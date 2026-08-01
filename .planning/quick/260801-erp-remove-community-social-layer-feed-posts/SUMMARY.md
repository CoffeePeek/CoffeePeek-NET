---
status: complete
quick_id: 260801-erp
slug: remove-community-social-layer-feed-posts
completed: 2026-08-01
---

# SUMMARY: Remove Community Social Layer

## Done

Removed the entire community/social layer added in June 2026 (PR #224): feed, posts, comments on reviews/check-ins, reactions, follows, and notifications.

## Kept

- Coffee shop Reviews (create/list/delete/moderation)
- CheckIns
- Public platform stats (`/api/public/stats`)

## Changes

- Deleted ~100 community source files (controllers, handlers, domain, repos, contracts, consumers, tests)
- Stripped community couplings from DeleteReview, CreateCheckIn, ModerationReviewApproved
- Removed Gateway community routes and `community-write` rate-limit policy
- Added EF drop migrations:
  - `RemoveCommunitySocialLayer` (Shops)
  - `RemoveCommunityNotifications` (Account)
  - `RemoveModerationCommunityPosts` (Moderation)

## Verification

- `dotnet build CoffeePeek.slnx` — 0 errors
- `dotnet test CoffeePeek.slnx` — 454 passed, 0 failed
