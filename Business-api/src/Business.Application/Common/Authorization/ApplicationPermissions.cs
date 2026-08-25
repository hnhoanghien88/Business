namespace Business.Application.Common.Authorization;

public static class ApplicationPermissions
{
    public const string Read = "Applications.Read";
    public const string Create = "Applications.Create";
    public const string Update = "Applications.Update";
    public const string Delete = "Applications.Delete";
    public static readonly string[] All = [Read, Create, Update, Delete];
}
