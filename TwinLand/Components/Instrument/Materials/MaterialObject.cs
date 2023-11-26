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


        public MaterialObject(SolidParticle sp)
        {
            SolidParticle = sp;
        }

        public enum MaterialType
        {
            SolidParticle
        }
    }
}
