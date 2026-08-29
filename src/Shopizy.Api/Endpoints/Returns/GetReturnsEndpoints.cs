using System.Security.Claims;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Api.Common.Extensions;
using Shopizy.Api.Common.LoggerMessages;
using Shopizy.Application.Returns.Queries.GetPendingReturns;
using Shopizy.Application.Returns.Queries.GetReturnById;
using Shopizy.Application.Returns.Queries.GetReturnsByOrder;
using Shopizy.Contracts.Common;
using Shopizy.Contracts.Returns;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Api.Endpoints.Returns;

public class GetReturnByIdEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/returns/{returnId:guid}",
                async (
                    Guid returnId,
                    ClaimsPrincipal user,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetReturnByIdEndpoint> logger
                ) =>
                {
                    var query = new GetReturnByIdQuery(returnId);
                    return await HandleAsync(
                        mediator,
                        query,
                        rr =>
                        {
                            if (!user.IsInRole("Admin") && !user.IsAuthorized(rr.UserId.Value))
                            {
                                return CustomResults.Problem([
                                    ErrorOr.Error.Forbidden(
                                        description: "You are not authorized to view this return request."
                                    ),
                                ]);
                            }

                            return Results.Ok(mapper.Map<ReturnRequestDto>(rr));
                        },
                        ex => logger.ReturnFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("Order.Read")
            .WithTags("Returns")
            .WithSummary("Get a return request by ID")
            .Produces<ReturnRequestDto>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status404NotFound)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}

public class GetReturnsByOrderEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/orders/{orderId:guid}/returns",
                async (
                    Guid orderId,
                    ClaimsPrincipal user,
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetReturnsByOrderEndpoint> logger
                ) =>
                {
                    var query = new GetReturnsByOrderQuery(orderId);
                    return await HandleAsync(
                        mediator,
                        query,
                        returns =>
                        {
                            if (
                                !user.IsInRole("Admin")
                                && returns.Any(r => !user.IsAuthorized(r.UserId.Value))
                            )
                            {
                                return CustomResults.Problem([
                                    ErrorOr.Error.Forbidden(
                                        description: "You are not authorized to view returns for this order."
                                    ),
                                ]);
                            }

                            return Results.Ok(mapper.Map<IReadOnlyList<ReturnRequestDto>>(returns));
                        },
                        ex => logger.ReturnFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("Order.Read")
            .WithTags("Returns")
            .WithSummary("Get all returns for an order")
            .Produces<IReadOnlyList<ReturnRequestDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}

public class GetPendingReturnsEndpoint : ApiEndpoint
{
    public override void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet(
                "api/v1.0/returns/pending",
                async (
                    [FromServices] IDispatcher mediator,
                    IMapper mapper,
                    ILogger<GetPendingReturnsEndpoint> logger
                ) =>
                {
                    var query = new GetPendingReturnsQuery();
                    return await HandleAsync(
                        mediator,
                        query,
                        returns => Results.Ok(mapper.Map<IReadOnlyList<ReturnRequestDto>>(returns)),
                        ex => logger.ReturnFetchError(ex)
                    );
                }
            )
            .RequireAuthorization("Order.Manage")
            .WithTags("Returns")
            .WithSummary("Get all pending return requests (Admin)")
            .Produces<IReadOnlyList<ReturnRequestDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResult>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResult>(StatusCodes.Status500InternalServerError);
}
