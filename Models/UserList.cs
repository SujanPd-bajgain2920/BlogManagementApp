using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogManagementApp.Models;

public partial class UserList
{
    public short UserId { get; set; }



    public string FullName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string UsePhoto { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public string CurrentAddress { get; set; } = null!;

    public virtual ICollection<BlogContent> BlogContentCancelUsers { get; set; } = new List<BlogContent>();

    public virtual ICollection<BlogContent> BlogContentUploadUsers { get; set; } = new List<BlogContent>();
}
