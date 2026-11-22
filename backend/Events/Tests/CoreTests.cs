using API.Controllers;
using Application.Core;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Tests;

public class PagingParamsTests
{
    [Fact]
    public void Defaults_are_page_one_with_ten_items()
    {
        var paging = new PagingParams();

        Assert.Equal(1, paging.PageNumber);
        Assert.Equal(10, paging.PageSize);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-5, 10)]
    [InlineData(1, 1)]
    [InlineData(50, 50)]
    [InlineData(51, 50)]
    [InlineData(1000, 50)]
    public void Page_size_is_clamped(int requested, int expected) =>
        Assert.Equal(expected, new PagingParams { PageSize = requested }.PageSize);

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(7, 7)]
    public void Page_number_is_at_least_one(int requested, int expected) =>
        Assert.Equal(expected, new PagingParams { PageNumber = requested }.PageNumber);
}

public class PagedListTests
{
    [Fact]
    public void Calculates_the_number_of_pages_rounding_up()
    {
        var list = new PagedList<int>([3, 4], count: 5, pageNumber: 2, pageSize: 2);

        Assert.Equal(3, list.TotalPages);
        Assert.Equal(5, list.TotalCount);
        Assert.Equal(2, list.CurrentPage);
        Assert.Equal(new[] { 3, 4 }, list.ToArray());
    }

    [Fact]
    public void An_exact_multiple_does_not_add_an_extra_page() =>
        Assert.Equal(2, new PagedList<int>([1, 2], count: 4, pageNumber: 1, pageSize: 2).TotalPages);
}

public class ResultTests
{
    [Fact]
    public void Success_carries_the_value()
    {
        var result = Result<string>.Success("x");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsNotFound);
        Assert.Equal("x", result.Value);
    }

    [Fact]
    public void Failure_carries_the_error()
    {
        var result = Result<string>.Failure("bad");

        Assert.False(result.IsSuccess);
        Assert.False(result.IsNotFound);
        Assert.Equal("bad", result.Error);
    }

    [Fact]
    public void Not_found_is_a_failure_flagged_as_not_found()
    {
        var result = Result<string>.NotFound("missing");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
        Assert.Equal("missing", result.Error);
    }
}

public class HandleResultTests
{
    private sealed class TestController : BaseApiController
    {
        public ActionResult Handle<T>(Result<T>? result) => HandleResult(result);
    }

    private readonly TestController _controller = new();

    [Fact]
    public void Success_maps_to_200_with_the_value()
    {
        var ok = Assert.IsType<OkObjectResult>(_controller.Handle(Result<string>.Success("x")));

        Assert.Equal("x", ok.Value);
    }

    [Fact]
    public void Not_found_maps_to_404_with_the_message()
    {
        var notFound = Assert.IsType<NotFoundObjectResult>(_controller.Handle(Result<string>.NotFound("missing")));

        Assert.Equal("missing", notFound.Value);
    }

    [Fact]
    public void Failure_maps_to_400_with_the_message()
    {
        var bad = Assert.IsType<BadRequestObjectResult>(_controller.Handle(Result<string>.Failure("bad")));

        Assert.Equal("bad", bad.Value);
    }

    [Fact]
    public void A_successful_result_without_a_value_maps_to_404() =>
        Assert.IsType<NotFoundResult>(_controller.Handle(Result<string>.Success(null!)));

    [Fact]
    public void A_null_result_maps_to_404() =>
        Assert.IsType<NotFoundResult>(_controller.Handle<string>(null));
}