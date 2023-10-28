/*
Ren Bot is a discord bot with some silly features included.
Copyright (C) 2023 - kingoworld

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;

namespace Vdcrpt.Desktop;

public class Preset
{
    public string Name { get; init; } = string.Empty;
    public int BurstSize { get; init; }
    public int Iterations { get; init; }

    // TODO: Range was tacked on at the last minute, this can be
    // represented more succinctly. Some of this is going to get blown up
    // for user presets anyway, so it's fine for now.
    //
    // When not using range, MinBurstLength is the constant burst length.
    public bool UseLengthRange { get; init; }
    public int MinBurstLength { get; init; }
    public int MaxBurstLength { get; init; }

    public static List<Preset> DefaultPresets => new()
    {
        new Preset { Name = "Melting Chaos", BurstSize = 3000, MinBurstLength = 8, Iterations = 400 },
        new Preset
        {
            Name = "Jittery", BurstSize = 20000, MinBurstLength = 1, MaxBurstLength = 8,
            UseLengthRange = true, Iterations = 200
        },
        new Preset
        {
            Name = "Source Engine", BurstSize = 45000, MinBurstLength = 2, MaxBurstLength = 6,
            UseLengthRange = true, Iterations = 60
        },
        new Preset { Name = "Subtle", BurstSize = 200, MinBurstLength = 2, Iterations = 60 },
        new Preset { Name = "Many Artifacts", BurstSize = 500, MinBurstLength = 3, Iterations = 2000 },
        new Preset
        {
            Name = "Trash (unstable, breaks audio)", BurstSize = 1, MinBurstLength = 1, Iterations = 10000
        },
        new Preset
        {
            Name = "Legacy", BurstSize = 1000, MinBurstLength = 10, MaxBurstLength = 90,
            UseLengthRange = true, Iterations = 50
        },
    };
}