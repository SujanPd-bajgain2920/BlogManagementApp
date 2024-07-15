using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementApp.Models;

public partial class BlogManagementSystemContext : DbContext
{
    public BlogManagementSystemContext()
    {
    }

    public BlogManagementSystemContext(DbContextOptions<BlogManagementSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogContent> BlogContents { get; set; }

    public virtual DbSet<UserList> UserLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=dbConn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogContent>(entity =>
        {
            entity.HasKey(e => e.Bid).HasName("PK__BlogCont__C6DE0CC13D97A41F");

            entity.ToTable("BlogContent");

            entity.Property(e => e.Bid)
                .ValueGeneratedNever()
                .HasColumnName("BId");

            entity.HasOne(d => d.CancelUser).WithMany(p => p.BlogContentCancelUsers)
                .HasForeignKey(d => d.CancelUserId)
                .HasConstraintName("FK__BlogConte__Cance__403A8C7D");

            entity.HasOne(d => d.UploadUser).WithMany(p => p.BlogContentUploadUsers)
                .HasForeignKey(d => d.UploadUserId)
                .HasConstraintName("FK__BlogConte__Uploa__3F466844");
        });

        modelBuilder.Entity<UserList>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserList__1788CC4C1E8ED714");

            entity.ToTable("UserList");

            entity.HasIndex(e => e.EmailAddress, "UQ__UserList__49A14740C12B5B85").IsUnique();

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.CurrentAddress).HasMaxLength(50);
            entity.Property(e => e.EmailAddress).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.UserRole).HasMaxLength(40);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
