﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tailwind.Trader.Auction.Api.Infrastucture;

namespace Tailwind.Trader.Auction.Api.Migrations
{
    [DbContext(typeof(AuctionContext))]
    partial class AuctionContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.11-servicing-32099")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Tailwind.Trader.Auction.Api.Models.BidHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BidderId");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<decimal>("Price");

                    b.Property<int>("ProductId");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("BidHistories");
                });

            modelBuilder.Entity("Tailwind.Trader.Auction.Api.Models.Detail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ProductId");

                    b.Property<string>("Topic");

                    b.Property<string>("TopicDetail");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("Details");
                });

            modelBuilder.Entity("Tailwind.Trader.Auction.Api.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AuctionStatus");

                    b.Property<DateTime>("CreatedDateTme")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<DateTime>("Expired")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<int>("HighestBidder");

                    b.Property<string>("Name");

                    b.Property<int>("PaidStatus");

                    b.Property<decimal>("Price");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Tailwind.Trader.Auction.Api.Models.ProductImagePath", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ImagePath");

                    b.Property<int>("ProductId");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductImagePaths");
                });

            modelBuilder.Entity("Tailwind.Trader.Auction.Api.Models.BidHistory", b =>
                {
                    b.HasOne("Tailwind.Trader.Auction.Api.Models.Product", "Product")
                        .WithMany("BidHistories")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tailwind.Trader.Auction.Api.Models.Detail", b =>
                {
                    b.HasOne("Tailwind.Trader.Auction.Api.Models.Product")
                        .WithMany("Details")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Tailwind.Trader.Auction.Api.Models.ProductImagePath", b =>
                {
                    b.HasOne("Tailwind.Trader.Auction.Api.Models.Product")
                        .WithMany("ProductImages")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
