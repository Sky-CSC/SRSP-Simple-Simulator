﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRace
{
    /// <summary>
    /// This class contains the compression factor and the tick duration of the simulation
    /// </summary>
    public class Time
    {
        float compressionFactor = 1;

        int tick = 1000;

        public float GetAccFactorValue()
        {
            return compressionFactor;
        }

        public void SetAccFactor(float factor)
        {
            this.compressionFactor = factor;
        }

        public float GetTickValue()
        {
            return tick;
        }

        public void SetTick(int factor)
        {
            this.tick = factor;
        }
    }
}
