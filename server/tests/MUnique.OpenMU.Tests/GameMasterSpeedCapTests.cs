// <copyright file="GameMasterSpeedCapTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Tests for the OriginSix speed cap of game masters: their attack and magic speed are capped at 300,
/// the other players' at the configured maximum (200).
/// </summary>
[TestFixture]
public class GameMasterSpeedCapTests
{
    private const float HighSpeed = 500;

    /// <summary>
    /// Tests that a normal player's speeds are capped at the configured maximum.
    /// </summary>
    [Test]
    public async ValueTask NormalPlayerSpeedsAreCappedAtTheConfiguredMaximumAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var character = player.SelectedCharacter!;
        character.CharacterStatus = CharacterStatus.Normal;

        using var attributeSystem = new ItemAwareAttributeSystem(player.Account!, character, player.GameContext.Configuration);
        AddSpeed(attributeSystem);

        Assert.That(attributeSystem[Stats.AttackSpeed], Is.EqualTo(Stats.AttackSpeed.MaximumValue));
        Assert.That(attributeSystem[Stats.MagicSpeed], Is.EqualTo(Stats.MagicSpeed.MaximumValue));
    }

    /// <summary>
    /// Tests that a game master's speeds are capped at 300.
    /// </summary>
    [Test]
    public async ValueTask GameMasterSpeedsAreCappedAt300Async()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var character = player.SelectedCharacter!;
        character.CharacterStatus = CharacterStatus.GameMaster;

        using var attributeSystem = new ItemAwareAttributeSystem(player.Account!, character, player.GameContext.Configuration);
        AddSpeed(attributeSystem);

        Assert.That(attributeSystem[Stats.AttackSpeed], Is.EqualTo(300));
        Assert.That(attributeSystem[Stats.MagicSpeed], Is.EqualTo(300));
    }

    private static void AddSpeed(ItemAwareAttributeSystem attributeSystem)
    {
        ((IComposableAttribute)attributeSystem.GetOrCreateAttribute(Stats.AttackSpeed)).AddElement(new ConstantElement(HighSpeed));
        ((IComposableAttribute)attributeSystem.GetOrCreateAttribute(Stats.MagicSpeed)).AddElement(new ConstantElement(HighSpeed));
    }
}
