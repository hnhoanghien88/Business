namespace Business.Application.Common.Authorization;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
}

public static class RoleGroups
{
    public const string All = $"{RoleNames.Admin},{RoleNames.Manager},{RoleNames.Employee}";
    public const string Management = $"{RoleNames.Admin},{RoleNames.Manager}";
}
