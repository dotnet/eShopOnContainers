using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identity.API.Migrations.ConfigurationDb
{
    public partial class InitialConfigurationMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AllowedAccessTokenSigningAlgorithms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShowInDiscoveryDocument = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAccessed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NonEditable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Required = table.Column<bool>(type: "bit", nullable: false),
                    Emphasize = table.Column<bool>(type: "bit", nullable: false),
                    ShowInDiscoveryDocument = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProtocolType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequireClientSecret = table.Column<bool>(type: "bit", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClientUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LogoUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequireConsent = table.Column<bool>(type: "bit", nullable: false),
                    AllowRememberConsent = table.Column<bool>(type: "bit", nullable: false),
                    AlwaysIncludeUserClaimsInIdToken = table.Column<bool>(type: "bit", nullable: false),
                    RequirePkce = table.Column<bool>(type: "bit", nullable: false),
                    AllowPlainTextPkce = table.Column<bool>(type: "bit", nullable: false),
                    RequireRequestObject = table.Column<bool>(type: "bit", nullable: false),
                    AllowAccessTokensViaBrowser = table.Column<bool>(type: "bit", nullable: false),
                    FrontChannelLogoutUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FrontChannelLogoutSessionRequired = table.Column<bool>(type: "bit", nullable: false),
                    BackChannelLogoutUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BackChannelLogoutSessionRequired = table.Column<bool>(type: "bit", nullable: false),
                    AllowOfflineAccess = table.Column<bool>(type: "bit", nullable: false),
                    IdentityTokenLifetime = table.Column<int>(type: "int", nullable: false),
                    AllowedIdentityTokenSigningAlgorithms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccessTokenLifetime = table.Column<int>(type: "int", nullable: false),
                    AuthorizationCodeLifetime = table.Column<int>(type: "int", nullable: false),
                    ConsentLifetime = table.Column<int>(type: "int", nullable: true),
                    AbsoluteRefreshTokenLifetime = table.Column<int>(type: "int", nullable: false),
                    SlidingRefreshTokenLifetime = table.Column<int>(type: "int", nullable: false),
                    RefreshTokenUsage = table.Column<int>(type: "int", nullable: false),
                    UpdateAccessTokenClaimsOnRefresh = table.Column<bool>(type: "bit", nullable: false),
                    RefreshTokenExpiration = table.Column<int>(type: "int", nullable: false),
                    AccessTokenType = table.Column<int>(type: "int", nullable: false),
                    EnableLocalLogin = table.Column<bool>(type: "bit", nullable: false),
                    IncludeJwtId = table.Column<bool>(type: "bit", nullable: false),
                    AlwaysSendClientClaims = table.Column<bool>(type: "bit", nullable: false),
                    ClientClaimsPrefix = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PairWiseSubjectSalt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAccessed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserSsoLifetime = table.Column<int>(type: "int", nullable: true),
                    UserCodeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeviceCodeLifetime = table.Column<int>(type: "int", nullable: false),
                    NonEditable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Required = table.Column<bool>(type: "bit", nullable: false),
                    Emphasize = table.Column<bool>(type: "bit", nullable: false),
                    ShowInDiscoveryDocument = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NonEditable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiResourceClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiResourceId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResourceClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiResourceClaims_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiResourceProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiResourceId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResourceProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiResourceProperties_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiResourceScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scope = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiResourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResourceScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiResourceScopes_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiResourceSecrets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiResourceId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResourceSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiResourceSecrets_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiScopeClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScopeId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopeClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiScopeClaims_ApiScopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "ApiScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiScopeProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScopeId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopeProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiScopeProperties_ApiScopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "ApiScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientClaims_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientCorsOrigins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Origin = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCorsOrigins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientCorsOrigins_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientGrantTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrantType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientGrantTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientGrantTypes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientIdPRestrictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientIdPRestrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientIdPRestrictions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientPostLogoutRedirectUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostLogoutRedirectUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPostLogoutRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientPostLogoutRedirectUris_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProperties_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientRedirectUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RedirectUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRedirectUris_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scope = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientScopes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientSecrets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSecrets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityResourceClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityResourceId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityResourceClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityResourceClaims_IdentityResources_IdentityResourceId",
                        column: x => x.IdentityResourceId,
                        principalTable: "IdentityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityResourceProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityResourceId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityResourceProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityResourceProperties_IdentityResources_IdentityResourceId",
                        column: x => x.IdentityResourceId,
                        principalTable: "IdentityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceClaims_ApiResourceId",
                table: "ApiResourceClaims",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceProperties_ApiResourceId",
                table: "ApiResourceProperties",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResources_Name",
                table: "ApiResources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceScopes_ApiResourceId",
                table: "ApiResourceScopes",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResourceSecrets_ApiResourceId",
                table: "ApiResourceSecrets",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopeClaims_ScopeId",
                table: "ApiScopeClaims",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopeProperties_ScopeId",
                table: "ApiScopeProperties",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopes_Name",
                table: "ApiScopes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientClaims_ClientId",
                table: "ClientClaims",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCorsOrigins_ClientId",
                table: "ClientCorsOrigins",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGrantTypes_ClientId",
                table: "ClientGrantTypes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientIdPRestrictions_ClientId",
                table: "ClientIdPRestrictions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPostLogoutRedirectUris_ClientId",
                table: "ClientPostLogoutRedirectUris",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProperties_ClientId",
                table: "ClientProperties",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRedirectUris_ClientId",
                table: "ClientRedirectUris",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ClientId",
                table: "Clients",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientScopes_ClientId",
                table: "ClientScopes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSecrets_ClientId",
                table: "ClientSecrets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityResourceClaims_IdentityResourceId",
                table: "IdentityResourceClaims",
                column: "IdentityResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityResourceProperties_IdentityResourceId",
                table: "IdentityResourceProperties",
                column: "IdentityResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityResources_Name",
                table: "IdentityResources",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiResourceClaims");

            migrationBuilder.DropTable(
                name: "ApiResourceProperties");

            migrationBuilder.DropTable(
                name: "ApiResourceScopes");

            migrationBuilder.DropTable(
                name: "ApiResourceSecrets");

            migrationBuilder.DropTable(
                name: "ApiScopeClaims");

            migrationBuilder.DropTable(
                name: "ApiScopeProperties");

            migrationBuilder.DropTable(
                name: "ClientClaims");

            migrationBuilder.DropTable(
                name: "ClientCorsOrigins");

            migrationBuilder.DropTable(
                name: "ClientGrantTypes");

            migrationBuilder.DropTable(
                name: "ClientIdPRestrictions");

            migrationBuilder.DropTable(
                name: "ClientPostLogoutRedirectUris");

            migrationBuilder.DropTable(
                name: "ClientProperties");

            migrationBuilder.DropTable(
                name: "ClientRedirectUris");

            migrationBuilder.DropTable(
                name: "ClientScopes");

            migrationBuilder.DropTable(
                name: "ClientSecrets");

            migrationBuilder.DropTable(
                name: "IdentityResourceClaims");

            migrationBuilder.DropTable(
                name: "IdentityResourceProperties");

            migrationBuilder.DropTable(
                name: "ApiResources");

            migrationBuilder.DropTable(
                name: "ApiScopes");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "IdentityResources");
        }
    }
}
