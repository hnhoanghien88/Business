namespace Business.Application.Common.Authorization;

public static class FoodsPermissions
{
    public const string Read = "Foods.Read";
    public const string Create = "Foods.Create";
    public const string Update = "Foods.Update";
    public const string Delete = "Foods.Delete";
    public const string Export = "Foods.Export";
    public const string Import = "Foods.Import";
    public const string ViewMenu = "Foods.ViewMenu";
    public static readonly string[] All =
        [Read, Create, Update, Delete, Export, Import, ViewMenu];
}
