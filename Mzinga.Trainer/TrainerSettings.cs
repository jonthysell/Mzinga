// 
// TrainerSettings.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mzinga.Core.AI;

namespace Mzinga.Trainer
{
    public class TrainerSettings
    {
        public static double DefaultMix
        {
            get
            {
                return 0.05;
            }
        }

        public static int DefaultLifecycleGenerations
        {
            get
            {
                return 1;
            }
        }

        public static int DefaultLifecycleBattles
        {
            get
            {
                return 1;
            }
        }

        public static int DefaultMaxDraws
        {
            get
            {
                return 1;
            }
        }

        public static int CullMinKeepCount
        {
            get
            {
                return 4;
            }
        }

        public static int DefaultMaxDepth
        {
            get
            {
                return GameAI.IterativeDepth;
            }
        }

        public static bool DefaultUseAlphaBetaPruning
        {
            get
            {
                return true;
            }
        }

        public static bool DefaultUseTranspositionTable
        {
            get
            {
                return true;
            }
        }

        public static TimeSpan DefaultMaxTime
        {
            get
            {
                return TimeSpan.FromSeconds(5.0);
            }
        }

        public static TimeSpan DefaultBattleTimeLimit
        {
            get
            {
                return TimeSpan.FromMinutes(5.0);
            }
        }
    }
}
