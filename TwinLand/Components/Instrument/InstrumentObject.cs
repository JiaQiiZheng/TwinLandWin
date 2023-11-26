using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinLand.Components.Instrument.Land_Brush;
using TwinLand.Components.Instrument.Planar_Emitter;

namespace TwinLand.Components.Instrument
{
    public class InstrumentObject
    {
        public PlanarPointEmitter PlanarPointEmitter { get; set; }
        public LandBrush LandBrush { get; set; }

        
        // Constructors
        public InstrumentObject()
        {
            PlanarPointEmitter = null;
            LandBrush = null;
        }

        public InstrumentObject(PlanarPointEmitter ppe)
        {
            PlanarPointEmitter = ppe;
        }

        public InstrumentObject(LandBrush lb)
        {
            LandBrush = lb;
        }
    }
}
