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
IF NOT EXISTS (SELECT 1 FROM [dbo].[Role] WHERE [RoleKey] = 'client.access')
    INSERT INTO [dbo].[Role] ([RoleKey], [Description], [Active], [Category])
    VALUES ('client.access', 'Sign in and general site access', 1, 1);

/* 2) Ensure the default customer "Users" access level exists for the website. */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Policy] WHERE [Title] = 'Users' AND [WebsiteID] = @WebsiteID)
    INSERT INTO [dbo].[Policy] ([Title], [Active], [WebsiteID])
    VALUES ('Users', 1, @WebsiteID);

DECLARE @UsersPolicyID INT =
    (SELECT TOP (1) [PolicyID] FROM [dbo].[Policy] WHERE [Title] = 'Users' AND [WebsiteID] = @WebsiteID);

/* 3) Grant the Users policy every client-category permission
      (client.access, client.purchase, client.review, client.profile),
      skipping any link that already exists. */
INSERT INTO [dbo].[PolicyRole] ([PolicyID], [RoleID], [Active])
SELECT @UsersPolicyID, r.[RoleID], 1
FROM [dbo].[Role] r
WHERE r.[Category] = 1
  AND r.[Active] = 1
  AND NOT EXISTS (
      SELECT 1 FROM [dbo].[PolicyRole] pr
      WHERE pr.[PolicyID] = @UsersPolicyID AND pr.[RoleID] = r.[RoleID]);
