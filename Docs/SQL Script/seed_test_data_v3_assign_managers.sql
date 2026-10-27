-- ============================================================
-- Leave Management System — v3: Assign Managers (Org Hierarchy)
-- ============================================================
-- PRE-CONDITION: seed_test_data.sql AND seed_test_data_v2.sql
--               have already been run AND the AddManagerToUser
--               migration has been applied (ManagerId column exists).
--
-- PURPOSE: Populates ManagerId on AspNetUsers to reflect the
--          real org hierarchy:
--
--   hr@test.com  (Bob HR — top of org, no manager)
--   ├── manager@test.com       (Jane Smith)
--   ├── sarah.manager@test.com (Sarah Connor)
--   ├── raj.manager@test.com   (Raj Patel)
--   └── priya.hr@test.com      (Priya Sharma)
--
--   Team 1 — Jane Smith   : employee@test.com, alice, bob,
--                            carol, david, emily        [6]
--   Team 2 — Sarah Connor : frank, grace, henry,
--                            isabel, james              [5]
--   Team 3 — Raj Patel    : karen, liam, mia,
--                            noah, olivia               [5]
--
-- NOTE: Updates are unconditional (no ManagerId IS NULL guard)
--       so this script always enforces the correct state,
--       whether run on a fresh DB or an existing one.
-- ============================================================

USE LeaveManagementDb;   -- << change if your DB name is different
GO

-- ============================================================
-- SECTION 1: Resolve user IDs by normalised email
-- ============================================================
DECLARE @hrId   INT = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'HR@TEST.COM');
DECLARE @mgr1Id INT = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'MANAGER@TEST.COM');
DECLARE @mgr2Id INT = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'SARAH.MANAGER@TEST.COM');
DECLARE @mgr3Id INT = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'RAJ.MANAGER@TEST.COM');

-- Fail fast if any required account is missing.
IF @hrId IS NULL OR @mgr1Id IS NULL OR @mgr2Id IS NULL OR @mgr3Id IS NULL
BEGIN
    RAISERROR(
        'One or more required accounts were not found. ' +
        'Ensure seed_test_data.sql and seed_test_data_v2.sql have been run before this script.',
        16, 1
    );
    RETURN;
END

PRINT 'IDs resolved — HR: '  + CAST(@hrId   AS NVARCHAR)
    + ', Mgr1: '             + CAST(@mgr1Id AS NVARCHAR)
    + ', Mgr2: '             + CAST(@mgr2Id AS NVARCHAR)
    + ', Mgr3: '             + CAST(@mgr3Id AS NVARCHAR);
GO

-- ============================================================
-- SECTION 2: Wire up the org hierarchy
-- ============================================================

DECLARE @hrId   INT = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'HR@TEST.COM');
DECLARE @mgr1Id INT = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'MANAGER@TEST.COM');
DECLARE @mgr2Id INT = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'SARAH.MANAGER@TEST.COM');
DECLARE @mgr3Id INT = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'RAJ.MANAGER@TEST.COM');

-- ── Level 1: Managers + secondary HR report to hr@test.com ──
UPDATE AspNetUsers
SET ManagerId = @hrId
WHERE NormalizedEmail IN (
    'MANAGER@TEST.COM',
    'SARAH.MANAGER@TEST.COM',
    'RAJ.MANAGER@TEST.COM',
    'PRIYA.HR@TEST.COM'
);

PRINT 'Level 1 assigned (→ HR): ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' row(s) updated.';

-- ── Level 2 Team 1: Jane Smith's direct reports ──────────────
UPDATE AspNetUsers
SET ManagerId = @mgr1Id
WHERE NormalizedEmail IN (
    'EMPLOYEE@TEST.COM',
    'ALICE.JOHNSON@TEST.COM',
    'BOB.MARTINEZ@TEST.COM',
    'CAROL.WHITE@TEST.COM',
    'DAVID.BROWN@TEST.COM',
    'EMILY.DAVIS@TEST.COM'
);

PRINT 'Team 1 assigned (→ Jane Smith): ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' row(s) updated.';

-- ── Level 2 Team 2: Sarah Connor's direct reports ────────────
UPDATE AspNetUsers
SET ManagerId = @mgr2Id
WHERE NormalizedEmail IN (
    'FRANK.WILSON@TEST.COM',
    'GRACE.MOORE@TEST.COM',
    'HENRY.TAYLOR@TEST.COM',
    'ISABEL.ANDERSON@TEST.COM',
    'JAMES.THOMAS@TEST.COM'
);

PRINT 'Team 2 assigned (→ Sarah Connor): ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' row(s) updated.';

-- ── Level 2 Team 3: Raj Patel's direct reports ───────────────
UPDATE AspNetUsers
SET ManagerId = @mgr3Id
WHERE NormalizedEmail IN (
    'KAREN.JACKSON@TEST.COM',
    'LIAM.HARRIS@TEST.COM',
    'MIA.CLARK@TEST.COM',
    'NOAH.LEWIS@TEST.COM',
    'OLIVIA.LEE@TEST.COM'
);

PRINT 'Team 3 assigned (→ Raj Patel): ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' row(s) updated.';
GO

-- ============================================================
-- SECTION 3: Verification
-- ============================================================

-- Direct report count per manager (full org view)
SELECT
    ISNULL(m.FirstName + ' ' + m.LastName, '(none — top of org)') AS Manager,
    ISNULL(m.Email, '')                                            AS ManagerEmail,
    COUNT(e.Id)                                                    AS DirectReportCount
FROM AspNetUsers e
LEFT JOIN AspNetUsers m ON m.Id = e.ManagerId
GROUP BY m.Id, m.FirstName, m.LastName, m.Email
ORDER BY DirectReportCount DESC;

-- Full org chart: every user with their manager
SELECT
    e.FirstName + ' ' + e.LastName                        AS [User],
    e.Email                                               AS [Email],
    r.Name                                                AS [Role],
    ISNULL(m.FirstName + ' ' + m.LastName, '— top of org') AS [ReportsTo]
FROM AspNetUsers e
INNER JOIN AspNetUserRoles ur ON ur.UserId = e.Id
INNER JOIN AspNetRoles r      ON r.Id      = ur.RoleId
LEFT  JOIN AspNetUsers m      ON m.Id      = e.ManagerId
ORDER BY [ReportsTo], [Role], [User];

-- Users with no manager (should be only hr@test.com)
SELECT
    e.FirstName + ' ' + e.LastName AS [User],
    e.Email,
    r.Name AS Role
FROM AspNetUsers e
INNER JOIN AspNetUserRoles ur ON ur.UserId = e.Id
INNER JOIN AspNetRoles r      ON r.Id      = ur.RoleId
WHERE e.ManagerId IS NULL;
GO
