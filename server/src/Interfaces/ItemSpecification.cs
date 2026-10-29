// <copyright file="ItemSpecification.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Interfaces;

/// <summary>A structured item request, independent of chat command parsing and persistence.</summary>
/// <param name="Group">The item group.</param>
/// <param name="Number">The item number within the group.</param>
/// <param name="Level">The item level.</param>
/// <param name="Skill">Whether to include the item's skill.</param>
/// <param name="Luck">Whether to include luck.</param>
/// <param name="OptionLevel">The additional option level or fixed-option mask.</param>
/// <param name="ExcellentOptions">The excellent option bit mask.</param>
/// <param name="AncientSet">The ancient set discriminator, or zero.</param>
/// <param name="AncientBonusLevel">The ancient bonus level.</param>
/// <param name="Sockets">The seeds mounted in the item's sockets; unlisted sockets stay empty.</param>
/// <param name="SocketBonusOption">The socket bonus option number, or <c>null</c> for none.</param>
/// <param name="HarmonyOption">The Jewel of Harmony option number, or zero for none.</param>
/// <param name="HarmonyLevel">The Jewel of Harmony option level.</param>
/// <param name="GuardianOption">Whether to add the level 380 (guardian) option.</param>
/// <param name="Durability">The durability, or <c>null</c> for the definition's default.</param>
/// <param name="Quantity">The number of identical items to create.</param>
/// <param name="Force">Whether to skip the definition's limits (levels, option ranges, socket elements). Administrator only.</param>
public sealed record ItemSpecification(
    byte Group,
    short Number,
    byte Level = 0,
    bool Skill = false,
    bool Luck = false,
    byte OptionLevel = 0,
    byte ExcellentOptions = 0,
    byte AncientSet = 0,
    byte AncientBonusLevel = 1,
    IReadOnlyList<SocketSpecification>? Sockets = null,
    byte? SocketBonusOption = null,
    byte HarmonyOption = 0,
    byte HarmonyLevel = 0,
    bool GuardianOption = false,
    byte? Durability = null,
    byte Quantity = 1,
    bool Force = false);

/// <summary>A seed mounted in one socket of an item.</summary>
/// <param name="Index">The socket index, starting at zero.</param>
/// <param name="Element">The seed element: Fire, Water, Ice, Wind, Lightning or Earth.</param>
/// <param name="Option">The seed option number within its element.</param>
/// <param name="Level">The seed sphere level, from 1 to 5.</param>
public sealed record SocketSpecification(byte Index, string Element, byte Option, byte Level);
