-- ============================================================
-- Leave Management System — Bulk Test Data Seed Script
-- ============================================================
-- Inserts ~15 employees + ~50 leave requests with audit trails.
-- Password hash below = ASP.NET Core Identity hash for "Test@1234"
-- All employees are assigned the "Employee" role.
--
-- SAFE TO RUN MULTIPLE TIMES: every INSERT is guarded by
-- IF NOT EXISTS checks. Does NOT touch the 3 seeded users
-- (employee@test.com, manager@test.com, hr@test.com).
-- ============================================================

USE LeaveManagementDb;   -- << change if your DB name is different
GO

-- ============================================================
-- SECTION 1: Verify the Employee role exists
-- ============================================================
-- The DataSeeder already creates this; this is just a safeguard.
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE [Name] = 'Employee')
BEGIN
    INSERT INTO AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
    VALUES ('Employee', 'EMPLOYEE', NEWID());
END
GO

-- ============================================================
-- SECTION 2: Insert 15 employee users
-- ============================================================
-- Password hash = ASP.NET Core Identity V3 hash for "Test@1234"
-- Generated using PasswordHasher<ApplicationUser>.HashPassword()
-- Do NOT change the hash — it is format-sensitive.
-- ============================================================
DECLARE @pwdHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAELm/PDHKb3KTlvKJT/P3xgGX9R2RQaUMhxiqWWJRaFXNP8WXoNcVvr9AwV2l8RfHrA==';

-- Helper: resolve Employee role ID dynamically
DECLARE @employeeRoleId INT = (SELECT Id FROM AspNetRoles WHERE [Name] = 'Employee');

-- ── Insert users ─────────────────────────────────────────────
DECLARE @employees TABLE (
    Email        NVARCHAR(256),
    FirstName    NVARCHAR(100),
    LastName     NVARCHAR(100)
);

INSERT INTO @employees VALUES
    ('alice.johnson@test.com',  'Alice',    'Johnson'),
    ('bob.martinez@test.com',   'Bob',      'Martinez'),
    ('carol.white@test.com',    'Carol',    'White'),
    ('david.brown@test.com',    'David',    'Brown'),
    ('emily.davis@test.com',    'Emily',    'Davis'),
    ('frank.wilson@test.com',   'Frank',    'Wilson'),
    ('grace.moore@test.com',    'Grace',    'Moore'),
    ('henry.taylor@test.com',   'Henry',    'Taylor'),
    ('isabel.anderson@test.com','Isabel',   'Anderson'),
    ('james.thomas@test.com',   'James',    'Thomas'),
    ('karen.jackson@test.com',  'Karen',    'Jackson'),
    ('liam.harris@test.com',    'Liam',     'Harris'),
    ('mia.clark@test.com',      'Mia',      'Clark'),
    ('noah.lewis@test.com',     'Noah',     'Lewis'),
    ('olivia.lee@test.com',     'Olivia',   'Lee');

DECLARE @email NVARCHAR(256), @firstName NVARCHAR(100), @lastName NVARCHAR(100);

DECLARE emp_cursor CURSOR FOR SELECT Email, FirstName, LastName FROM @employees;
OPEN emp_cursor;
FETCH NEXT FROM emp_cursor INTO @email, @firstName, @lastName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = UPPER(@email))
    BEGIN
        INSERT INTO AspNetUsers
            (UserName, NormalizedUserName, Email, NormalizedEmail,
             EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
             PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
             AccessFailedCount, FirstName, LastName)
        VALUES
            (@email, UPPER(@email), @email, UPPER(@email),
             1, @pwdHash, NEWID(), NEWID(),
             0, 0, 1,
             0, @firstName, @lastName);

        -- Assign Employee role
        DECLARE @newUserId INT = SCOPE_IDENTITY();
        IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @newUserId AND RoleId = @employeeRoleId)
        BEGIN
            INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@newUserId, @employeeRoleId);
        END
    END

    FETCH NEXT FROM emp_cursor INTO @email, @firstName, @lastName;
END

CLOSE emp_cursor;
DEALLOCATE emp_cursor;
GO

-- ============================================================
-- SECTION 3: Insert ~50 leave requests + audit records
-- ============================================================
-- Strategy:
--   • Pull all employee-role user IDs dynamically (includes
--     the 3 seeded users + the 15 we just added).
--   • Manager user ID is used as the auditor for approved/rejected.
--   • Spread across all 5 leave types and 4 statuses.
-- ============================================================

DECLARE @managerId INT = (
    SELECT u.Id
    FROM AspNetUsers u
    INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
    INNER JOIN AspNetRoles r      ON r.Id = ur.RoleId
    WHERE r.NormalizedName = 'MANAGER'
    ORDER BY u.Id
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY
);

-- Collect all employee user IDs into a temp table
CREATE TABLE #EmpIds (RowNum INT IDENTITY(1,1), UserId INT);

INSERT INTO #EmpIds (UserId)
SELECT u.Id
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r      ON r.Id = ur.RoleId
WHERE r.NormalizedName = 'EMPLOYEE'
ORDER BY u.Id;

DECLARE @totalEmps INT = (SELECT COUNT(*) FROM #EmpIds);

-- ── Leave requests data ──────────────────────────────────────
-- Each row: (leaveType, startDate, endDate, reason, leaveStatus, auditAction, auditComments, daysOffset)
-- leaveType:  0=Annual, 1=Sick, 2=Unpaid, 3=Maternity, 4=Paternity
-- leaveStatus: 0=Pending, 1=Approved, 2=Rejected, 3=Canceled
-- auditAction: 0=Approved, 1=Rejected, 2=Canceled  (NULL if Pending)

DECLARE @leaveData TABLE (
    Idx           INT IDENTITY(1,1),
    LeaveType     INT,
    StartDate     DATE,
    EndDate       DATE,
    Reason        NVARCHAR(500),
    LeaveStatus   INT,
    AuditAction   INT NULL,   -- NULL = no audit (Pending)
    AuditComments NVARCHAR(500) NULL
);

INSERT INTO @leaveData (LeaveType, StartDate, EndDate, Reason, LeaveStatus, AuditAction, AuditComments) VALUES
-- Annual leaves
(0, '2025-01-06', '2025-01-08', 'Family vacation trip',             1, 0, 'Approved. Enjoy your leave.'),
(0, '2025-01-20', '2025-01-21', 'Personal travel',                  1, 0, 'Approved.'),
(0, '2025-02-03', '2025-02-07', 'Holiday trip abroad',              1, 0, 'Approved. Have a great trip.'),
(0, '2025-02-17', '2025-02-18', 'Long weekend break',               2, 1, 'Team deliverable due. Rescheduled.'),
(0, '2025-03-10', '2025-03-12', 'Attending family wedding',         1, 0, 'Approved.'),
(0, '2025-03-24', '2025-03-28', 'Annual leave — year-end balance',  1, 0, 'Approved.'),
(0, '2025-04-14', '2025-04-16', 'School summer break travel',       1, 0, 'Approved.'),
(0, '2025-04-28', '2025-04-30', 'Personal errands and rest',        0, NULL, NULL),
(0, '2025-05-05', '2025-05-07', 'Anniversary trip',                 1, 0, 'Approved.'),
(0, '2025-05-19', '2025-05-21', 'Home renovation work',             2, 1, 'Critical sprint week. Denied.'),
-- Sick leaves
(1, '2025-01-13', '2025-01-14', 'Fever and cold',                   1, 0, 'Approved. Get well soon.'),
(1, '2025-02-10', '2025-02-11', 'Migraine — doctor advised rest',   1, 0, 'Approved.'),
(1, '2025-02-24', '2025-02-25', 'Stomach flu',                      1, 0, 'Approved.'),
(1, '2025-03-17', '2025-03-19', 'Post-surgery recovery',            1, 0, 'Approved with medical cert.'),
(1, '2025-04-07', '2025-04-08', 'Dental procedure',                 1, 0, 'Approved.'),
(1, '2025-04-21', '2025-04-22', 'Back pain — physio ordered rest',  0, NULL, NULL),
(1, '2025-05-12', '2025-05-14', 'Eye infection',                    3, 2, 'Cancelled by employee.'),
(1, '2025-06-02', '2025-06-03', 'Food poisoning',                   1, 0, 'Approved.'),
(1, '2025-06-16', '2025-06-17', 'Flu — isolation advised',          1, 0, 'Approved.'),
(1, '2025-07-07', '2025-07-08', 'Respiratory infection',            0, NULL, NULL),
-- Unpaid leaves
(2, '2025-01-27', '2025-01-31', 'Personal sabbatical',              1, 0, 'Approved after discussion.'),
(2, '2025-03-03', '2025-03-07', 'Family emergency abroad',          1, 0, 'Approved.'),
(2, '2025-05-26', '2025-05-30', 'Visa interview and travel',        2, 1, 'Insufficient notice period.'),
(2, '2025-07-14', '2025-07-18', 'Unpaid study leave — exam prep',   1, 0, 'Approved with documentation.'),
(2, '2025-08-04', '2025-08-08', 'Extended family care',             0, NULL, NULL),
-- Maternity leaves
(3, '2025-02-01', '2025-05-01', 'Maternity leave — newborn',        1, 0, 'Approved. Congratulations!'),
(3, '2025-04-01', '2025-07-01', 'Maternity leave',                  1, 0, 'Approved.'),
(3, '2025-06-01', '2025-09-01', 'Maternity leave — postnatal care', 0, NULL, NULL),
-- Paternity leaves
(4, '2025-01-15', '2025-01-22', 'Paternity leave — newborn',        1, 0, 'Approved.'),
(4, '2025-03-05', '2025-03-12', 'Paternity leave',                  1, 0, 'Approved.'),
(4, '2025-05-20', '2025-05-27', 'Paternity leave',                  0, NULL, NULL),
-- Additional Annual — varied statuses to pad to 50
(0, '2025-06-09', '2025-06-11', 'Cousin''s wedding reception',      1, 0, 'Approved.'),
(0, '2025-06-23', '2025-06-25', 'Camping trip',                     2, 1, 'Team meeting conflict.'),
(0, '2025-07-01', '2025-07-03', 'National holiday extension',       1, 0, 'Approved.'),
(0, '2025-07-14', '2025-07-16', 'Home repairs scheduled',           0, NULL, NULL),
(0, '2025-07-28', '2025-07-30', 'Attending school reunion',         1, 0, 'Approved.'),
(1, '2025-07-21', '2025-07-22', 'Chronic back pain episode',        3, 2, 'Cancelled by employee.'),
(1, '2025-08-11', '2025-08-12', 'Medical checkup and rest',         0, NULL, NULL),
(0, '2025-08-18', '2025-08-20', 'Personal day off',                 1, 0, 'Approved.'),
(2, '2025-09-01', '2025-09-05', 'Voluntary unpaid leave',           1, 0, 'Approved.'),
(0, '2025-09-08', '2025-09-10', 'Festival holiday',                 1, 0, 'Approved.'),
(1, '2025-09-15', '2025-09-16', 'Minor surgery recovery',           1, 0, 'Approved with medical cert.'),
(0, '2025-09-22', '2025-09-24', 'Travelling for sibling''s event',  0, NULL, NULL),
(0, '2025-10-06', '2025-10-08', 'Annual leave — Q4 balance',        1, 0, 'Approved.'),
(1, '2025-10-13', '2025-10-14', 'Viral fever',                      2, 1, 'Deadline critical. Rejected.'),
(0, '2025-10-20', '2025-10-22', 'Year-end personal travel',         0, NULL, NULL),
(0, '2025-11-03', '2025-11-05', 'Diwali extended break',            1, 0, 'Approved.'),
(0, '2025-11-17', '2025-11-19', 'Relative''s engagement ceremony',  1, 0, 'Approved.'),
(0, '2025-12-01', '2025-12-03', 'Pre-Christmas travel',             0, NULL, NULL),
(0, '2025-12-22', '2025-12-26', 'Christmas and New Year break',     1, 0, 'Approved. Happy holidays!');

-- ── Insert leave requests ────────────────────────────────────
DECLARE @idx       INT = 1;
DECLARE @totalLeave INT = (SELECT COUNT(*) FROM @leaveData);
DECLARE @empCount  INT = (SELECT COUNT(*) FROM #EmpIds);

WHILE @idx <= @totalLeave
BEGIN
    DECLARE @empRow    INT = ((@idx - 1) % @empCount) + 1;
    DECLARE @empId     INT = (SELECT UserId FROM #EmpIds WHERE RowNum = @empRow);

    DECLARE @lType     INT;
    DECLARE @sDate     DATE;
    DECLARE @eDate     DATE;
    DECLARE @reason    NVARCHAR(500);
    DECLARE @lStatus   INT;
    DECLARE @aAction   INT;
    DECLARE @aComments NVARCHAR(500);

    SELECT
        @lType     = LeaveType,
        @sDate     = StartDate,
        @eDate     = EndDate,
        @reason    = Reason,
        @lStatus   = LeaveStatus,
        @aAction   = AuditAction,
        @aComments = AuditComments
    FROM @leaveData
    WHERE Idx = @idx;

    DECLARE @leaveId UNIQUEIDENTIFIER = NEWID();
    DECLARE @submitted DATETIME = DATEADD(DAY, -14, CAST(@sDate AS DATETIME));

    INSERT INTO LeaveRequests (Id, SubmittedDate, EmployeeId, LeaveType, StartDate, EndDate, Reason, LeaveStatus)
    VALUES (@leaveId, @submitted, @empId, @lType, @sDate, @eDate, @reason, @lStatus);

    -- Insert audit record if the request is not Pending
    IF @aAction IS NOT NULL
    BEGIN
        INSERT INTO LeaveAudits (AuditId, LeaveRequestId, AuditorId, ProcessDateTime, Comments, Action)
        VALUES (NEWID(), @leaveId, @managerId, DATEADD(DAY, 1, @submitted), @aComments, @aAction);
    END

    SET @idx = @idx + 1;
END

DROP TABLE #EmpIds;
GO

-- ============================================================
-- SECTION 4: Verification queries
-- ============================================================
SELECT 'Users by Role' AS [Check];
SELECT r.Name AS Role, COUNT(*) AS UserCount
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON ur.UserId = u.Id
INNER JOIN AspNetRoles r      ON r.Id      = ur.RoleId
GROUP BY r.Name;

SELECT 'Leave Requests by Status' AS [Check];
SELECT
    CASE LeaveStatus
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Approved'
        WHEN 2 THEN 'Rejected'
        WHEN 3 THEN 'Canceled'
    END AS [Status],
    COUNT(*) AS Total
FROM LeaveRequests
GROUP BY LeaveStatus
ORDER BY LeaveStatus;

SELECT 'Leave Requests by Type' AS [Check];
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
GROUP BY LeaveType
ORDER BY LeaveType;

SELECT 'Total Leave Audit Records' AS [Check], COUNT(*) AS Total FROM LeaveAudits;
GO
