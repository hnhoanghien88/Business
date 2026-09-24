using Business.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Business.Api.Hubs;

[Authorize(Policy = TableOperationsPermissions.Read)]
public sealed class TableOperationsHub : Hub;

