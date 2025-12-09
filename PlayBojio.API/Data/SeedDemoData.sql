-- Seed Demo Data for PlayBojio
SET QUOTED_IDENTIFIER ON;
GO

-- First, let's get or create a demo user
DECLARE @DemoUserId NVARCHAR(450);

-- Check if demo user exists, if not create one
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'demo@playbojio.com')
BEGIN
    SET @DemoUserId = NEWID();
    
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                            PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, 
                            TwoFactorEnabled, LockoutEnabled, AccessFailedCount, DisplayName, 
                            WillingToHost, IsProfilePublic, AttendedSessions, TotalSessions, CreatedAt)
    VALUES (
        @DemoUserId,
        'demo@playbojio.com',
        'DEMO@PLAYBOJIO.COM',
        'demo@playbojio.com',
        'DEMO@PLAYBOJIO.COM',
        1,
        'AQAAAAIAAYagAAAAEBw5KxPZ9ZqYwN7V5XE8K5xF5H7fJz1P9+Z5YQxKLN8T5fH7J9Z1P+xF5K8V7XE5', -- demo123
        NEWID(),
        NEWID(),
        0,
        0,
        1,
        0,
        'Demo User',
        1,
        1,
        0,
        0,
        GETDATE()
    );
END
ELSE
BEGIN
    SELECT @DemoUserId = Id FROM AspNetUsers WHERE Email = 'demo@playbojio.com';
END

-- Insert 6 Demo Sessions
INSERT INTO Sessions (Title, SessionType, Location, LocationType, StartTime, EndTime, CostPerPerson, 
                      CostNotes, PrimaryGame, AdditionalGames, MinPlayers, MaxPlayers, GameTags, 
                      IsNewbieFriendly, Language, AdditionalNotes, Visibility, IsCancelled, HostId, 
                      CreatedAt, ImageUrl)
VALUES
    -- Session 1
    ('Catan Night at Board Game Cafe', 0, 'The Mind Cafe, North Point', 1, 
     DATEADD(DAY, 2, GETDATE()), DATEADD(HOUR, 3, DATEADD(DAY, 2, GETDATE())), 15.00,
     'Includes drinks and snacks', 'Settlers of Catan', 'Cities & Knights expansion available', 
     3, 4, 'Strategy, Trading, Classic', 1, 'English', 
     'Great for beginners! We will teach the rules.', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?w=800&h=450&fit=crop'),
    
    -- Session 2
    ('Wingspan Tournament', 0, 'Home (Punggol)', 0,
     DATEADD(DAY, 5, GETDATE()), DATEADD(HOUR, 4, DATEADD(DAY, 5, GETDATE())), NULL,
     NULL, 'Wingspan', 'European and Oceania expansions', 
     2, 5, 'Strategy, Engine Building, Birds', 0, 'English', 
     'Bring your favorite bird strategies! Snacks will be provided.', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1606167668584-78701c57f13d?w=800&h=450&fit=crop'),
    
    -- Session 3
    ('Azul Casual Play', 0, 'Brewerkz, Riverside Point', 2,
     DATEADD(DAY, 7, GETDATE()), DATEADD(HOUR, 2, DATEADD(DAY, 7, GETDATE())), 25.00,
     'Includes lunch and drinks', 'Azul', 'Multiple variants available', 
     2, 4, 'Abstract, Tile Placement, Family', 1, 'English/Mandarin', 
     'Relaxed gaming session over lunch. All skill levels welcome!', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1566694191855-c136a7cb37d6?w=800&h=450&fit=crop'),
    
    -- Session 4
    ('Pandemic Legacy Campaign', 0, 'Home (Tampines)', 0,
     DATEADD(DAY, 10, GETDATE()), DATEADD(HOUR, 5, DATEADD(DAY, 10, GETDATE())), NULL,
     NULL, 'Pandemic Legacy: Season 1', 'Continuing our campaign', 
     3, 4, 'Cooperative, Legacy, Strategy', 0, 'English', 
     'This is session 3 of our campaign. Dedicated players only.', 1, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1606503153255-59d86e1c2763?w=800&h=450&fit=crop'),
    
    -- Session 5
    ('Party Game Night', 0, 'Orchard Gateway Community Club', 2,
     DATEADD(DAY, 3, GETDATE()), DATEADD(HOUR, 3, DATEADD(DAY, 3, GETDATE())), 10.00,
     'Venue rental cost sharing', 'Codenames', 'Secret Hitler, Avalon, One Night Werewolf', 
     6, 12, 'Party, Social Deduction, Fun', 1, 'English', 
     'Bring your friends! The more the merrier. Light snacks provided.', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&h=450&fit=crop'),
    
    -- Session 6
    ('Ticket to Ride: Europe', 0, 'Duxton Road Board Game Cafe', 1,
     DATEADD(DAY, 14, GETDATE()), DATEADD(HOUR, 2, DATEADD(DAY, 14, GETDATE())), 12.00,
     'Cafe minimum spend', 'Ticket to Ride: Europe', NULL, 
     2, 5, 'Route Building, Family, Gateway', 1, 'English', 
     'Perfect introduction to modern board games!', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1611891487688-ec7f3b8b8e9e?w=800&h=450&fit=crop');

-- Insert 6 Demo Events
INSERT INTO Events (Name, Description, StartDate, EndDate, Location, MapLink, MaxParticipants, 
                    Price, EventType, Visibility, IsCancelled, OrganizerId, CreatedAt, ImageUrl)
VALUES
    -- Event 1
    ('Singapore Board Game Convention 2024', 
     'Join us for the biggest board game convention in Singapore! Featuring demo stations, tournaments, and special guests from the industry.',
     DATEADD(DAY, 30, GETDATE()), DATEADD(DAY, 32, GETDATE()),
     'Marina Bay Sands Convention Centre', 'https://maps.google.com/?q=Marina+Bay+Sands',
     500, 50.00, 'Convention', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1559827260-dc66d52bef19?w=800&h=450&fit=crop'),
    
    -- Event 2
    ('Monthly Game Night Meetup', 
     'Our regular monthly meetup! Bring your favorite games or try something new. All are welcome!',
     DATEADD(DAY, 15, GETDATE()), DATEADD(DAY, 15, GETDATE()),
     'The Settlers Cafe, Bugis', 'https://maps.google.com/?q=The+Settlers+Cafe+Bugis',
     40, 15.00, 'Open Meetup', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1566694191855-c136a7cb37d6?w=800&h=450&fit=crop'),
    
    -- Event 3
    ('Gloomhaven Campaign Weekend', 
     'Two-day intensive Gloomhaven campaign! We will play through multiple scenarios with breaks for meals.',
     DATEADD(DAY, 20, GETDATE()), DATEADD(DAY, 21, GETDATE()),
     'Private Venue, Novena', NULL,
     8, 80.00, 'Campaign Event', 1, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1606503153255-59d86e1c2763?w=800&h=450&fit=crop'),
    
    -- Event 4
    ('Beginner Board Game Workshop', 
     'New to modern board games? Join our workshop where we will introduce you to popular gateway games and teach you how to play!',
     DATEADD(DAY, 8, GETDATE()), DATEADD(DAY, 8, GETDATE()),
     'Pasarbella, The Grandstand', 'https://maps.google.com/?q=Pasarbella+Grandstand',
     25, 25.00, 'Workshop', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?w=800&h=450&fit=crop'),
    
    -- Event 5
    ('Twilight Imperium Marathon', 
     'Epic space opera gaming! Full-day event dedicated to playing Twilight Imperium 4th Edition. Meals included.',
     DATEADD(DAY, 25, GETDATE()), DATEADD(DAY, 25, GETDATE()),
     'Private Gaming Space, Tanjong Pagar', NULL,
     6, 100.00, 'Marathon Event', 2, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1614286479089-7bff6f3ea870?w=800&h=450&fit=crop'),
    
    -- Event 6
    ('Family Game Day', 
     'Bring the whole family! We have games suitable for all ages from 6 to 60+. Food and refreshments included.',
     DATEADD(DAY, 12, GETDATE()), DATEADD(DAY, 12, GETDATE()),
     'East Coast Park Community Centre', 'https://maps.google.com/?q=East+Coast+Park+Community+Centre',
     60, 20.00, 'Family Event', 0, 0, @DemoUserId, GETDATE(),
     'https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&h=450&fit=crop');

PRINT 'Demo data seeded successfully!';
PRINT 'Demo user email: demo@playbojio.com';
PRINT 'Demo user password: demo123';

