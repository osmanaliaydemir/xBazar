using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Favorite : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public FavoriteType Type { get; set; }
    
    [Required]
    public Guid ItemId { get; set; } // ProductId or StoreId
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Product? Product { get; set; }
    public virtual Store? Store { get; set; }
}

public enum FavoriteType
{
    Product = 0,
    Store = 1
}
