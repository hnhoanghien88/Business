namespace Business.Application.Common.Authorization;

public static class ResourcePermissions
{
    public const string Read = "Resources.Read";
    public const string Create = "Resources.Create";
    public const string Update = "Resources.Update";
    public const string Delete = "Resources.Delete";
    public static readonly string[] All = [Read, Create, Update, Delete];
}
