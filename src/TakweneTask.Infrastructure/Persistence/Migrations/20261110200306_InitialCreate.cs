using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TakweneTask.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "artists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dsps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dsps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ArtistId = table.Column<Guid>(type: "uuid", nullable: false),
                    Isrc = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Genre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tracks_artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "track_distributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    DspId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_track_distributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_track_distributions_dsps_DspId",
                        column: x => x.DspId,
                        principalTable: "dsps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_track_distributions_tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "artists",
                columns: new[] { "Id", "Country", "Email", "Name" },
                values: new object[,]
                {
                    { new Guid("019318a0-0011-7000-8000-000000000001"), "EG", "nour.elsabah@takwene.com", "Nour El Sabah" },
                    { new Guid("019318a0-0012-7000-8000-000000000002"), "LB", "layla.haddad@takwene.com", "Layla Haddad" },
                    { new Guid("019318a0-0013-7000-8000-000000000003"), "SA", "omar.alrashid@takwene.com", "Omar Al-Rashid" },
                    { new Guid("019318a0-0014-7000-8000-000000000004"), "MA", "yasmine.benali@takwene.com", "Yasmine Benali" }
                });

            migrationBuilder.InsertData(
                table: "dsps",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("019318a0-0001-7000-8000-000000000001"), "Spotify" },
                    { new Guid("019318a0-0002-7000-8000-000000000002"), "Apple Music" },
                    { new Guid("019318a0-0003-7000-8000-000000000003"), "YouTube" }
                });

            migrationBuilder.InsertData(
                table: "tracks",
                columns: new[] { "Id", "ArtistId", "Genre", "Isrc", "ReleaseDate", "Status", "Title" },
                values: new object[,]
                {
                    { new Guid("019318a0-0021-7000-8000-000000000001"), new Guid("019318a0-0011-7000-8000-000000000001"), "Mahraganat", "EGA012600101", new DateOnly(2026, 1, 15), "Draft", "Layali El Qahira" },
                    { new Guid("019318a0-0022-7000-8000-000000000002"), new Guid("019318a0-0012-7000-8000-000000000002"), "Arabic Pop", "LBA012600102", new DateOnly(2026, 2, 1), "Draft", "Beirut Bil Lail" },
                    { new Guid("019318a0-0023-7000-8000-000000000003"), new Guid("019318a0-0013-7000-8000-000000000003"), "Khaleeji", "SAA012600103", new DateOnly(2026, 2, 20), "Draft", "Sahra Fel Janoub" },
                    { new Guid("019318a0-0024-7000-8000-000000000004"), new Guid("019318a0-0011-7000-8000-000000000001"), "Shaabi", "EGA012600104", new DateOnly(2026, 3, 1), "Submitted", "Nabd El Mahata" },
                    { new Guid("019318a0-0025-7000-8000-000000000005"), new Guid("019318a0-0012-7000-8000-000000000002"), "Arabic Pop", "LBA012600105", new DateOnly(2026, 3, 10), "Submitted", "Qalb Men Ward" },
                    { new Guid("019318a0-0026-7000-8000-000000000006"), new Guid("019318a0-0014-7000-8000-000000000004"), "Rai", "MAA012600106", new DateOnly(2026, 3, 25), "Submitted", "Ghaymat Sheta" },
                    { new Guid("019318a0-0027-7000-8000-000000000007"), new Guid("019318a0-0011-7000-8000-000000000001"), "Mahraganat", "EGA012600107", new DateOnly(2025, 11, 1), "Distributed", "Darb El Hawa" },
                    { new Guid("019318a0-0028-7000-8000-000000000008"), new Guid("019318a0-0012-7000-8000-000000000002"), "Arabic Pop", "LBA012600108", new DateOnly(2025, 12, 5), "Distributed", "Awtar El Rouh" },
                    { new Guid("019318a0-0029-7000-8000-000000000009"), new Guid("019318a0-0013-7000-8000-000000000003"), "Khaleeji", "SAA012600109", new DateOnly(2026, 1, 1), "Distributed", "Rimal El Majd" },
                    { new Guid("019318a0-002a-7000-8000-000000000010"), new Guid("019318a0-0014-7000-8000-000000000004"), "Hip-Hop", "MAA012600110", new DateOnly(2026, 1, 10), "Distributed", "Sawt El Atlas" }
                });

            migrationBuilder.InsertData(
                table: "track_distributions",
                columns: new[] { "Id", "DspId", "Status", "SubmittedAt", "TrackId" },
                values: new object[,]
                {
                    { new Guid("019318a0-0031-7000-8000-000000000001"), new Guid("019318a0-0001-7000-8000-000000000001"), "Pending", new DateTime(2026, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0024-7000-8000-000000000004") },
                    { new Guid("019318a0-0031-7000-8000-000000000002"), new Guid("019318a0-0002-7000-8000-000000000002"), "Rejected", new DateTime(2026, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0024-7000-8000-000000000004") },
                    { new Guid("019318a0-0032-7000-8000-000000000001"), new Guid("019318a0-0001-7000-8000-000000000001"), "Pending", new DateTime(2026, 3, 10, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0025-7000-8000-000000000005") },
                    { new Guid("019318a0-0032-7000-8000-000000000002"), new Guid("019318a0-0003-7000-8000-000000000003"), "Pending", new DateTime(2026, 3, 10, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0025-7000-8000-000000000005") },
                    { new Guid("019318a0-0033-7000-8000-000000000001"), new Guid("019318a0-0002-7000-8000-000000000002"), "Pending", new DateTime(2026, 3, 25, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0026-7000-8000-000000000006") },
                    { new Guid("019318a0-0034-7000-8000-000000000001"), new Guid("019318a0-0001-7000-8000-000000000001"), "Live", new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0027-7000-8000-000000000007") },
                    { new Guid("019318a0-0034-7000-8000-000000000002"), new Guid("019318a0-0002-7000-8000-000000000002"), "Live", new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0027-7000-8000-000000000007") },
                    { new Guid("019318a0-0034-7000-8000-000000000003"), new Guid("019318a0-0003-7000-8000-000000000003"), "Live", new DateTime(2025, 11, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0027-7000-8000-000000000007") },
                    { new Guid("019318a0-0035-7000-8000-000000000001"), new Guid("019318a0-0001-7000-8000-000000000001"), "Live", new DateTime(2025, 12, 6, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0028-7000-8000-000000000008") },
                    { new Guid("019318a0-0035-7000-8000-000000000002"), new Guid("019318a0-0002-7000-8000-000000000002"), "Live", new DateTime(2025, 12, 6, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0028-7000-8000-000000000008") },
                    { new Guid("019318a0-0036-7000-8000-000000000001"), new Guid("019318a0-0002-7000-8000-000000000002"), "Live", new DateTime(2026, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0029-7000-8000-000000000009") },
                    { new Guid("019318a0-0036-7000-8000-000000000002"), new Guid("019318a0-0003-7000-8000-000000000003"), "Rejected", new DateTime(2026, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-0029-7000-8000-000000000009") },
                    { new Guid("019318a0-0037-7000-8000-000000000001"), new Guid("019318a0-0001-7000-8000-000000000001"), "Live", new DateTime(2026, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-002a-7000-8000-000000000010") },
                    { new Guid("019318a0-0037-7000-8000-000000000002"), new Guid("019318a0-0003-7000-8000-000000000003"), "Live", new DateTime(2026, 1, 10, 10, 0, 0, 0, DateTimeKind.Utc), new Guid("019318a0-002a-7000-8000-000000000010") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_artists_Email",
                table: "artists",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dsps_Name",
                table: "dsps",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_track_distributions_DspId",
                table: "track_distributions",
                column: "DspId");

            migrationBuilder.CreateIndex(
                name: "IX_track_distributions_TrackId_DspId",
                table: "track_distributions",
                columns: new[] { "TrackId", "DspId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tracks_ArtistId",
                table: "tracks",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_tracks_Isrc",
                table: "tracks",
                column: "Isrc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "track_distributions");

            migrationBuilder.DropTable(
                name: "dsps");

            migrationBuilder.DropTable(
                name: "tracks");

            migrationBuilder.DropTable(
                name: "artists");
        }
    }
}
