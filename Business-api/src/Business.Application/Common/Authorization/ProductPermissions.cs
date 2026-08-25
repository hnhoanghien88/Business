namespace Business.Application.Common.Authorization;

public static class ProductPermissions
{
    public const string Read = "Products.Read";
    public const string Create = "Products.Create";
    public const string Update = "Products.Update";
    public const string Delete = "Products.Delete";
    public static readonly string[] All = [Read, Create, Update, Delete];
}
