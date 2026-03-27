-- migrate-existing-users-email-confirmed.sql
--
-- Run ONCE on any environment that has users registered BEFORE email verification
-- enforcement was deployed (i.e. before the login block was enabled).
--
-- This marks all existing unverified users as email-confirmed so they are not
-- locked out when the new login check goes live.
--
-- Instructions:
--   1. Review the WHERE clause — remove or adjust if you want to exclude certain accounts.
--   2. Run against your target database BEFORE deploying the login enforcement change.
--   3. This is a one-time operation; it is safe to run multiple times (idempotent).
--
-- Usage (PostgreSQL):
--   psql "$DATABASE_URL" -f migrate-existing-users-email-confirmed.sql

UPDATE "AspNetUsers"
SET "EmailConfirmed" = TRUE
WHERE "EmailConfirmed" = FALSE;
