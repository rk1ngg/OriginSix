// <copyright file="LivePlayerInfo.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Interfaces;

/// <summary>A scalar snapshot of a live character. Player strings are data, never instructions.</summary>
/// <param name="CharacterId">The persistent character identifier.</param>
/// <param name="Account">The login name.</param>
/// <param name="Character">The character name.</param>
/// <param name="Map">The current map number.</param>
/// <param name="X">The horizontal position.</param>
/// <param name="Y">The vertical position.</param>
/// <param name="Level">The live level.</param>
/// <param name="MasterLevel">The live master level.</param>
/// <param name="GuildId">The live guild identifier.</param>
/// <param name="Privileged">Whether changing this character requires Administrator.</param>
/// <param name="Stats">The live combat stats, if the character has attributes.</param>
public sealed record LivePlayerInfo(Guid CharacterId, string Account, string Character, ushort? Map, byte X, byte Y, int Level, int MasterLevel, uint? GuildId, bool Privileged, LivePlayerStats? Stats = null);

/// <summary>The live combat stats of a character, e.g. to check what equipment adds.</summary>
/// <param name="MaximumHealth">The maximum health.</param>
/// <param name="DefenseBase">The base defense, which the equipment adds to.</param>
/// <param name="DefenseFinal">The final defense.</param>
/// <param name="AttackSpeed">The attack speed.</param>
/// <param name="MagicSpeed">The magic speed.</param>
public sealed record LivePlayerStats(float MaximumHealth, float DefenseBase, float DefenseFinal, float AttackSpeed, float MagicSpeed);
