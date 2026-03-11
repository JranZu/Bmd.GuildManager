using Bmd.GuildManager.Core.Models;
using Bmd.GuildManager.Functions.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bmd.GuildManager.Tests.Functions;

public class GetCharactersFunctionTests
{
    private static Character BuildCharacter(Guid playerId) =>
        Character.CreateWithId(
            characterId: Guid.NewGuid(),
            playerId: playerId,
            name: "Aldric",
            level: 1,
            strength: 8,
            luck: 6,
            endurance: 7);

    [Fact]
    public async Task RunAsync_PlayerWithCharacters_ReturnsOkWithRoster()
    {
        var playerId = Guid.NewGuid();
        var repository = new FakeCharacterRepository();
        repository.Characters.Add(BuildCharacter(playerId));
        repository.Characters.Add(BuildCharacter(playerId));

        var function = new GetCharactersFunction(
            repository,
            NullLogger<GetCharactersFunction>.Instance);

        var request = new DefaultHttpContext().Request;
        var result = await function.RunAsync(request, playerId) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var characters = result.Value as IReadOnlyList<Character>;
        Assert.NotNull(characters);
        Assert.Equal(2, characters.Count);
    }

    [Fact]
    public async Task RunAsync_PlayerWithNoCharacters_ReturnsOkWithEmptyList()
    {
        var repository = new FakeCharacterRepository();
        var function = new GetCharactersFunction(
            repository,
            NullLogger<GetCharactersFunction>.Instance);

        var request = new DefaultHttpContext().Request;
        var result = await function.RunAsync(request, Guid.NewGuid()) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var characters = result.Value as IReadOnlyList<Character>;
        Assert.NotNull(characters);
        Assert.Empty(characters);
    }

    [Fact]
    public async Task RunAsync_OnlyReturnsCharactersForRequestedPlayer()
    {
        var playerId = Guid.NewGuid();
        var otherPlayerId = Guid.NewGuid();
        var repository = new FakeCharacterRepository();
        repository.Characters.Add(BuildCharacter(playerId));
        repository.Characters.Add(BuildCharacter(otherPlayerId));

        var function = new GetCharactersFunction(
            repository,
            NullLogger<GetCharactersFunction>.Instance);

        var request = new DefaultHttpContext().Request;
        var result = await function.RunAsync(request, playerId) as OkObjectResult;

        var characters = result!.Value as IReadOnlyList<Character>;
        Assert.NotNull(characters);
        Assert.Single(characters);
        Assert.Equal(playerId, characters[0].PlayerId);
    }
}
