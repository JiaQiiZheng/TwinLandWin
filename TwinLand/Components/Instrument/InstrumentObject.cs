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
        public PlanarEmitter PlanarEmitter { get; set; }
        public LandBrush LandBrush { get; set; }
        // For instrument to switch materials

        
        // Constructors
        public InstrumentObject()
        {
            PlanarEmitter = null;
            LandBrush = null;
        }

        public InstrumentObject(PlanarEmitter ppe)
        {
            PlanarEmitter = ppe;
        }

        public InstrumentObject(LandBrush lb)
        {
            LandBrush = lb;
        }
    }
}
