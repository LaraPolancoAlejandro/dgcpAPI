﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using dgcp.infrastructure;

#nullable disable

namespace dgcp.infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("dgcp.domain.Models.Empresa", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("NombreEmpresa")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Empresas");
                });

            modelBuilder.Entity("dgcp.domain.Models.EmpresaKeyword", b =>
                {
                    b.Property<int>("EmpresaID")
                        .HasColumnType("int");

                    b.Property<int>("KeywordID")
                        .HasColumnType("int");

                    b.HasKey("EmpresaID", "KeywordID");

                    b.HasIndex("KeywordID");

                    b.ToTable("EmpresaKeywords");
                });

            modelBuilder.Entity("dgcp.domain.Models.EmpresaRubro", b =>
                {
                    b.Property<int>("EmpresaID")
                        .HasColumnType("int");

                    b.Property<int>("RubroID")
                        .HasColumnType("int");

                    b.HasKey("EmpresaID", "RubroID");

                    b.HasIndex("RubroID");

                    b.ToTable("EmpresaRubros");
                });

            modelBuilder.Entity("dgcp.domain.Models.Keyword", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("Palabra")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Keywords");
                });

            modelBuilder.Entity("dgcp.domain.Models.Rubro", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("CodigoRubro")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Rubros");
                });

            modelBuilder.Entity("dgcp.domain.Models.Tender", b =>
                {
                    b.Property<string>("ReleaseOcid")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DocumentUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmpresaIds")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ProcuringEntity")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("PublicationPolicy")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime?>("PublishedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Publisher")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("ReleaseId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("TenderId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("ReleaseOcid");

                    b.ToTable("Tenders");
                });

            modelBuilder.Entity("dgcp.domain.Models.TenderFinal", b =>
                {
                    b.Property<string>("ReleaseOcid")
                        .HasColumnType("nvarchar(450)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Currency")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DocumentUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmpresaIds")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ProcuringEntity")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PublicationPolicy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("PublishedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Publisher")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReleaseId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenderId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ReleaseOcid");

                    b.ToTable("TendersFinal");
                });

            modelBuilder.Entity("dgcp.domain.Models.TenderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Classification")
                        .HasColumnType("int");

                    b.Property<Guid>("TenderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("TenderReleaseOcid")
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("TenderReleaseOcid");

                    b.ToTable("TenderItem");
                });

            modelBuilder.Entity("dgcp.domain.Models.VisitedUrl", b =>
                {
                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("VisitDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Url");

                    b.ToTable("VisitedUrls");
                });

            modelBuilder.Entity("dgcp.domain.Models.EmpresaKeyword", b =>
                {
                    b.HasOne("dgcp.domain.Models.Empresa", "Empresa")
                        .WithMany("EmpresaKeywords")
                        .HasForeignKey("EmpresaID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("dgcp.domain.Models.Keyword", "Keyword")
                        .WithMany("EmpresaKeywords")
                        .HasForeignKey("KeywordID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Empresa");

                    b.Navigation("Keyword");
                });

            modelBuilder.Entity("dgcp.domain.Models.EmpresaRubro", b =>
                {
                    b.HasOne("dgcp.domain.Models.Empresa", "Empresa")
                        .WithMany("EmpresaRubros")
                        .HasForeignKey("EmpresaID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("dgcp.domain.Models.Rubro", "Rubro")
                        .WithMany("EmpresaRubros")
                        .HasForeignKey("RubroID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Empresa");

                    b.Navigation("Rubro");
                });

            modelBuilder.Entity("dgcp.domain.Models.TenderItem", b =>
                {
                    b.HasOne("dgcp.domain.Models.Tender", "Tender")
                        .WithMany("Items")
                        .HasForeignKey("TenderReleaseOcid");

                    b.Navigation("Tender");
                });

            modelBuilder.Entity("dgcp.domain.Models.Empresa", b =>
                {
                    b.Navigation("EmpresaKeywords");

                    b.Navigation("EmpresaRubros");
                });

            modelBuilder.Entity("dgcp.domain.Models.Keyword", b =>
                {
                    b.Navigation("EmpresaKeywords");
                });

            modelBuilder.Entity("dgcp.domain.Models.Rubro", b =>
                {
                    b.Navigation("EmpresaRubros");
                });

            modelBuilder.Entity("dgcp.domain.Models.Tender", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
