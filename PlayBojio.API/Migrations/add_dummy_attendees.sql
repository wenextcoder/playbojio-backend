-- Add Dummy Attendees fields to Events table
ALTER TABLE "Events" 
ADD COLUMN IF NOT EXISTS "DummyAttendeesCount" integer NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS "DummyAttendeesDescription" text;

-- Add Dummy Attendees fields to Sessions table
ALTER TABLE "Sessions" 
ADD COLUMN IF NOT EXISTS "DummyAttendeesCount" integer NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS "DummyAttendeesDescription" text;

-- Update comments
COMMENT ON COLUMN "Events"."DummyAttendeesCount" IS 'Number of placeholder/dummy attendees that reduce available slots';
COMMENT ON COLUMN "Events"."DummyAttendeesDescription" IS 'Names or details of dummy attendees';
COMMENT ON COLUMN "Sessions"."DummyAttendeesCount" IS 'Number of placeholder/dummy attendees that reduce available slots';
COMMENT ON COLUMN "Sessions"."DummyAttendeesDescription" IS 'Names or details of dummy attendees';
