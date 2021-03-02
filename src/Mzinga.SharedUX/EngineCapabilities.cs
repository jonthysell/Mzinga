// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Mzinga.Core;

namespace Mzinga.SharedUX
{
    public class EngineCapabilities
    {
        public ExpansionPieces ExpansionPieces { get; private set; } = ExpansionPieces.None;

        public EngineCapabilities(string capabilitiesString)
        {
            if (!string.IsNullOrWhiteSpace(capabilitiesString))
            {
                foreach (string capabilityString in capabilitiesString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        switch (capabilityString.ToLower())
                        {
                            case "mosquito":
                                ExpansionPieces |= ExpansionPieces.Mosquito;
                                break;
                            case "ladybug":
                                ExpansionPieces |= ExpansionPieces.Ladybug;
                                break;
                            case "pillbug":
                                ExpansionPieces |= ExpansionPieces.Pillbug;
                                break;
                        }                       
                    }
                    catch (Exception) { }
                }
            }
        }
    }
}
