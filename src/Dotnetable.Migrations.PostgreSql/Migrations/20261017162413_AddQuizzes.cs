using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizCategoryID",
                table: "MenuItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuizID",
                table: "MenuItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuizCategories",
                columns: table => new
                {
                    QuizCategoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    ParentCategoryID = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ShowInPosts = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCategories", x => x.QuizCategoryID);
                    table.ForeignKey(
                        name: "FK_QuizCategories_QuizCategories_ParentCategoryID",
                        column: x => x.ParentCategoryID,
                        principalTable: "QuizCategories",
                        principalColumn: "QuizCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizCategories_Websites_WebsiteID",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizCategoryTranslations",
                columns: table => new
                {
                    QuizCategoryTranslationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizCategoryID = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizCategoryTranslations", x => x.QuizCategoryTranslationID);
                    table.ForeignKey(
                        name: "FK_QuizCategoryTranslations_QuizCategories_QuizCategoryID",
                        column: x => x.QuizCategoryID,
                        principalTable: "QuizCategories",
                        principalColumn: "QuizCategoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    QuizID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    QuizCategoryID = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    QuestionMode = table.Column<byte>(type: "smallint", nullable: false),
                    RandomCount = table.Column<int>(type: "integer", nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    OfferPrivateReview = table.Column<bool>(type: "boolean", nullable: false),
                    PrivateReviewFee = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", unicode: false, maxLength: 3, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.QuizID);
                    table.ForeignKey(
                        name: "FK_Quizzes_QuizCategories_QuizCategoryID",
                        column: x => x.QuizCategoryID,
                        principalTable: "QuizCategories",
                        principalColumn: "QuizCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quizzes_Websites_WebsiteID",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    QuizAttemptID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizID = table.Column<int>(type: "integer", nullable: false),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "integer", nullable: true),
                    PublicToken = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    BandTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BandBody = table.Column<string>(type: "text", nullable: true),
                    WantsPrivateReview = table.Column<bool>(type: "boolean", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PrivateReviewStatus = table.Column<byte>(type: "smallint", nullable: false),
                    PrivateReviewFee = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", unicode: false, maxLength: 3, nullable: true),
                    PaymentID = table.Column<int>(type: "integer", nullable: true),
                    PaymentGatewayID = table.Column<int>(type: "integer", nullable: true),
                    GatewayAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GatewayRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    PrivateAnswer = table.Column<string>(type: "text", nullable: true),
                    PrivateAnswerAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    LanguageCode = table.Column<string>(type: "character(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.QuizAttemptID);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_PaymentGateways_PaymentGatewayID",
                        column: x => x.PaymentGatewayID,
                        principalTable: "PaymentGateways",
                        principalColumn: "PaymentGatewayID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Payments_PaymentID",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "PaymentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_WebsiteClients_WebsiteClientID",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Websites_WebsiteID",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    QuizQuestionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizID = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    AllowMultiple = table.Column<bool>(type: "boolean", nullable: false),
                    AllowText = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.QuizQuestionID);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizScoreBands",
                columns: table => new
                {
                    QuizScoreBandID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizID = table.Column<int>(type: "integer", nullable: false),
                    MinPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MaxPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizScoreBands", x => x.QuizScoreBandID);
                    table.ForeignKey(
                        name: "FK_QuizScoreBands_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizTranslations",
                columns: table => new
                {
                    QuizTranslationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizID = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizTranslations", x => x.QuizTranslationID);
                    table.ForeignKey(
                        name: "FK_QuizTranslations_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttemptAnswers",
                columns: table => new
                {
                    QuizAttemptAnswerID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizAttemptID = table.Column<int>(type: "integer", nullable: false),
                    QuizQuestionID = table.Column<int>(type: "integer", nullable: false),
                    TextAnswer = table.Column<string>(type: "text", nullable: true),
                    PointsAwarded = table.Column<decimal>(type: "numeric(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptAnswers", x => x.QuizAttemptAnswerID);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizAttempts_QuizAttemptID",
                        column: x => x.QuizAttemptID,
                        principalTable: "QuizAttempts",
                        principalColumn: "QuizAttemptID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizQuestions_QuizQuestionID",
                        column: x => x.QuizQuestionID,
                        principalTable: "QuizQuestions",
                        principalColumn: "QuizQuestionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttemptQuestions",
                columns: table => new
                {
                    QuizAttemptID = table.Column<int>(type: "integer", nullable: false),
                    QuizQuestionID = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptQuestions", x => new { x.QuizAttemptID, x.QuizQuestionID });
                    table.ForeignKey(
                        name: "FK_QuizAttemptQuestions_QuizAttempts_QuizAttemptID",
                        column: x => x.QuizAttemptID,
                        principalTable: "QuizAttempts",
                        principalColumn: "QuizAttemptID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttemptQuestions_QuizQuestions_QuizQuestionID",
                        column: x => x.QuizQuestionID,
                        principalTable: "QuizQuestions",
                        principalColumn: "QuizQuestionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizOptions",
                columns: table => new
                {
                    QuizOptionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizQuestionID = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizOptions", x => x.QuizOptionID);
                    table.ForeignKey(
                        name: "FK_QuizOptions_QuizQuestions_QuizQuestionID",
                        column: x => x.QuizQuestionID,
                        principalTable: "QuizQuestions",
                        principalColumn: "QuizQuestionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestionImages",
                columns: table => new
                {
                    QuizQuestionImageID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizQuestionID = table.Column<int>(type: "integer", nullable: false),
                    FileRecordID = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestionImages", x => x.QuizQuestionImageID);
                    table.ForeignKey(
                        name: "FK_QuizQuestionImages_FileRecords_FileRecordID",
                        column: x => x.FileRecordID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizQuestionImages_QuizQuestions_QuizQuestionID",
                        column: x => x.QuizQuestionID,
                        principalTable: "QuizQuestions",
                        principalColumn: "QuizQuestionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizScoreBandTranslations",
                columns: table => new
                {
                    QuizScoreBandTranslationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizScoreBandID = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizScoreBandTranslations", x => x.QuizScoreBandTranslationID);
                    table.ForeignKey(
                        name: "FK_QuizScoreBandTranslations_QuizScoreBands_QuizScoreBandID",
                        column: x => x.QuizScoreBandID,
                        principalTable: "QuizScoreBands",
                        principalColumn: "QuizScoreBandID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttemptAnswerOptions",
                columns: table => new
                {
                    QuizAttemptAnswerID = table.Column<int>(type: "integer", nullable: false),
                    QuizOptionID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptAnswerOptions", x => new { x.QuizAttemptAnswerID, x.QuizOptionID });
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswerOptions_QuizAttemptAnswers_QuizAttemptAnsw~",
                        column: x => x.QuizAttemptAnswerID,
                        principalTable: "QuizAttemptAnswers",
                        principalColumn: "QuizAttemptAnswerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswerOptions_QuizOptions_QuizOptionID",
                        column: x => x.QuizOptionID,
                        principalTable: "QuizOptions",
                        principalColumn: "QuizOptionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_QuizCategoryID",
                table: "MenuItems",
                column: "QuizCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_QuizID",
                table: "MenuItems",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswerOptions_QuizOptionID",
                table: "QuizAttemptAnswerOptions",
                column: "QuizOptionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_QuizAttemptID",
                table: "QuizAttemptAnswers",
                column: "QuizAttemptID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_QuizQuestionID",
                table: "QuizAttemptAnswers",
                column: "QuizQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptQuestions_QuizQuestionID",
                table: "QuizAttemptQuestions",
                column: "QuizQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_GatewayAuthority",
                table: "QuizAttempts",
                column: "GatewayAuthority");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_PaymentGatewayID",
                table: "QuizAttempts",
                column: "PaymentGatewayID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_PaymentID",
                table: "QuizAttempts",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizID",
                table: "QuizAttempts",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_WebsiteClientID",
                table: "QuizAttempts",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_WebsiteID",
                table: "QuizAttempts",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_QuizAttempts_PublicToken",
                table: "QuizAttempts",
                column: "PublicToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizCategories_ParentCategoryID",
                table: "QuizCategories",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "UQ_QuizCategories_WebsiteID_Slug",
                table: "QuizCategories",
                columns: new[] { "WebsiteID", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_QuizCategoryTranslations_Category_Language",
                table: "QuizCategoryTranslations",
                columns: new[] { "QuizCategoryID", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuizQuestionID",
                table: "QuizOptions",
                column: "QuizQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestionImages_FileRecordID",
                table: "QuizQuestionImages",
                column: "FileRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestionImages_QuizQuestionID",
                table: "QuizQuestionImages",
                column: "QuizQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizID",
                table: "QuizQuestions",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizScoreBands_QuizID",
                table: "QuizScoreBands",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "UQ_QuizScoreBandTranslations_Band_Language",
                table: "QuizScoreBandTranslations",
                columns: new[] { "QuizScoreBandID", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_QuizTranslations_Quiz_Language",
                table: "QuizTranslations",
                columns: new[] { "QuizID", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_QuizCategoryID",
                table: "Quizzes",
                column: "QuizCategoryID");

            migrationBuilder.CreateIndex(
                name: "UQ_Quizzes_WebsiteID_Slug",
                table: "Quizzes",
                columns: new[] { "WebsiteID", "Slug" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_QuizCategories_QuizCategoryID",
                table: "MenuItems",
                column: "QuizCategoryID",
                principalTable: "QuizCategories",
                principalColumn: "QuizCategoryID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Quizzes_QuizID",
                table: "MenuItems",
                column: "QuizID",
                principalTable: "Quizzes",
                principalColumn: "QuizID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_QuizCategories_QuizCategoryID",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Quizzes_QuizID",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "QuizAttemptAnswerOptions");

            migrationBuilder.DropTable(
                name: "QuizAttemptQuestions");

            migrationBuilder.DropTable(
                name: "QuizCategoryTranslations");

            migrationBuilder.DropTable(
                name: "QuizQuestionImages");

            migrationBuilder.DropTable(
                name: "QuizScoreBandTranslations");

            migrationBuilder.DropTable(
                name: "QuizTranslations");

            migrationBuilder.DropTable(
                name: "QuizAttemptAnswers");

            migrationBuilder.DropTable(
                name: "QuizOptions");

            migrationBuilder.DropTable(
                name: "QuizScoreBands");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "QuizCategories");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_QuizCategoryID",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_QuizID",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "QuizCategoryID",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "QuizID",
                table: "MenuItems");
        }
    }
}
