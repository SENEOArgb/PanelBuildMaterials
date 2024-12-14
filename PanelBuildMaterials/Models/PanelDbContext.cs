using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PanelBuildMaterials.Models;

public partial class PanelDbContext : DbContext
{
    public PanelDbContext()
    {
    }

    public PanelDbContext(DbContextOptions<PanelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BuildingMaterial> BuildingMaterials { get; set; }

    public virtual DbSet<BuildingMaterialsServicesOrder> BuildingMaterialsServicesOrders { get; set; }

    public virtual DbSet<BuildingMaterialsWarehouse> BuildingMaterialsWarehouses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=NEONOVIYY\\SQLEXPRESS;Database=PanelBuildMaterial;Trusted_Connection=True;Encrypt=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuildingMaterial>(entity =>
        {
            entity.Property(e => e.BuildingMaterialId).HasColumnName("buildingMaterialID");
            entity.Property(e => e.CategoryId).HasColumnName("categoryId");
            entity.Property(e => e.NameBuildingMaterial)
                .HasMaxLength(300)
                .HasColumnName("nameBuildingMaterial");
            entity.Property(e => e.RetailPrice)
                .HasColumnType("decimal(8, 0)")
                .HasColumnName("retailPrice");
            entity.Property(e => e.WholesalePrice)
                .HasColumnType("decimal(8, 0)")
                .HasColumnName("wholesalePrice");

            entity.HasOne(d => d.Category).WithMany(p => p.BuildingMaterials)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuildingMaterials_Categories1");
        });

        modelBuilder.Entity<BuildingMaterialsServicesOrder>(entity =>
        {
            entity.HasKey(e => e.BuildingMaterialServiceOrderId);

            entity.Property(e => e.BuildingMaterialServiceOrderId).HasColumnName("BuildingMaterialServiceOrderID");
            entity.Property(e => e.BuildingMaterialId).HasColumnName("buildingMaterialId");
            entity.Property(e => e.CountBuildingMaterial).HasColumnName("countBuildingMaterial");
            entity.Property(e => e.OrderId).HasColumnName("orderId");
            entity.Property(e => e.OrderPrice)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("orderPrice");
            entity.Property(e => e.ServiceId).HasColumnName("serviceId");

            entity.HasOne(d => d.BuildingMaterial).WithMany(p => p.BuildingMaterialsServicesOrders)
                .HasForeignKey(d => d.BuildingMaterialId)
                .HasConstraintName("FK_BuildingMaterialsServicesOrders_BuildingMaterials1");

            entity.HasOne(d => d.Order).WithMany(p => p.BuildingMaterialsServicesOrders)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_BuildingMaterialsServicesOrders_Orders1");

            entity.HasOne(d => d.Service).WithMany(p => p.BuildingMaterialsServicesOrders)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK_BuildingMaterialsServicesOrders_Services1");
        });

        modelBuilder.Entity<BuildingMaterialsWarehouse>(entity =>
        {
            entity.HasKey(e => e.BuildingMaterialWarehouseId);

            entity.Property(e => e.BuildingMaterialWarehouseId).HasColumnName("buildingMaterialWarehouseID");
            entity.Property(e => e.AmountBuildingMaterial).HasColumnName("amountBuildingMaterial");
            entity.Property(e => e.BuildingMaterialId).HasColumnName("buildingMaterialId");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouseId");

            entity.HasOne(d => d.BuildingMaterial).WithMany(p => p.BuildingMaterialsWarehouses)
                .HasForeignKey(d => d.BuildingMaterialId)
                .HasConstraintName("FK_BuildingMaterialsWarehouses_BuildingMaterials");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.BuildingMaterialsWarehouses)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuildingMaterialsWarehouses_Warehouses1");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CategoryId).HasColumnName("categoryID");
            entity.Property(e => e.NameCategory)
                .HasMaxLength(100)
                .HasColumnName("nameCategory");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.Property(e => e.LogId).HasColumnName("logID");
            entity.Property(e => e.DateTimeLog)
                .HasColumnType("datetime")
                .HasColumnName("dateTimeLog");
            entity.Property(e => e.LogDescription)
                .HasMaxLength(550)
                .HasColumnName("logDescription");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Logs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Logs_Users");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.OrderId).HasColumnName("orderID");
            entity.Property(e => e.DateOrder).HasColumnName("dateOrder");
            entity.Property(e => e.TimeOrder)
                .HasPrecision(2)
                .HasColumnName("timeOrder");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Orders_Users1");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.ServiceId).HasColumnName("serviceID");
            entity.Property(e => e.NameService)
                .HasMaxLength(100)
                .HasColumnName("nameService");
            entity.Property(e => e.PriceService)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("priceService");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnName("userID");
            entity.Property(e => e.UserLaws)
                .HasMaxLength(75)
                .HasColumnName("userLaws");
            entity.Property(e => e.UserLogin)
                .HasMaxLength(100)
                .HasColumnName("userLogin");
            entity.Property(e => e.UserPasswordHash)
                .HasMaxLength(300)
                .HasColumnName("userPasswordHash");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.Property(e => e.WarehouseId).HasColumnName("warehouseID");
            entity.Property(e => e.WarehouseCity)
                .HasMaxLength(100)
                .HasColumnName("warehouseCity");
            entity.Property(e => e.WarehouseName)
                .HasMaxLength(100)
                .HasColumnName("warehouseName");
            entity.Property(e => e.WarehouseStreet)
                .HasMaxLength(100)
                .HasColumnName("warehouseStreet");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
