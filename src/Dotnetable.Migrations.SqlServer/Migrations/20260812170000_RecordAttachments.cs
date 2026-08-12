using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260812170000_RecordAttachments")]
    public class RecordAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RecordAttachments]', N'U') IS NULL
CREATE TABLE [dbo].[RecordAttachments](
  [RecordAttachmentID] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [WebsiteID] INT NOT NULL,
  [EntityType] NVARCHAR(64) NOT NULL,
  [EntityID] BIGINT NOT NULL,
  [FileRecordID] INT NOT NULL,
  [Title] NVARCHAR(200) NULL,
  [Note] NVARCHAR(500) NULL,
  [CreatedByMemberID] INT NULL,
  [CreatedAt] DATETIME2 NOT NULL,
  CONSTRAINT [FK_RecordAttachments_Websites] FOREIGN KEY([WebsiteID]) REFERENCES [dbo].[Websites]([WebsiteID]),
  CONSTRAINT [FK_RecordAttachments_Files] FOREIGN KEY([FileRecordID]) REFERENCES [dbo].[FileRecords]([FileRecordID]),
  CONSTRAINT [FK_RecordAttachments_Members] FOREIGN KEY([CreatedByMemberID]) REFERENCES [dbo].[Members]([MemberID])
);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RecordAttachments_Entity')
  CREATE NONCLUSTERED INDEX [IX_RecordAttachments_Entity] ON [dbo].[RecordAttachments]([WebsiteID],[EntityType],[EntityID]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RecordAttachments_File')
  CREATE NONCLUSTERED INDEX [IX_RecordAttachments_File] ON [dbo].[RecordAttachments]([FileRecordID]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RecordAttachments]', N'U') IS NOT NULL
  DROP TABLE [dbo].[RecordAttachments];
");
        }
    }
}
