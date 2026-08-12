using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    [Migration("20260812170010_RecordAttachments")]
    public class RecordAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS ""RecordAttachments"" (
  ""RecordAttachmentID"" BIGSERIAL PRIMARY KEY,
  ""WebsiteID"" INT NOT NULL REFERENCES ""Websites""(""WebsiteID""),
  ""EntityType"" VARCHAR(64) NOT NULL,
  ""EntityID"" BIGINT NOT NULL,
  ""FileRecordID"" INT NOT NULL REFERENCES ""FileRecords""(""FileRecordID""),
  ""Title"" VARCHAR(200) NULL,
  ""Note"" VARCHAR(500) NULL,
  ""CreatedByMemberID"" INT NULL REFERENCES ""Members""(""MemberID""),
  ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL
);
CREATE INDEX IF NOT EXISTS ""IX_RecordAttachments_Entity"" ON ""RecordAttachments"" (""WebsiteID"", ""EntityType"", ""EntityID"");
CREATE INDEX IF NOT EXISTS ""IX_RecordAttachments_File"" ON ""RecordAttachments"" (""FileRecordID"");
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""RecordAttachments"";");
        }
    }
}
