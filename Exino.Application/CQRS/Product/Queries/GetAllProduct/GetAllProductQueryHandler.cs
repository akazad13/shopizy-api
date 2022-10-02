﻿using AutoMapper;
using Exino.Application.Common.Wrappers;
using Exino.Application.RepositoriesInterface;
using Exino.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exino.Application.CQRS.Product.Queries.GetAllProduct
{
    public class GetAllProductQueryHandler
        : IRequestHandler<GetAllProductQueryRequest, IResult<IPaginate<GetAllProductQueryResponse>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetAllProductQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IResult<IPaginate<GetAllProductQueryResponse>>> Handle(
            GetAllProductQueryRequest request,
            CancellationToken cancellationToken
        )
        {
            var products = await _productRepository.GetFilteredList(
                selector: x =>
                    new GetAllProductQueryResponse
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Price = x.Price,
                        Size = x.Size,
                        Color = x.Color,
                        ThumbnailImagePath =
                            x.ProductImages == null
                                ? null
                                : (
                                    x.ProductImages.Count(p => p.IsThumbnail == true) > 0
                                        ? null
                                        : x.ProductImages
                                            .First(p => p.IsThumbnail == true)
                                            .ImagePath
                                ),
                        Stock = x.Stock,
                        CategoryName = x.Category == null ? null : x.Category.Name
                    },
                expression: x => x.Status != Status.Deactive,
                orderBy: x => x.OrderBy(x => x.Name),
                include: x => x.Include(x => x.Category),
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                disableTracing: true,
                cancellationToken: cancellationToken
            );

            return Response<IPaginate<GetAllProductQueryResponse>>.SuccessResponese(products);
        }
    }
}
