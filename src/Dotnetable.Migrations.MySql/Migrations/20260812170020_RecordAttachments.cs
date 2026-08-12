using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.MySql.Migrations
{
    [Migration("20260812170020_RecordAttachments")]
    public class RecordAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS RecordAttachments (
  RecordAttachmentID BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  WebsiteID INT NOT NULL,
  EntityType VARCHAR(64) NOT NULL,
  EntityID BIGINT NOT NULL,
  FileRecordID INT NOT NULL,
  Title VARCHAR(200) NULL,
  Note VARCHAR(500) NULL,
  CreatedByMemberID INT NULL,
  CreatedAt DATETIME(6) NOT NULL,
  KEY IX_RecordAttachments_Entity (WebsiteID, EntityType, EntityID),
  KEY IX_RecordAttachments_File (FileRecordID),
  CONSTRAINT FK_RecordAttachments_Websites FOREIGN KEY (WebsiteID) REFERENCES Websites(WebsiteID),
  CONSTRAINT FK_RecordAttachments_Files FOREIGN KEY (FileRecordID) REFERENCES FileRecords(FileRecordID),
  CONSTRAINT FK_RecordAttachments_Members FOREIGN KEY (CreatedByMemberID) REFERENCES Members(MemberID)
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS RecordAttachments;");
        }
    }
}
