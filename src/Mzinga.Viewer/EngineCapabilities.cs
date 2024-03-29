﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Mzinga.Core;

namespace Mzinga.Viewer
{
    public class EngineCapabilities
    {
        public readonly bool Mosquito = false;
        public readonly bool Ladybug = false;
        public readonly bool Pillbug = false;

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
                                Mosquito = true;
                                break;
                            case "ladybug":
                                Ladybug = true;
                                break;
                            case "pillbug":
                                Pillbug = true;
                                break;
                        }                       
                    }
                    catch (Exception) { }
                }
            }
        }

        public bool CanPlayGameType(GameType gameType)
        {
            bool mosquitoCheck = !Enums.BugTypeIsEnabledForGameType(BugType.Mosquito, gameType) || Mosquito;
            bool ladybugCheck = !Enums.BugTypeIsEnabledForGameType(BugType.Ladybug, gameType) || Ladybug;
            bool pillbugCheck = !Enums.BugTypeIsEnabledForGameType(BugType.Pillbug, gameType) || Pillbug;

            return mosquitoCheck && ladybugCheck && pillbugCheck;
        }
    }
}
