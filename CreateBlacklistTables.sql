-- SQL Script to create GroupBlacklists and EventBlacklists tables on PostgreSQL
-- Run this script directly on your Render PostgreSQL database

-- Create GroupBlacklists table
CREATE TABLE IF NOT EXISTS "GroupBlacklists" (
    "Id" SERIAL PRIMARY KEY,
    "GroupId" integer NOT NULL,
    "BlacklistedUserId" text NOT NULL,
    "BlacklistedByUserId" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "Reason" text NULL,
    CONSTRAINT "FK_GroupBlacklists_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupBlacklists_AspNetUsers_BlacklistedUserId" FOREIGN KEY ("BlacklistedUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupBlacklists_AspNetUsers_BlacklistedByUserId" FOREIGN KEY ("BlacklistedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

-- Create EventBlacklists table
CREATE TABLE IF NOT EXISTS "EventBlacklists" (
    "Id" SERIAL PRIMARY KEY,
    "EventId" integer NOT NULL,
    "BlacklistedUserId" text NOT NULL,
    "BlacklistedByUserId" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "Reason" text NULL,
    CONSTRAINT "FK_EventBlacklists_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EventBlacklists_AspNetUsers_BlacklistedUserId" FOREIGN KEY ("BlacklistedUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_EventBlacklists_AspNetUsers_BlacklistedByUserId" FOREIGN KEY ("BlacklistedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_GroupBlacklists_GroupId_BlacklistedUserId" ON "GroupBlacklists" ("GroupId", "BlacklistedUserId");
CREATE INDEX IF NOT EXISTS "IX_GroupBlacklists_BlacklistedUserId" ON "GroupBlacklists" ("BlacklistedUserId");
CREATE INDEX IF NOT EXISTS "IX_GroupBlacklists_BlacklistedByUserId" ON "GroupBlacklists" ("BlacklistedByUserId");

CREATE INDEX IF NOT EXISTS "IX_EventBlacklists_EventId_BlacklistedUserId" ON "EventBlacklists" ("EventId", "BlacklistedUserId");
CREATE INDEX IF NOT EXISTS "IX_EventBlacklists_BlacklistedUserId" ON "EventBlacklists" ("BlacklistedUserId");
CREATE INDEX IF NOT EXISTS "IX_EventBlacklists_BlacklistedByUserId" ON "EventBlacklists" ("BlacklistedByUserId");

-- Add unique constraint
CREATE UNIQUE INDEX IF NOT EXISTS "UQ_GroupBlacklists_GroupId_BlacklistedUserId" ON "GroupBlacklists" ("GroupId", "BlacklistedUserId");
CREATE UNIQUE INDEX IF NOT EXISTS "UQ_EventBlacklists_EventId_BlacklistedUserId" ON "EventBlacklists" ("EventId", "BlacklistedUserId");

-- Update migrations history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251214102608_AddGroupAndEventBlacklists', '8.0.0')
ON CONFLICT DO NOTHING;

-- Verify tables were created
SELECT 
    'GroupBlacklists' as table_name, 
    COUNT(*) as row_count 
FROM "GroupBlacklists"
UNION ALL
SELECT 
    'EventBlacklists' as table_name, 
    COUNT(*) as row_count 
FROM "EventBlacklists";
