﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Wirehome.Core.History.Repository.Entities;

namespace Wirehome.Core.Migrations
{
    [DbContext(typeof(HistoryDatabaseContext))]
    partial class HistoryDatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Wirehome.Core.History.Repository.Entities.ComponentStatusEntity", b =>
                {
                    b.Property<uint>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ComponentUid")
                        .HasMaxLength(256);

                    b.Property<bool>("IsLatest");

                    b.Property<uint?>("PredecessorID");

                    b.Property<DateTimeOffset>("RangeEnd");

                    b.Property<DateTimeOffset>("RangeStart");

                    b.Property<string>("StatusUid")
                        .HasMaxLength(256);

                    b.Property<string>("Value")
                        .HasMaxLength(1024);

                    b.HasKey("ID");

                    b.HasIndex("PredecessorID");

                    b.HasIndex("RangeStart", "RangeEnd", "ComponentUid", "StatusUid");

                    b.ToTable("ComponentStatus");
                });

            modelBuilder.Entity("Wirehome.Core.History.Repository.Entities.ComponentStatusEntity", b =>
                {
                    b.HasOne("Wirehome.Core.History.Repository.Entities.ComponentStatusEntity", "Predecessor")
                        .WithMany()
                        .HasForeignKey("PredecessorID");
                });
#pragma warning restore 612, 618
        }
    }
}