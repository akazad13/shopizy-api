using ErrorOr;
using Shopizy.Application.Common.Interfaces.Persistence;
using Shopizy.Domain.Brands.ValueObjects;
using Shopizy.Domain.Categories.ValueObjects;
using Shopizy.Domain.Products.ValueObjects;
using Shopizy.SharedKernel.Application.Messaging;

namespace Shopizy.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IProductReader productReader)
    : IQueryHandler<GetProductsQuery, ErrorOr<ProductsResult>>
{
    private readonly IProductReader _productReader = productReader;

    public async Task<ErrorOr<ProductsResult>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.PageNumber <= 0)
        {
            query = query with { PageNumber = 1 };
        }

        if (query.PageSize <= 0)
        {
            query = query with { PageSize = 10 };
        }

        var categoryIds =
            query.CategoryIds?.Any() == true
                ? query.CategoryIds.Select(categoryId => CategoryId.Create(categoryId)).ToList()
                : null;

        var productIds =
            query.ProductIds?.Any() == true
                ? query.ProductIds.Select(productId => ProductId.Create(productId)).ToList()
                : null;

        var brandIds =
            query.BrandIds?.Any() == true
                ? query.BrandIds.Select(brandId => BrandId.Create(brandId)).ToList()
                : null;

        var criteria = new ProductSearchCriteria(
            productIds,
            query.Name,
            categoryIds,
            brandIds,
            query.AverageRating,
            query.MinPrice,
            query.MaxPrice,
            query.InStockOnly,
            query.SortBy,
            query.PageNumber,
            query.PageSize
        );

        var (products, totalCount) = await _productReader.GetProductsWithCountAsync(
            criteria,
            cancellationToken
        );

        return new ProductsResult(
            products.ToList(),
            totalCount,
            (int)Math.Ceiling(totalCount / (1.0 * query.PageSize)),
            query.PageNumber
        );
    }
}
