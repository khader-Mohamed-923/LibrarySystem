using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameBorrowToLoanAndAddColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Table was already renamed from 'Loan' to 'Loans' in the DB.
            // Use IF NOT EXISTS to safely handle partially-applied state.

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Loan]', 'U') IS NOT NULL AND OBJECT_ID(N'[Loans]', 'U') IS NULL
                BEGIN
                    EXEC sp_rename N'[Loan]', N'Loans';
                    
                    IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Loan_BookId' AND object_id = OBJECT_ID(N'[Loans]'))
                        EXEC sp_rename N'[Loans].[IX_Loan_BookId]', N'IX_Loans_BookId', N'INDEX';
                        
                    IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Loan_MemberId' AND object_id = OBJECT_ID(N'[Loans]'))
                        EXEC sp_rename N'[Loans].[IX_Loan_MemberId]', N'IX_Loans_MemberId', N'INDEX';
                        
                    IF EXISTS(SELECT * FROM sys.objects WHERE name = 'PK_Loan' AND parent_object_id = OBJECT_ID(N'[Loans]'))
                        EXEC sp_rename N'[PK_Loan]', N'PK_Loans';
                        
                    IF EXISTS(SELECT * FROM sys.objects WHERE name = 'FK_Loan_Books_BookId' AND parent_object_id = OBJECT_ID(N'[Loans]'))
                        EXEC sp_rename N'[FK_Loan_Books_BookId]', N'FK_Loans_Books_BookId';
                        
                    IF EXISTS(SELECT * FROM sys.objects WHERE name = 'FK_Loan_Members_MemberId' AND parent_object_id = OBJECT_ID(N'[Loans]'))
                        EXEC sp_rename N'[FK_Loan_Members_MemberId]', N'FK_Loans_Members_MemberId';
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'OutstandingFine')
                BEGIN
                    ALTER TABLE [Members] ADD [OutstandingFine] decimal(10,2) NOT NULL DEFAULT 0.0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Loans') AND name = 'DueDate')
                BEGIN
                    ALTER TABLE [Loans] ADD [DueDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00';
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Loans') AND name = 'FineAmount')
                BEGIN
                    ALTER TABLE [Loans] ADD [FineAmount] decimal(10,2) NOT NULL DEFAULT 0.0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Loans') AND name = 'IsReturned')
                BEGIN
                    ALTER TABLE [Loans] ADD [IsReturned] bit NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Loans') AND name = 'LoanDate')
                BEGIN
                    ALTER TABLE [Loans] ADD [LoanDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00';
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Loans') AND name = 'ReturnedAt')
                BEGIN
                    ALTER TABLE [Loans] ADD [ReturnedAt] datetime2 NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Books_BookId",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Members_MemberId",
                table: "Loans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Loans",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "OutstandingFine",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "FineAmount",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LoanDate",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "ReturnedAt",
                table: "Loans");

            migrationBuilder.RenameTable(
                name: "Loans",
                newName: "Loan");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_MemberId",
                table: "Loan",
                newName: "IX_Loan_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_BookId",
                table: "Loan",
                newName: "IX_Loan_BookId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Loan",
                table: "Loan",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Loan_Books_BookId",
                table: "Loan",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loan_Members_MemberId",
                table: "Loan",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
