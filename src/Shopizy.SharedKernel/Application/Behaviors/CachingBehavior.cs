using ErrorOr;
using Microsoft.Extensions.Logging;
using Shopizy.SharedKernel.Application.Caching;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.SharedKernel.Application.Behaviors;

public class CachingQueryHandlerDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> innerHandler,
    ICacheHelper cacheHelper,
    ILogger<CachingQueryHandlerDecorator<TQuery, TResponse>> logger
) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken = default)
    {
        if (query is not ICachableRequest cachableRequest)
        {
            return await innerHandler.Handle(query, cancellationToken);
        }

        logger.LogCheckingCache(typeof(TQuery).Name, cachableRequest.CacheKey);

        var cacheResult = await cacheHelper.GetAsync<TResponse>(cachableRequest.CacheKey);
        if (cacheResult.Success)
        {
            logger.LogCacheHit(typeof(TQuery).Name, cachableRequest.CacheKey);
            return cacheResult.Value!;
        }

        logger.LogCacheMiss(typeof(TQuery).Name, cachableRequest.CacheKey);
        var response = await innerHandler.Handle(query, cancellationToken);

        if (response is not IErrorOr errorOr || !errorOr.IsError)
        {
            await cacheHelper.SetAsync(
                cachableRequest.CacheKey,
                response,
                cachableRequest.Expiration
            );
        }

        return response;
    }
}
