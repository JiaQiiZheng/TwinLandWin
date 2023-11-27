using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwinLand.Components.Instrument.Materials
{
    public class MaterialObject
    {
        public SolidParticle SolidParticle { get; set; }
        public FluidParticle FluidParticle { get; set; }


        public MaterialObject()
        {
            SolidParticle = null;
            FluidParticle = null;
        }
        public MaterialObject(SolidParticle sp)
        {
            SolidParticle = sp;
        }
        public MaterialObject(FluidParticle fp)
        {
            FluidParticle = fp;
        }

        public enum MaterialType
        {
            SolidParticle,
            FluidParticle
        }
    }
}
