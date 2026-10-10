using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Common.CustomErrors;
using Shopizy.Domain.Products.Entities;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Queries.GetProductVariants;

public class GetProductVariantsQueryHandler(IProductReader productReader)
    : IQueryHandler<GetProductVariantsQuery, ErrorOr<IReadOnlyList<ProductVariant>>>
{
    private readonly IProductReader _productReader = productReader;

    public async Task<ErrorOr<IReadOnlyList<ProductVariant>>> Handle(
        GetProductVariantsQuery query,
        CancellationToken cancellationToken
    )
    {
        var variants = await _productReader.GetProductVariantsAsync(
            ProductId.Create(query.ProductId),
            cancellationToken
        );

        if (variants is null)
        {
            return (Error)CustomErrors.Product.ProductNotFound;
        }

        return ErrorOrFactory.From(variants);
    }
}
