using MongoDB.Bson.Serialization.Attributes;

namespace AuthService.Domain.Entities.User;

public enum RoleEnum
{
    Buyer = 1,
    Seller = 2
}

public sealed class UserRole
{
    [BsonElement]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public string Value { get; }

    public UserRole(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public bool Equals(RoleEnum? other)
    {
        if (other == null)
            return false;
        return Value.CompareTo(other.Value) > -1;
    }
    
    public override bool Equals(object? obj) => obj is RoleEnum u && Equals(u);
    public override int GetHashCode() => Value.GetHashCode();
}