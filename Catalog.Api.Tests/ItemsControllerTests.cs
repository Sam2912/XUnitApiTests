using System;
using System.Collections.Generic;
using Catalog.Api.Controllers;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Catalog.Api.Tests;

public class ItemsControllerTests
{
    private readonly Mock<IItemsRepository> repositoryStub = new();
    private readonly Mock<ILogger<ItemsController>> loggerStub = new();
    private readonly Random rand = new();

    [Fact]
    public async void GetItemAsync_WithUnexistingItem_ReturnsNotFound()
    {
        //Sub --fake version of instance/class

        //UnitOfWork_StateUnderTest_ExpectedBehavior
        //Arrange

        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
        .ReturnsAsync((Item)null);


        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        var result = await controller.GetItemAsync(Guid.NewGuid());

        //Assert
        //Assert.IsType<NotFoundResult>(result.Result);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async void GetItemAsync_WithExistingItem_ReturnsExpectedItem()
    {
        //Arrange
        var expectedItem = CreateRandomItem();

        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
        .ReturnsAsync(expectedItem);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        var result = await controller.GetItemAsync(Guid.NewGuid());

        //Assert
        result.Value.Should().BeEquivalentTo(expectedItem);


        // Assert.IsType<ItemDto>(result.Value);
        // var dto = (result as ActionResult<ItemDto>).Value;
        // Assert.Equal(expectedItem.Id, dto.Id);
    }

    [Fact]
    public async void GetItemsAsync_WithExistingItems_ReturnsAllItems()
    {
        //Arrange
        var expectedItems = new[]{
            CreateRandomItem(),
            CreateRandomItem(),
            CreateRandomItem()
        };

        repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(expectedItems);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        var actualItems = await controller.GetItemsAsync();

        //Assert
        actualItems.Should().BeEquivalentTo(expectedItems);
    }

    [Fact]
    public async void GetItemsAsync_WithMatchingItems_ReturnsMatchingItems()
    {
        //Arrange
        var allItems = new[]{
            new Item(){Name="Pratibha"},
            new Item(){Name="Pihu"},
            new Item(){Name="Jiya Pihu"}
        };

        var nameToMatch = "Pihu";

        repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(allItems);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        IEnumerable<ItemDto> foundItems = await controller.GetItemsAsync(nameToMatch);

        //Assert
        foundItems.Should().OnlyContain(item => item.Name == allItems[1].Name || item.Name == allItems[2].Name);
    }

    [Fact]
    public async void CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
    {
        //Arrange
        var itemToCreate = new CreateItemDto(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            rand.Next(1000)
        );

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        var result = await controller.CreateItemAsync(itemToCreate);

        //Assert
        var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;
        itemToCreate.Should().BeEquivalentTo(createdItem,
        options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers());

        createdItem.Id.Should().NotBeEmpty();
        createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1000));
    }

    [Fact]
    public async void UpdateItemAsync_WithExisingItem_ReturnsNoContent()
    {
        //Arrange
        var existingItem = CreateRandomItem();

        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
        .ReturnsAsync(existingItem);

        var itemId = existingItem.Id;
        var itemToUpdate = new UpdateItemDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), existingItem.Price + 3);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        var result = await controller.UpdateItemAsync(itemId, itemToUpdate);

        //Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async void DeleteItemAsync_WithExisingItem_ReturnsNoContent()
    {
        //Arrange
        var existingItem = CreateRandomItem();

        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
        .ReturnsAsync(existingItem);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //Act
        var result = await controller.DeleteItemAsync(existingItem.Id);

        //Assert
        result.Should().BeOfType<NoContentResult>();
    }

    private Item CreateRandomItem()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString(),
            Price = rand.Next(1000),
            CreatedDate = DateTimeOffset.UtcNow
        };
    }
}