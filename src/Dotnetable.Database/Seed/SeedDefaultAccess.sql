/* =====================================================================
   Default-access seed for EXISTING databases (created before the
   self-registration feature). Fresh installs get this automatically
   from InitialDataSeeder; run this once, manually, on databases that
   were already set up.

   Idempotent: safe to run more than once. Targets the master website
   (WebsiteID = 1). Adjust @WebsiteID to seed another website.
   ===================================================================== */
SET NOCOUNT ON;

DECLARE @WebsiteID INT = 1;

/* 1) Ensure the "Sign in and general site access" client permission exists.
      (Existing rows are also topped up at app startup by SyncRoleCatalogAsync.) */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleKey] = 'client.access')
    INSERT INTO [dbo].[Roles] ([RoleKey], [Description], [Active], [Category])
    VALUES ('client.access', 'Sign in and general site access', 1, 1);

/* 2) Ensure the default customer "Users" access level exists for the website. */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Policies] WHERE [Title] = 'Users' AND [WebsiteID] = @WebsiteID)
    INSERT INTO [dbo].[Policies] ([Title], [Active], [WebsiteID])
    VALUES ('Users', 1, @WebsiteID);

DECLARE @UsersPolicyID INT =
    (SELECT TOP (1) [PolicyID] FROM [dbo].[Policies] WHERE [Title] = 'Users' AND [WebsiteID] = @WebsiteID);

/* 3) Grant the Users policy every client-category permission
      (client.access, client.purchase, client.review, client.profile),
      skipping any link that already exists. */
INSERT INTO [dbo].[PolicyRoles] ([PolicyID], [RoleID], [Active])
SELECT @UsersPolicyID, r.[RoleID], 1
FROM [dbo].[Roles] r
WHERE r.[Category] = 1
  AND r.[Active] = 1
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[PolicyRoles] pr
      WHERE pr.[PolicyID] = @UsersPolicyID AND pr.[RoleID] = r.[RoleID]);
