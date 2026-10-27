-- ============================================================
-- Leave Management System — Supplemental Seed Script v2
-- ============================================================
-- PRE-CONDITION: seed_test_data.sql has already been run.
-- That means: 15 employees + 50 leave requests already exist.
--
-- This script:
--   1. Adds 2 more Managers and 1 more HR user
--   2. Adds 25 more leave requests, with audit records
--      distributed across all 3 managers (realistic scenario)
-- ============================================================

USE LeaveManagementDb;   -- << change if your DB name differs
GO

-- ============================================================
-- SECTION 1: Add 2 Managers + 1 HR
-- ============================================================
DECLARE @pwdHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAELm/PDHKb3KTlvKJT/P3xgGX9R2RQaUMhxiqWWJRaFXNP8WXoNcVvr9AwV2l8RfHrA==';

DECLARE @managerRoleId INT = (SELECT Id FROM AspNetRoles WHERE NormalizedName = 'MANAGER');
DECLARE @hrRoleId      INT = (SELECT Id FROM AspNetRoles WHERE NormalizedName = 'HR');

-- ── Manager 2: Sarah Connor ──────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'SARAH.MANAGER@TEST.COM')
BEGIN
    INSERT INTO AspNetUsers
        (UserName, NormalizedUserName, Email, NormalizedEmail,
         EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
         PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
         AccessFailedCount, FirstName, LastName)
    VALUES
        ('sarah.manager@test.com', 'SARAH.MANAGER@TEST.COM',
         'sarah.manager@test.com', 'SARAH.MANAGER@TEST.COM',
         1, @pwdHash, NEWID(), NEWID(),
         0, 0, 1, 0, 'Sarah', 'Connor');

    DECLARE @mgr2Id INT = SCOPE_IDENTITY();
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@mgr2Id, @managerRoleId);
    PRINT 'Created Manager: sarah.manager@test.com (ID=' + CAST(@mgr2Id AS NVARCHAR) + ')';
END
ELSE
    PRINT 'Skipped: sarah.manager@test.com already exists.';

-- ── Manager 3: Raj Patel ─────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'RAJ.MANAGER@TEST.COM')
BEGIN
    INSERT INTO AspNetUsers
        (UserName, NormalizedUserName, Email, NormalizedEmail,
         EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
         PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
         AccessFailedCount, FirstName, LastName)
    VALUES
        ('raj.manager@test.com', 'RAJ.MANAGER@TEST.COM',
         'raj.manager@test.com', 'RAJ.MANAGER@TEST.COM',
         1, @pwdHash, NEWID(), NEWID(),
         0, 0, 1, 0, 'Raj', 'Patel');

    DECLARE @mgr3Id INT = SCOPE_IDENTITY();
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@mgr3Id, @managerRoleId);
    PRINT 'Created Manager: raj.manager@test.com (ID=' + CAST(@mgr3Id AS NVARCHAR) + ')';
END
ELSE
    PRINT 'Skipped: raj.manager@test.com already exists.';

-- ── HR 2: Priya Sharma ───────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'PRIYA.HR@TEST.COM')
BEGIN
    INSERT INTO AspNetUsers
        (UserName, NormalizedUserName, Email, NormalizedEmail,
         EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
         PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
         AccessFailedCount, FirstName, LastName)
    VALUES
        ('priya.hr@test.com', 'PRIYA.HR@TEST.COM',
         'priya.hr@test.com', 'PRIYA.HR@TEST.COM',
         1, @pwdHash, NEWID(), NEWID(),
         0, 0, 1, 0, 'Priya', 'Sharma');

    DECLARE @hr2Id INT = SCOPE_IDENTITY();
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@hr2Id, @hrRoleId);
    PRINT 'Created HR: priya.hr@test.com (ID=' + CAST(@hr2Id AS NVARCHAR) + ')';
END
ELSE
    PRINT 'Skipped: priya.hr@test.com already exists.';
GO

-- ============================================================
-- SECTION 2: 25 more leave requests, auditors spread across
--            all 3 managers
-- ============================================================
-- Fetch all 3 manager IDs into variables for round-robin assignment
DECLARE @mgr1Id INT, @mgr2Id INT, @mgr3Id INT;

SELECT @mgr1Id = MIN(u.Id)
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.NormalizedName = 'MANAGER';

SELECT @mgr3Id = MAX(u.Id)
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.NormalizedName = 'MANAGER';

SELECT @mgr2Id = u.Id
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.NormalizedName = 'MANAGER'
  AND u.Id != @mgr1Id AND u.Id != @mgr3Id;

PRINT 'Manager IDs: ' + CAST(@mgr1Id AS NVARCHAR) + ', ' + CAST(@mgr2Id AS NVARCHAR) + ', ' + CAST(@mgr3Id AS NVARCHAR);

-- All employee user IDs into a temp table
CREATE TABLE #EmpIds2 (RowNum INT IDENTITY(1,1), UserId INT);

INSERT INTO #EmpIds2 (UserId)
SELECT u.Id
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.NormalizedName = 'EMPLOYEE'
ORDER BY u.Id;

DECLARE @empCount INT = (SELECT COUNT(*) FROM #EmpIds2);

-- ── Leave request batch ───────────────────────────────────────
-- Columns: LeaveType, StartDate, EndDate, Reason, LeaveStatus,
--          AuditAction (NULL=Pending), AuditComments, ManagerSlot (1/2/3)
-- ManagerSlot controls which manager acts as auditor (round-robin of 3)

DECLARE @batch TABLE (
    Idx           INT IDENTITY(1,1),
    LeaveType     INT,
    StartDate     DATE,
    EndDate       DATE,
    Reason        NVARCHAR(500),
    LeaveStatus   INT,
    AuditAction   INT NULL,
    AuditComments NVARCHAR(500) NULL,
    ManagerSlot   INT           -- 1, 2, or 3
);

INSERT INTO @batch (LeaveType, StartDate, EndDate, Reason, LeaveStatus, AuditAction, AuditComments, ManagerSlot) VALUES
-- Approved by Manager 1 (Jane Smith — original seeded manager)
(0, '2025-10-27', '2025-10-29', 'Team outing follow-up leave',         1, 0, 'Approved.',                           1),
(1, '2025-11-03', '2025-11-04', 'Seasonal flu',                        1, 0, 'Approved. Get well soon.',            1),
(0, '2025-11-10', '2025-11-12', 'Personal errands — pre-festival',     1, 0, 'Approved.',                           1),
(4, '2025-11-17', '2025-11-24', 'Paternity leave — twins born',        1, 0, 'Approved. Congratulations!',          1),
(2, '2025-12-08', '2025-12-12', 'Unpaid leave — relocation support',   2, 1, 'Insufficient notice provided.',       1),
(0, '2026-01-05', '2026-01-07', 'New year extended break',             0, NULL, NULL,                               1),
(1, '2026-01-19', '2026-01-20', 'Recurring migraine',                  1, 0, 'Approved with doctor note.',          1),
(0, '2026-02-09', '2026-02-11', 'Valentine''s week travel',            2, 1, 'Project go-live week. Denied.',       1),
-- Approved by Manager 2 (Sarah Connor)
(0, '2025-10-27', '2025-10-28', 'Religious festival observance',       1, 0, 'Approved.',                           2),
(1, '2025-11-03', '2025-11-05', 'Post-accident rest — minor injury',   1, 0, 'Approved with medical certificate.',  2),
(3, '2025-11-01', '2026-02-01', 'Maternity leave',                     1, 0, 'Approved. Best wishes!',              2),
(0, '2025-12-01', '2025-12-03', 'Year-end personal errands',           3, 2, 'Cancelled by employee on request.',   2),
(2, '2026-01-12', '2026-01-16', 'Extended family care — abroad',       1, 0, 'Approved after documentation.',       2),
(0, '2026-02-02', '2026-02-04', 'Engagement ceremony travel',          1, 0, 'Approved.',                           2),
(1, '2026-02-16', '2026-02-17', 'Eye infection',                       0, NULL, NULL,                               2),
(0, '2026-03-02', '2026-03-04', 'Holi festival holiday extension',     1, 0, 'Approved.',                           2),
-- Approved by Manager 3 (Raj Patel)
(0, '2025-11-03', '2025-11-05', 'Sibling''s wedding ceremony',         1, 0, 'Approved.',                           3),
(1, '2025-11-17', '2025-11-18', 'Stomach infection — doctor''s rest',  1, 0, 'Approved.',                           3),
(0, '2025-12-15', '2025-12-17', 'Pre-Christmas shopping and errands',  2, 1, 'Quarter-close week. Rejected.',       3),
(4, '2026-01-05', '2026-01-12', 'Paternity leave',                     1, 0, 'Approved.',                           3),
(0, '2026-01-26', '2026-01-28', 'Republic Day long weekend',           1, 0, 'Approved.',                           3),
(2, '2026-02-23', '2026-02-27', 'Unpaid study leave — board exams',    0, NULL, NULL,                               3),
(1, '2026-03-09', '2026-03-10', 'Seasonal allergies — rest advised',   1, 0, 'Approved.',                           3),
(0, '2026-03-23', '2026-03-25', 'Pre-Eid travel',                      3, 2, 'Cancelled by employee.',              3),
(0, '2026-04-06', '2026-04-08', 'Ugadi / Tamil New Year break',        0, NULL, NULL,                               3);

-- ── Insert the batch ─────────────────────────────────────────
DECLARE @i         INT = 1;
DECLARE @total     INT = (SELECT COUNT(*) FROM @batch);

WHILE @i <= @total
BEGIN
    DECLARE @empRow    INT  = ((@i - 1) % @empCount) + 1;
    DECLARE @empId     INT  = (SELECT UserId FROM #EmpIds2 WHERE RowNum = @empRow);

    DECLARE @lType     INT;
    DECLARE @sDate     DATE;
    DECLARE @eDate     DATE;
    DECLARE @reason    NVARCHAR(500);
    DECLARE @lStatus   INT;
    DECLARE @aAction   INT;
    DECLARE @aComments NVARCHAR(500);
    DECLARE @mgrSlot   INT;

    SELECT
        @lType     = LeaveType,
        @sDate     = StartDate,
        @eDate     = EndDate,
        @reason    = Reason,
        @lStatus   = LeaveStatus,
        @aAction   = AuditAction,
        @aComments = AuditComments,
        @mgrSlot   = ManagerSlot
    FROM @batch WHERE Idx = @i;

    -- Pick the auditor based on ManagerSlot
    DECLARE @auditorId INT =
        CASE @mgrSlot
            WHEN 1 THEN @mgr1Id
            WHEN 2 THEN @mgr2Id
            WHEN 3 THEN @mgr3Id
            ELSE @mgr1Id
        END;

    DECLARE @leaveId   UNIQUEIDENTIFIER = NEWID();
    DECLARE @submitted DATETIME         = DATEADD(DAY, -14, CAST(@sDate AS DATETIME));

    INSERT INTO LeaveRequests (Id, SubmittedDate, EmployeeId, LeaveType, StartDate, EndDate, Reason, LeaveStatus)
    VALUES (@leaveId, @submitted, @empId, @lType, @sDate, @eDate, @reason, @lStatus);

    IF @aAction IS NOT NULL
    BEGIN
        INSERT INTO LeaveAudits (AuditId, LeaveRequestId, AuditorId, ProcessDateTime, Comments, Action)
        VALUES (NEWID(), @leaveId, @auditorId, DATEADD(DAY, 1, @submitted), @aComments, @aAction);
    END

    SET @i = @i + 1;
END

DROP TABLE #EmpIds2;
GO

-- ============================================================
-- SECTION 3: Verification
-- ============================================================
SELECT 'Users by Role' AS [Check];
SELECT r.Name AS Role, COUNT(*) AS UserCount
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r      ON r.Id      = ur.RoleId
GROUP BY r.Name;

SELECT 'All Manager accounts' AS [Check];
SELECT u.Id, u.FirstName, u.LastName, u.Email
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.NormalizedName = 'MANAGER';

SELECT 'Leave Requests by Status (total)' AS [Check];
SELECT
    CASE LeaveStatus
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Approved'
        WHEN 2 THEN 'Rejected'
        WHEN 3 THEN 'Canceled'
    END AS [Status],
    COUNT(*) AS Total
FROM LeaveRequests
GROUP BY LeaveStatus ORDER BY LeaveStatus;

SELECT 'Leave Requests by Type (total)' AS [Check];
SELECT
    CASE LeaveType
        WHEN 0 THEN 'Annual'
        WHEN 1 THEN 'Sick'
        WHEN 2 THEN 'Unpaid'
        WHEN 3 THEN 'Maternity'
        WHEN 4 THEN 'Paternity'
    END AS [Type],
    COUNT(*) AS Total
FROM LeaveRequests
GROUP BY LeaveType ORDER BY LeaveType;

SELECT 'Audit records by Auditor' AS [Check];
SELECT u.FirstName + ' ' + u.LastName AS Auditor, u.Email, COUNT(*) AS AuditCount
FROM LeaveAudits la
INNER JOIN AspNetUsers u ON u.Id = la.AuditorId
GROUP BY u.Id, u.FirstName, u.LastName, u.Email
ORDER BY AuditCount DESC;

SELECT 'Grand totals' AS [Check];
SELECT
    (SELECT COUNT(*) FROM LeaveRequests) AS TotalLeaveRequests,
    (SELECT COUNT(*) FROM LeaveAudits)   AS TotalAuditRecords,
    (SELECT COUNT(*) FROM AspNetUsers)   AS TotalUsers;
GO
