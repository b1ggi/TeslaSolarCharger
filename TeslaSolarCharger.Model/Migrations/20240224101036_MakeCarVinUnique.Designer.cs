﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TeslaSolarCharger.Model.EntityFramework;

#nullable disable

namespace TeslaSolarCharger.Model.Migrations
{
    [DbContext(typeof(TeslaSolarChargerContext))]
    [Migration("20240224101036_MakeCarVinUnique")]
    partial class MakeCarVinUnique
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.CachedCarState", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CarId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CarStateJson")
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("CachedCarStates");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChargeMode")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChargingPriority")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IgnoreLatestTimeToReachSocDate")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LatestTimeToReachSoC")
                        .HasColumnType("TEXT");

                    b.Property<int>("MaximumAmpere")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MinimumAmpere")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MinimumSoc")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<bool?>("ShouldBeManaged")
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("ShouldSetChargeStartTimes")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TeslaFleetApiState")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TeslaMateCarId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UsableEnergy")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Vin")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TeslaMateCarId")
                        .IsUnique();

                    b.HasIndex("Vin")
                        .IsUnique();

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.ChargePrice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AddSpotPriceToGridPrice")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EnergyProvider")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(6);

                    b.Property<string>("EnergyProviderConfiguration")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("GridPrice")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SolarPrice")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SpotPriceCorrectionFactor")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ValidSince")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ChargePrices");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.HandledCharge", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal?>("AverageSpotPrice")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("CalculatedPrice")
                        .HasColumnType("TEXT");

                    b.Property<int>("CarId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChargingProcessId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal?>("UsedGridEnergy")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("UsedSolarEnergy")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("HandledCharges");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.PowerDistribution", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChargingPower")
                        .HasColumnType("INTEGER");

                    b.Property<float>("GridProportion")
                        .HasColumnType("REAL");

                    b.Property<int>("HandledChargeId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PowerFromGrid")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("TEXT");

                    b.Property<float?>("UsedWattHours")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("HandledChargeId");

                    b.ToTable("PowerDistributions");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.SpotPrice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SpotPrices");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.TeslaToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpiresAtUtc")
                        .HasColumnType("TEXT");

                    b.Property<string>("IdToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Region")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UnauthorizedCounter")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("TeslaTokens");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.TscConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("TscConfigurations");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.PowerDistribution", b =>
                {
                    b.HasOne("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.HandledCharge", "HandledCharge")
                        .WithMany("PowerDistributions")
                        .HasForeignKey("HandledChargeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HandledCharge");
                });

            modelBuilder.Entity("TeslaSolarCharger.Model.Entities.TeslaSolarCharger.HandledCharge", b =>
                {
                    b.Navigation("PowerDistributions");
                });
#pragma warning restore 612, 618
        }
    }
}
