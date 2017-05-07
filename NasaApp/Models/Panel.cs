using System;

namespace NasaApp.Models
{
    public class Panel
    {
        public int ID { get; set; }
        public int NetworkID { get; set; }
        public PanelModuleType ModuleType { get; set; }
        public PanelArrayType ArrayType { get; set; }
        public double? TiltAngle { get; set; }
        // The system losses account for performance losses you would expect in a real system.
        public double SystemLosses { get; set; }
        // The power rating of the photovoltaic array in kilowatts (kW).
        public double PowerRating { get; set; }
        // The number of panels of this type.
        public int Count { get; set; }
    }

    public enum PanelModuleType
    {
        Monocrystalline = 0,
        Polycrystalline = 1,
        ThinFilm = 2,
    }

    public enum PanelArrayType
    {
        FixedOpenRack = 0,
        FixedRoofMounted = 1,
        OneAxis = 2,
        OneAxisBacktracking = 3,
        TwoAxis = 4
    }
}