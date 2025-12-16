-- Create Friends table
CREATE TABLE IF NOT EXISTS "Friends" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" text NOT NULL,
    "FriendId" text NOT NULL,
    "CreatedAt" timestamp NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_Friends_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Friends_AspNetUsers_FriendId" FOREIGN KEY ("FriendId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Friends_UserId_FriendId" ON "Friends" ("UserId", "FriendId");

-- Create FriendRequests table
CREATE TABLE IF NOT EXISTS "FriendRequests" (
    "Id" SERIAL PRIMARY KEY,
    "SenderId" text NOT NULL,
    "ReceiverId" text NOT NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp NOT NULL DEFAULT NOW(),
    "RespondedAt" timestamp,
    CONSTRAINT "FK_FriendRequests_AspNetUsers_SenderId" FOREIGN KEY ("SenderId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FriendRequests_AspNetUsers_ReceiverId" FOREIGN KEY ("ReceiverId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_FriendRequests_SenderId_ReceiverId" ON "FriendRequests" ("SenderId", "ReceiverId");

-- Create GroupJoinRequests table
CREATE TABLE IF NOT EXISTS "GroupJoinRequests" (
    "Id" SERIAL PRIMARY KEY,
    "GroupId" integer NOT NULL,
    "UserId" text NOT NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp NOT NULL DEFAULT NOW(),
    "RespondedAt" timestamp,
    "RespondedByUserId" text,
    CONSTRAINT "FK_GroupJoinRequests_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupJoinRequests_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupJoinRequests_AspNetUsers_RespondedByUserId" FOREIGN KEY ("RespondedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_GroupJoinRequests_GroupId_UserId_Status" ON "GroupJoinRequests" ("GroupId", "UserId", "Status");

-- Create GroupInvitations table
CREATE TABLE IF NOT EXISTS "GroupInvitations" (
    "Id" SERIAL PRIMARY KEY,
    "GroupId" integer NOT NULL,
    "InvitedUserId" text NOT NULL,
    "InvitedByUserId" text NOT NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp NOT NULL DEFAULT NOW(),
    "RespondedAt" timestamp,
    CONSTRAINT "FK_GroupInvitations_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupInvitations_AspNetUsers_InvitedUserId" FOREIGN KEY ("InvitedUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_GroupInvitations_AspNetUsers_InvitedByUserId" FOREIGN KEY ("InvitedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_GroupInvitations_GroupId_InvitedUserId_Status" ON "GroupInvitations" ("GroupId", "InvitedUserId", "Status");

-- Insert migration history record
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251216072037_AddFriendsAndGroupInvitationSystem', '8.0.0')
ON CONFLICT DO NOTHING;
