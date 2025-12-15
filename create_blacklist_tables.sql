-- Check if tables exist first
DO $$ 
BEGIN
    -- Create EventBlacklists table if it doesn't exist
    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'EventBlacklists') THEN
        CREATE TABLE "EventBlacklists" (
            "Id" SERIAL PRIMARY KEY,
            "EventId" integer NOT NULL,
            "BlacklistedUserId" text NOT NULL,
            "BlacklistedByUserId" text NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
            "Reason" text NULL,
            CONSTRAINT "FK_EventBlacklists_AspNetUsers_BlacklistedByUserId" 
                FOREIGN KEY ("BlacklistedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_EventBlacklists_AspNetUsers_BlacklistedUserId" 
                FOREIGN KEY ("BlacklistedUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_EventBlacklists_Events_EventId" 
                FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE
        );

        CREATE INDEX "IX_EventBlacklists_BlacklistedByUserId" ON "EventBlacklists" ("BlacklistedByUserId");
        CREATE INDEX "IX_EventBlacklists_BlacklistedUserId" ON "EventBlacklists" ("BlacklistedUserId");
        CREATE UNIQUE INDEX "IX_EventBlacklists_EventId_BlacklistedUserId" ON "EventBlacklists" ("EventId", "BlacklistedUserId");
        
        RAISE NOTICE 'EventBlacklists table created successfully';
    ELSE
        RAISE NOTICE 'EventBlacklists table already exists';
    END IF;

    -- Create GroupBlacklists table if it doesn't exist
    IF NOT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'GroupBlacklists') THEN
        CREATE TABLE "GroupBlacklists" (
            "Id" SERIAL PRIMARY KEY,
            "GroupId" integer NOT NULL,
            "BlacklistedUserId" text NOT NULL,
            "BlacklistedByUserId" text NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
            "Reason" text NULL,
            CONSTRAINT "FK_GroupBlacklists_AspNetUsers_BlacklistedByUserId" 
                FOREIGN KEY ("BlacklistedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_GroupBlacklists_AspNetUsers_BlacklistedUserId" 
                FOREIGN KEY ("BlacklistedUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
            CONSTRAINT "FK_GroupBlacklists_Groups_GroupId" 
                FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id") ON DELETE CASCADE
        );

        CREATE INDEX "IX_GroupBlacklists_BlacklistedByUserId" ON "GroupBlacklists" ("BlacklistedByUserId");
        CREATE INDEX "IX_GroupBlacklists_BlacklistedUserId" ON "GroupBlacklists" ("BlacklistedUserId");
        CREATE UNIQUE INDEX "IX_GroupBlacklists_GroupId_BlacklistedUserId" ON "GroupBlacklists" ("GroupId", "BlacklistedUserId");
        
        RAISE NOTICE 'GroupBlacklists table created successfully';
    ELSE
        RAISE NOTICE 'GroupBlacklists table already exists';
    END IF;

    -- Update migration history if not already present
    IF NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251214120000_AddBlacklistTables') THEN
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20251214120000_AddBlacklistTables', '8.0.4');
        RAISE NOTICE 'Migration history updated';
    ELSE
        RAISE NOTICE 'Migration already recorded in history';
    END IF;
END $$;
