namespace Business.Application.Common.Authorization;

public static class ProductPermissions
{
    public const string Read = "Product.Read";
    public const string Create = "Product.Create";
    public const string Update = "Product.Update";
    public const string Delete = "Product.Delete";
    public const string Export = "Product.Export";
    public const string Import = "Product.Import";
    public const string ViewMenu = "Product.ViewMenu";
    public static readonly string[] All =
        [Read, Create, Update, Delete, Export, Import, ViewMenu];
}
