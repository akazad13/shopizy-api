using System.Net;
using Shopizy.Contracts.Category;
using Shouldly;
using Xunit;

namespace Shopizy.Api.IntegrationTests.Categories;

public class CategoryTreeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetCategoryTree_Anonymous_ReturnsOk()
    {
        // Arrange — no authentication needed
        ClearAuthToken();

        // Act
        var response = await HttpClient.GetAsync(
            "/api/v1.0/categories/tree",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tree = await response.Content.ReadFromJsonAsync<List<CategoryTreeResponse>>(
            TestContext.Current.CancellationToken
        );
        tree.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetCategoryTree_AfterCreatingParentAndChild_ReturnsHierarchy()
    {
        // Arrange — create a parent and child category as admin
        await AuthenticateAsAdminAsync();

        var parentName = $"Tree Parent {Guid.NewGuid().ToString()[..4]}";
        var parentResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest(parentName, null),
            TestContext.Current.CancellationToken
        );
        parentResponse.EnsureSuccessStatusCode();
        var parent = await parentResponse.Content.ReadFromJsonAsync<CategoryResponse>(
            TestContext.Current.CancellationToken
        );

        var childName = $"Tree Child {Guid.NewGuid().ToString()[..4]}";
        var childResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest(childName, parent!.Id),
            TestContext.Current.CancellationToken
        );
        childResponse.EnsureSuccessStatusCode();
        var child = await childResponse.Content.ReadFromJsonAsync<CategoryResponse>(
            TestContext.Current.CancellationToken
        );

        // Browse as anonymous
        ClearAuthToken();

        // Act
        var treeResponse = await HttpClient.GetAsync(
            "/api/v1.0/categories/tree",
            TestContext.Current.CancellationToken
        );

        // Assert
        treeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tree = await treeResponse.Content.ReadFromJsonAsync<List<CategoryTreeResponse>>(
            TestContext.Current.CancellationToken
        );
        tree.ShouldNotBeNull();

        var parentNode = tree.FirstOrDefault(c => c.Id == parent.Id);
        parentNode.ShouldNotBeNull();
        parentNode.Name.ShouldBe(parentName);
        parentNode.Children.ShouldNotBeNull();
        parentNode.Children.ShouldContain(c => c.Id == child!.Id && c.Name == childName);
    }

    [Fact]
    public async Task GetCategoryTree_WithMultipleRoots_ReturnsAllRoots()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var root1Name = $"Root1 {Guid.NewGuid().ToString()[..4]}";
        var root2Name = $"Root2 {Guid.NewGuid().ToString()[..4]}";

        await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest(root1Name, null),
            TestContext.Current.CancellationToken
        );
        await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest(root2Name, null),
            TestContext.Current.CancellationToken
        );

        ClearAuthToken();

        // Act
        var response = await HttpClient.GetAsync(
            "/api/v1.0/categories/tree",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tree = await response.Content.ReadFromJsonAsync<List<CategoryTreeResponse>>(
            TestContext.Current.CancellationToken
        );
        tree.ShouldNotBeNull();
        tree.ShouldContain(c => c.Name == root1Name);
        tree.ShouldContain(c => c.Name == root2Name);
    }

    [Fact]
    public async Task GetCategoryList_Anonymous_ReturnsAllCategories()
    {
        // Arrange
        ClearAuthToken();

        // Act
        var response = await HttpClient.GetAsync(
            "/api/v1.0/categories",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>(
            TestContext.Current.CancellationToken
        );
        categories.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetCategory_WithValidId_ReturnsCategory()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        var catName = $"SingleGet {Guid.NewGuid().ToString()[..4]}";
        var createResponse = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest(catName, null),
            TestContext.Current.CancellationToken
        );
        var category = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>(
            TestContext.Current.CancellationToken
        );

        ClearAuthToken();

        // Act
        var response = await HttpClient.GetAsync(
            $"/api/v1.0/categories/{category!.Id}",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<CategoryResponse>(
            TestContext.Current.CancellationToken
        );
        fetched.ShouldNotBeNull();
        fetched.Id.ShouldBe(category.Id);
        fetched.Name.ShouldBe(catName);
    }

    [Fact]
    public async Task GetCategory_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        ClearAuthToken();

        // Act
        var response = await HttpClient.GetAsync(
            $"/api/v1.0/categories/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_WithParentId_SetsParentCorrectly()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var parentResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest($"ParentCat {Guid.NewGuid().ToString()[..4]}", null),
            TestContext.Current.CancellationToken
        );
        var parent = await parentResp.Content.ReadFromJsonAsync<CategoryResponse>(
            TestContext.Current.CancellationToken
        );

        // Act
        var childResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest($"ChildCat {Guid.NewGuid().ToString()[..4]}", parent!.Id),
            TestContext.Current.CancellationToken
        );

        // Assert
        childResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var child = await childResp.Content.ReadFromJsonAsync<CategoryResponse>(
            TestContext.Current.CancellationToken
        );
        child.ShouldNotBeNull();
        child.ParentId.ShouldBe(parent.Id);
    }

    [Fact]
    public async Task GetCategoryTree_ReflectsNewCategory_ImmediatelyAfterCreation()
    {
        // Arrange — fetch tree first to populate cache
        ClearAuthToken();
        var initialTreeResp = await HttpClient.GetAsync(
            "/api/v1.0/categories/tree",
            TestContext.Current.CancellationToken
        );
        initialTreeResp.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act — create a new root category as admin
        await AuthenticateAsAdminAsync();
        var newCatName = $"CacheInvalidationTest_{Guid.NewGuid().ToString()[..4]}";
        var createResp = await HttpClient.PostAsJsonAsync(
            "/api/v1.0/admin/categories",
            new CreateCategoryRequest(newCatName, null),
            TestContext.Current.CancellationToken
        );
        createResp.EnsureSuccessStatusCode();

        // Act — fetch tree again as anonymous
        ClearAuthToken();
        var updatedTreeResp = await HttpClient.GetAsync(
            "/api/v1.0/categories/tree",
            TestContext.Current.CancellationToken
        );

        // Assert — tree cache should have been invalidated and reflect the newly created category
        updatedTreeResp.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tree = await updatedTreeResp.Content.ReadFromJsonAsync<List<CategoryTreeResponse>>(
            TestContext.Current.CancellationToken
        );
        tree.ShouldNotBeNull();
        tree.ShouldContain(c => c.Name == newCatName);
    }
}
