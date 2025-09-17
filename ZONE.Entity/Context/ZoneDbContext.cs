using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ZONE.Entity.Model;

namespace ZONE.Entity.Context;

public partial class ZoneDbContext : DbContext
{
    public ZoneDbContext()
    {
    }

    public ZoneDbContext(DbContextOptions<ZoneDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CameraDetail> CameraDetails { get; set; }

    public virtual DbSet<EventDetail> EventDetails { get; set; }

    public virtual DbSet<ObjectType> ObjectTypes { get; set; }

    public virtual DbSet<UserDetail> UserDetails { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CameraDetail>(entity =>
        {
            entity.HasKey(e => e.CameraId);

            entity.ToTable("Camera_Details");

            entity.Property(e => e.CameraId).HasColumnName("Camera_Id");
            entity.Property(e => e.CameraIpAddress)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Camera_IpAddress");
            entity.Property(e => e.CameraName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Camera_Name");
            entity.Property(e => e.CameraPassword)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Camera_Password");
            entity.Property(e => e.CameraPort)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("Camera_Port");
            entity.Property(e => e.CameraSubstream)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Camera_Substream");
            entity.Property(e => e.CameraUserName)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Camera_UserName");
            entity.Property(e => e.Roicoordinates)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("ROICoordinates");
            entity.Property(e => e.RoiendPercentageHeight).HasColumnName("ROIEndPercentageHeight");
            entity.Property(e => e.RoiendPercentageWidth).HasColumnName("ROIEndPercentageWidth");
            entity.Property(e => e.RoistartPercentageHeight).HasColumnName("ROIStartPercentageHeight");
            entity.Property(e => e.RoistartPercentageWidth).HasColumnName("ROIStartPercentageWidth");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("URL");
        });

        modelBuilder.Entity<EventDetail>(entity =>
        {
            entity.HasKey(e => e.EventId);

            entity.ToTable("Event_Details");

            entity.Property(e => e.EventId).HasColumnName("Event_Id");
            entity.Property(e => e.CameraId).HasColumnName("Camera_Id");
            entity.Property(e => e.CameraName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Camera_Name");
            entity.Property(e => e.EventTime)
                .HasColumnType("datetime")
                .HasColumnName("Event_Time");
            entity.Property(e => e.Img).IsUnicode(false);
            entity.Property(e => e.Link)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ObjectType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Object_Type");
        });

        modelBuilder.Entity<ObjectType>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Object)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("User_Details");

            entity.Property(e => e.UserId).HasColumnName("User_Id");
            entity.Property(e => e.EmailId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Email_Id");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("Phone_Number");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("User_Name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
